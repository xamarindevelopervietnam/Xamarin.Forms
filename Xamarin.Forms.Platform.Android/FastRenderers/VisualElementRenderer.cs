using System;
using System.ComponentModel;
using Android.OS;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	// TODO hartez 2017/03/03 14:11:17 It's weird that this class is called VisualElementRenderer but it doesn't implement that interface. The name should probably be different.
	internal sealed class VisualElementRenderer : IDisposable, IEffectControlProvider
	{
		bool _disposed;
		
		IVisualElementRenderer _renderer;
		readonly GestureManager _gestureManager;
		readonly AutomationPropertiesProvider _automationPropertiesProvider;
		readonly EffectControlProvider _effectControlProvider;

		public VisualElementRenderer(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
			_renderer.ElementPropertyChanged += OnElementPropertyChanged;
			_renderer.ElementChanged += OnElementChanged;
			_gestureManager = new GestureManager(_renderer);
			_automationPropertiesProvider = new AutomationPropertiesProvider(_renderer);

			_effectControlProvider = new EffectControlProvider(_renderer?.View);
		}

		VisualElement Element => _renderer?.Element;

		IVisualElementController VisualElementController => Element as VisualElement;
		
		AView Control => _renderer?.View;

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			_effectControlProvider.RegisterEffect(effect);
		}

		public void UpdateBackgroundColor(Color? color = null)
		{		
			if (_disposed || Element == null || Control == null)
				return;

			Control.SetBackgroundColor((color ?? Element.BackgroundColor).ToAndroid());
		}

		public void UpdateFlowDirection()
		{
			if (_disposed || VisualElementController == null || Control == null || (int)Build.VERSION.SdkInt < 17)
				return;

			if (VisualElementController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.RightToLeft))
				Control.LayoutDirection = LayoutDirection.Rtl;
			else if (VisualElementController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.LeftToRight))
				Control.LayoutDirection = LayoutDirection.Ltr;
		}

	    public bool OnTouchEvent(MotionEvent e)
	    {
	        return _gestureManager.OnTouchEvent(e);
	    }

	    public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_gestureManager?.Dispose();
				_automationPropertiesProvider?.Dispose();

				if (_renderer != null)
				{
					_renderer.ElementChanged -= OnElementChanged;
					_renderer.ElementPropertyChanged -= OnElementPropertyChanged;
					_renderer = null;
				}
			}
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
		{
			if (e.OldElement != null)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
			}

			if (e.NewElement != null)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				UpdateBackgroundColor();
				UpdateFlowDirection();
			}

			EffectUtilities.RegisterEffectControlProvider(this, e.OldElement, e.NewElement);
		}

		void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}
	}
}
