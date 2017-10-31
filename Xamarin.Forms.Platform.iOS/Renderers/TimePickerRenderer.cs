using System;
using System.ComponentModel;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class TimePickerRenderer : ViewRenderer<TimePicker, UITextField>
	{
		UIDatePicker _picker;
		UIColor _defaultTextColor;
		bool _disposed;

		IElementController ElementController => Element as IElementController;
		IViewController ElementViewController => Element;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				_defaultTextColor = null;

				if (_picker != null)
				{
					_picker.RemoveFromSuperview();
					_picker.ValueChanged -= OnValueChanged;
					_picker.Dispose();
					_picker = null;
				}

				if (Control != null)
				{
					Control.EditingDidBegin -= OnStarted;
					Control.EditingDidEnd -= OnEnded;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var entry = new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };

					entry.EditingDidBegin += OnStarted;
					entry.EditingDidEnd += OnEnded;

					_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

					var width = UIScreen.MainScreen.Bounds.Width;
					var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
					var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
					var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) => entry.ResignFirstResponder());

					toolbar.SetItems(new[] { spacer, doneButton }, false);

					entry.InputView = _picker;
					entry.InputAccessoryView = toolbar;

					_defaultTextColor = entry.TextColor;

					_picker.ValueChanged += OnValueChanged;

					SetNativeControl(entry);
				}

				UpdateTime();
				UpdateTextColor();
				UpdateFlowDirection();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == TimePicker.TimeProperty.PropertyName || e.PropertyName == TimePicker.FormatProperty.PropertyName)
				UpdateTime();

			if (e.PropertyName == TimePicker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			ElementController.SetValueFromRenderer(TimePicker.TimeProperty, _picker.Date.ToDateTime() - new DateTime(1, 1, 1));
		}

		void UpdateFlowDirection()
		{
			if (ElementViewController == null || Control == null)
				return;

			if (ElementViewController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.RightToLeft))
			{
				Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
				(Control as UITextField).TextAlignment = UITextAlignment.Right;
			}
			else if (ElementViewController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.LeftToRight))
			{
				Control.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
				(Control as UITextField).TextAlignment = UITextAlignment.Left;
			}
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToUIColor();
		}

		void UpdateTime()
		{
			_picker.Date = new DateTime(1, 1, 1).Add(Element.Time).ToNSDate();
			Control.Text = DateTime.Today.Add(Element.Time).ToString(Element.Format);
		}
	}
}