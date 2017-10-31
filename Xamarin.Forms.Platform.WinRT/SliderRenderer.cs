﻿using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class SliderRenderer : ViewRenderer<Slider, Windows.UI.Xaml.Controls.Slider>
	{
		IViewController ViewController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var slider = new Windows.UI.Xaml.Controls.Slider();
					SetNativeControl(slider);

					Control.Minimum = e.NewElement.Minimum;
					Control.Maximum = e.NewElement.Maximum;
					Control.Value = e.NewElement.Value;

					slider.ValueChanged += OnNativeValueChanged;

					// Even when using Center/CenterAndExpand, a Slider has an oddity where it looks
					// off-center in its layout by a smidge. The default templates are slightly different
					// between 8.1/UWP; the 8.1 rows are 17/Auto/32 and UWP are 18/Auto/18. The value of
					// the hardcoded 8.1 rows adds up to 49 (when halved is 24.5) and UWP are 36 (18). Using
					// a difference of about 6 pixels to correct this oddity seems to make them both center
					// more correctly.
					//
					// The VerticalAlignment needs to be set as well since a control would not actually be
					// centered if a larger HeightRequest is set.
					if (Element.VerticalOptions.Alignment == LayoutAlignment.Center && Control.Orientation == Windows.UI.Xaml.Controls.Orientation.Horizontal)
					{
						Control.VerticalAlignment = VerticalAlignment.Center;
#if WINDOWS_UWP
						slider.Margin = new Windows.UI.Xaml.Thickness(0, 7, 0, 0);
#else
						slider.Margin = new Windows.UI.Xaml.Thickness(0, 13, 0, 0);
#endif
					}
				}

				double stepping = Math.Min((e.NewElement.Maximum - e.NewElement.Minimum) / 10, 1);
				Control.StepFrequency = stepping;
				Control.SmallChange = stepping;
				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Slider.MinimumProperty.PropertyName)
				Control.Minimum = Element.Minimum;
			else if (e.PropertyName == Slider.MaximumProperty.PropertyName)
				Control.Maximum = Element.Maximum;
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
			{
				if (Control.Value != Element.Value)
					Control.Value = Element.Value;
			}
		}

		protected override void UpdateBackgroundColor()
		{
			if (Control != null)
			{
				Color backgroundColor = Element.BackgroundColor;
				if (!backgroundColor.IsDefault)
				{
					Control.Background = backgroundColor.ToBrush();
				}
				else
				{
					Control.ClearValue(Windows.UI.Xaml.Controls.Control.BackgroundProperty);
				}
			}
		}

		void UpdateFlowDirection()
		{
			if (ViewController == null || Control == null)
				return;

			if (ViewController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.RightToLeft))
				Control.FlowDirection = WFlowDirection.RightToLeft;
			else if (ViewController.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.LeftToRight))
				Control.FlowDirection = WFlowDirection.LeftToRight;
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnNativeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, e.NewValue);
		}
	}
}