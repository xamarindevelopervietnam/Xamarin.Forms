using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Debug = System.Diagnostics.Debug;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	internal class InnerGestureListener : Object, GestureDetector.IOnGestureListener, GestureDetector.IOnDoubleTapListener
	{
		readonly TapGestureHandler _tapGestureHandler;
		readonly PanGestureHandler _panGestureHandler;
		readonly Context _context;
		bool _isScrolling;		
		float _lastX;
		float _lastY;
		bool _disposed;

		Func<bool> _scrollCompleteDelegate;
		Func<float, float, int, bool> _scrollDelegate;
		Func<int, bool> _scrollStartedDelegate;
		Func<int, bool> _tapDelegate;
		Func<int, IEnumerable<TapGestureRecognizer>> _tapGestureRecognizers;
		int _slop;

		public InnerGestureListener(TapGestureHandler tapGestureHandler, PanGestureHandler panGestureHandler, Context context)
		{
			if (tapGestureHandler == null)
			{
				throw new ArgumentNullException(nameof(tapGestureHandler));
			}

			if (panGestureHandler == null)
			{
				throw new ArgumentNullException(nameof(panGestureHandler));
			}

			_tapGestureHandler = tapGestureHandler;
			_panGestureHandler = panGestureHandler;
			_context = context;

			_tapDelegate = tapGestureHandler.OnTap;
			_tapGestureRecognizers = tapGestureHandler.TapGestureRecognizers;
			_scrollDelegate = panGestureHandler.OnPan;
			_scrollStartedDelegate = panGestureHandler.OnPanStarted;
			_scrollCompleteDelegate = panGestureHandler.OnPanComplete;

			_slop = ViewConfiguration.Get(context).ScaledTouchSlop;
			
			//Resource.GetDimensionPixelSize(
			//	311                com.android.internal.R.dimen.config_viewConfigurationTouchSlop);
		}

		bool HasAnyGestures()
		{
			return _panGestureHandler.HasAnyGestures() || _tapGestureHandler.HasAnyGestures();
		}

		// This is needed because GestureRecognizer callbacks can be delayed several hundred milliseconds
		// which can result in the need to resurrect this object if it has already been disposed. We dispose
		// eagerly to allow easier garbage collection of the renderer
		internal InnerGestureListener(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
		{
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTap(MotionEvent e)
		{
			if (_disposed)
				return false;

			if (HasDoubleTapHandler())
			{
				return _tapDelegate(2);
			}

			if (HasSingleTapHandler())
			{
				// If we're registering double taps and we don't actually have a double-tap handler,
				// but we _do_ have a single-tap handler, then we're really just seeing two singles in a row
				// Fire off the delegate for the second single-tap (OnSingleTapUp already did the first one)
				return _tapDelegate(1);
			}

			return false;
		}

		bool GestureDetector.IOnDoubleTapListener.OnDoubleTapEvent(MotionEvent e)
		{
			return false;
		}

		bool GestureDetector.IOnGestureListener.OnDown(MotionEvent e)
		{
			SetStartingPosition(e);

			if (HasAnyGestures())
			{
				// If we have any gestures to listen for, we need to return true to show we're interested in the rest
				// of the events.		
				return true;
			}

			// Since we don't have any gestures we're listening for, we return false to show we're not interested
			// and let parent controls have a whack at the events
			return false;
		}

		// TODO hartez 2017/10/23 17:35:49 Go back to your android app and do all this on there
		// also, look for a X.A example for taps to compare with.

		const int MinimumFlingDistance = 60;
		const int MinimumFlingDistanceSquared = MinimumFlingDistance * MinimumFlingDistance;
		

		bool GestureDetector.IOnGestureListener.OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
		{
			if (e1 == null)
			{
				Debug.WriteLine($">>>>> InnerGestureListener OnFling e1 was null");
				return false;
			}
			
			if (e2 == null)
			{
				Debug.WriteLine($">>>>> InnerGestureListener OnFling e2 was null");
				return false;
			}
		

			Debug.WriteLine($">>>>> InnerGestureListener OnFling 104: {e1.Action}, {e2.Action}");
			Debug.WriteLine($">>>>> InnerGestureListener OnFling: e1 - {e1.GetX()}, {e1.GetY()}; e2 - {e2.GetX()}, {e2.GetY()}");

			//Debug.WriteLine($">>>>> InnerGestureListener OnFling 128: e1.XPrecision = {e1.XPrecision}");
			//Debug.WriteLine($">>>>> InnerGestureListener OnFling 128: e1.YPrecision = {e1.YPrecision}");
			//Debug.WriteLine($">>>>> InnerGestureListener OnFling 128: e2.XPrecision = {e2.XPrecision}");
			//Debug.WriteLine($">>>>> InnerGestureListener OnFling 128: e2.YPrecision = {e2.YPrecision}");

			EndScrolling();
			
			float x1 = e1.GetX();
			float x2 = e2.GetX();
			float y1 = e1.GetY();
			float y2 = e2.GetY();

			var a = x1 - x2;
			Debug.WriteLine($">>>>> InnerGestureListener OnFling 141: a is {a}");

			var b = y1 - y2;
			Debug.WriteLine($">>>>> InnerGestureListener OnFling 141: b is {b}");

			var csquared = (a * a) + (b * b);

			Debug.WriteLine($">>>>> InnerGestureListener OnFling 148: csquared is {csquared}");

			var distance = Math.Sqrt(csquared);

			Debug.WriteLine($">>>>> InnerGestureListener OnFling 138: distance is {distance}, touch slop is {_slop}");

			if (_slop >= distance)
			{
				Debug.WriteLine($">>>>> InnerGestureListener OnFling 142: This should never happen, right?");
			}

			//if ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) > MinimumFlingDistanceSquared)
			//{
			//	// Fling stuff goes here; we don't support it just yet
			//	return true;
			//}
			
			//// A very tiny fling; the user was probably trying to tap
			//Debug.WriteLine($">>>>> InnerGestureListener OnFling 135: Tiny fling, probably meant to tap");
			//return (this as GestureDetector.IOnGestureListener).OnSingleTapUp(e1); // This should be whichever is the up action, probably e2

			return false;
		}

		void GestureDetector.IOnGestureListener.OnLongPress(MotionEvent e)
		{
			SetStartingPosition(e);
		}

		bool GestureDetector.IOnGestureListener.OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
		{
			//Debug.WriteLine($">>>>> InnerGestureListener OnScroll 113: MESSAGE");

			if (e1 == null || e2 == null)
				return false;

			SetStartingPosition(e1);

			return StartScrolling(e2);
		}

		void GestureDetector.IOnGestureListener.OnShowPress(MotionEvent e)
		{
		}

		bool GestureDetector.IOnGestureListener.OnSingleTapUp(MotionEvent e)
		{
			Debug.WriteLine($">>>>> InnerGestureListener OnSingleTapUp 132: MESSAGE");

			if (_disposed)
				return false;

			if (HasDoubleTapHandler())
			{
				// Because we have a handler for double-tap, we need to wait for
				// OnSingleTapConfirmed (to verify it's really just a single tap) before running the delegate
				return false;
			}

			// A single tap has occurred and there's no handler for double tap to worry about,
			// so we can go ahead and run the delegate
			return _tapDelegate(1);
		}

		bool GestureDetector.IOnDoubleTapListener.OnSingleTapConfirmed(MotionEvent e)
		{
			//Debug.WriteLine($">>>>> InnerGestureListener OnSingleTapConfirmed 151: MESSAGE");

			if (_disposed)
				return false;

			if (!HasDoubleTapHandler())
			{
				// We're not worried about double-tap, so OnSingleTapUp has already run the delegate
				// there's nothing for us to do here
				return false;
			}

			// Since there was a double-tap handler, we had to wait for OnSingleTapConfirmed;
			// Now that we're sure it's a single tap, we can run the delegate
			return _tapDelegate(1);
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				_tapDelegate = null;
				_tapGestureRecognizers = null;
				_scrollDelegate = null;
				_scrollStartedDelegate = null;
				_scrollCompleteDelegate = null;
			}

			base.Dispose(disposing);
		}

		void SetStartingPosition(MotionEvent e1)
		{
			_lastX = e1.GetX();
			_lastY = e1.GetY();
		}

		bool StartScrolling(MotionEvent e2)
		{
			if (_scrollDelegate == null)
				return false;

			if (!_isScrolling && _scrollStartedDelegate != null)
				_scrollStartedDelegate(e2.PointerCount);

			_isScrolling = true;

			float totalX = e2.GetX() - _lastX;
			float totalY = e2.GetY() - _lastY;

			return _scrollDelegate(totalX, totalY, e2.PointerCount);
		}

		void EndScrolling()
		{
			if (_isScrolling && _scrollCompleteDelegate != null)
				_scrollCompleteDelegate();

			_isScrolling = false;
		}

		bool HasDoubleTapHandler()
		{
			if (_tapGestureRecognizers == null)
				return false;
			return _tapGestureRecognizers(2).Any();
		}

		bool HasSingleTapHandler()
		{
			if (_tapGestureRecognizers == null)
				return false;
			return _tapGestureRecognizers(1).Any();
		}
	}
}