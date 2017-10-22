using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal static class AlignmentExtensions
	{
		internal static NSTextAlignment ToNativeTextAlignment(this TextAlignment alignment, EffectiveFlowDirection flowDirection)
		{
			var isLtr = flowDirection.HasFlag(EffectiveFlowDirection.LeftToRight);
			switch (alignment)
			{
				case TextAlignment.Center:
					return NSTextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return NSTextAlignment.Right;
					else
						return NSTextAlignment.Left;
				default:
					if (isLtr)
						return NSTextAlignment.Left;
					else
						return NSTextAlignment.Natural;
			}
		}

		internal static FlowDirection ToFlowDirection(this NSApplicationLayoutDirection direction)
		{
			switch (direction)
			{
				case NSApplicationLayoutDirection.LeftToRight:
					return FlowDirection.LeftToRight;
				case NSApplicationLayoutDirection.RightToLeft:
					return FlowDirection.RightToLeft;
				default:
					return FlowDirection.MatchParent;
			}
		}
	}
}