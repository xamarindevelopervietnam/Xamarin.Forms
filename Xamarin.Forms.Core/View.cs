using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class View : VisualElement, IViewController, IFlowDirectionController
	{
		public static readonly BindableProperty VerticalOptionsProperty = BindableProperty.Create("VerticalOptions", typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
			propertyChanged: (bindable, oldvalue, newvalue) => ((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged));

		public static readonly BindableProperty HorizontalOptionsProperty = BindableProperty.Create("HorizontalOptions", typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
			propertyChanged: (bindable, oldvalue, newvalue) => ((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.HorizontalOptionsChanged));

		public static readonly BindableProperty MarginProperty = BindableProperty.Create("Margin", typeof(Thickness), typeof(View), default(Thickness), propertyChanged: MarginPropertyChanged);

		readonly ObservableCollection<IGestureRecognizer> _gestureRecognizers = new ObservableCollection<IGestureRecognizer>();
		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(View), FlowDirection.MatchParent, propertyChanged: FlowDirectionChanged);

		IFlowDirectionController FlowController => this;

		protected internal View()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						foreach (IElement item in args.NewItems.OfType<IElement>())
						{
							ValidateGesture(item as IGestureRecognizer);
							item.Parent = this;
						}
						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (IElement item in args.OldItems.OfType<IElement>())
							item.Parent = null;
						break;
					case NotifyCollectionChangedAction.Replace:
						foreach (IElement item in args.NewItems.OfType<IElement>())
						{
							ValidateGesture(item as IGestureRecognizer);
							item.Parent = this;
						}
						foreach (IElement item in args.OldItems.OfType<IElement>())
							item.Parent = null;
						break;
					case NotifyCollectionChangedAction.Reset:
						foreach (IElement item in _gestureRecognizers.OfType<IElement>())
							item.Parent = this;
						break;
				}
			};
		}

		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		public LayoutOptions HorizontalOptions
		{
			get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
			set { SetValue(HorizontalOptionsProperty, value); }
		}

		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		public LayoutOptions VerticalOptions
		{
			get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
			set { SetValue(VerticalOptionsProperty, value); }
		}

		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection { get; set; } = EffectiveFlowDirection.LeftToRight | EffectiveFlowDirection.Implicit;

		protected override void OnBindingContextChanged()
		{
			var gotBindingContext = false;
			object bc = null;

			for (var i = 0; i < GestureRecognizers.Count; i++)
			{
				var bo = GestureRecognizers[i] as BindableObject;
				if (bo == null)
					continue;

				if (!gotBindingContext)
				{
					bc = BindingContext;
					gotBindingContext = true;
				}

				SetInheritedBindingContext(bo, bc);
			}

			base.OnBindingContextChanged();
		}

		EffectiveFlowDirection IViewController.EffectiveFlowDirection => FlowController.EffectiveFlowDirection;

		static void FlowDirectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IFlowDirectionController;

			if (self.EffectiveFlowDirection.HasFlag(EffectiveFlowDirection.Explicit) && oldValue == newValue)
				return;

			var newFlowDirection = (FlowDirection)newValue;

			self.EffectiveFlowDirection = newFlowDirection.ToEffectiveFlowDirection(EffectiveFlowDirection.Explicit);

			self.NotifyFlowDirectionChanged();
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();

			FlowController.NotifyFlowDirectionChanged();
		}

		static void MarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.MarginChanged);
		}

		void IFlowDirectionController.NotifyFlowDirectionChanged()
		{
			SetFlowDirectionFromParent(this);

			foreach (var element in LogicalChildren)
			{
				var view = element as IFlowDirectionController;
				if (view == null)
					continue;
				view.NotifyFlowDirectionChanged();
			}
		}

		void ValidateGesture(IGestureRecognizer gesture)
		{
			if (gesture == null)
				return;
			if (gesture is PinchGestureRecognizer && _gestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().Count() > 1)
				throw new InvalidOperationException($"Only one {nameof(PinchGestureRecognizer)} per view is allowed");
		}
	}
}