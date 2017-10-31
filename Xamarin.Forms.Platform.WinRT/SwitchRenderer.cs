﻿using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class SwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
	{
		IVisualElementController VisualElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = new ToggleSwitch();
					control.Toggled += OnNativeToggled;
					control.ClearValue(ToggleSwitch.OnContentProperty);
					control.ClearValue(ToggleSwitch.OffContentProperty);

					SetNativeControl(control);
				}

				Control.IsOn = Element.IsToggled;

				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
			{
				Control.IsOn = Element.IsToggled;
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
			}
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnNativeToggled(object sender, RoutedEventArgs routedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.IsOn);
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
	}
}