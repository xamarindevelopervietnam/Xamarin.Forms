﻿using System;
using System.ComponentModel;
using WFlowDirection = Windows.UI.Xaml.FlowDirection;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class StepperRenderer : ViewRenderer<Stepper, StepperControl>
	{
		IVisualElementController VisualElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new StepperControl());
					Control.ValueChanged += OnControlValue;
				}

				UpdateMaximum();
				UpdateMinimum();
				UpdateValue();
				UpdateIncrement(); 
				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Stepper.ValueProperty.PropertyName)
				UpdateValue();
			else if (e.PropertyName == Stepper.MaximumProperty.PropertyName)
				UpdateMaximum();
			else if (e.PropertyName == Stepper.MinimumProperty.PropertyName)
				UpdateMinimum();
			else if (e.PropertyName == Stepper.IncrementProperty.PropertyName)
				UpdateIncrement();
			else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor();
		}

		protected override void UpdateBackgroundColor()
		{
			if (Control != null)
				Control.ButtonBackgroundColor = Element.BackgroundColor;
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlValue(object sender, EventArgs e)
		{
			Element.SetValueCore(Stepper.ValueProperty, Control.Value);
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

		void UpdateIncrement()
		{
			Control.Increment = Element.Increment;
		}

		void UpdateMaximum()
		{
			Control.Maximum = Element.Maximum;
		}

		void UpdateMinimum()
		{
			Control.Minimum = Element.Minimum;
		}

		void UpdateValue()
		{
			Control.Value = Element.Value;
		}
	}
}