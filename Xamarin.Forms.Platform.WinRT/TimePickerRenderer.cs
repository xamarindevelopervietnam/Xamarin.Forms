﻿using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, Windows.UI.Xaml.Controls.TimePicker>
	{
		Brush _defaultBrush;

		IVisualElementController VisualElementController => Element;

		protected override void Dispose(bool disposing)
		{
			if (disposing && Control != null)
			{
				Control.TimeChanged -= OnControlTimeChanged;
				Control.Loaded -= ControlOnLoaded;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var picker = new Windows.UI.Xaml.Controls.TimePicker();
					SetNativeControl(picker);

					Control.TimeChanged += OnControlTimeChanged;
					Control.Loaded += ControlOnLoaded;
				}

				UpdateTime();
				UpdateFlowDirection();
			}
		}

		void ControlOnLoaded(object sender, RoutedEventArgs routedEventArgs)
		{
			// The defaults from the control template won't be available
			// right away; we have to wait until after the template has been applied
			_defaultBrush = Control.Foreground;
			UpdateTextColor();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName)
				UpdateTime();

			if (e.PropertyName == TimePicker.TextColorProperty.PropertyName)
				UpdateTextColor();

			if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlTimeChanged(object sender, TimePickerValueChangedEventArgs e)
		{
			Element.Time = e.NewTime;
			((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.SizeRequestChanged);
		}

		void UpdateFlowDirection()
		{
			if (VisualElementController == null || Control == null)
				return;

			if (VisualElementController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.RightToLeft))
				Control.FlowDirection = WFlowDirection.RightToLeft;
			else if (VisualElementController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.LeftToRight))
				Control.FlowDirection = WFlowDirection.LeftToRight;
		}

		void UpdateTime()
		{
			Control.Time = Element.Time;
		}

		void UpdateTextColor()
		{
			Color color = Element.TextColor;
			Control.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}
	}
}