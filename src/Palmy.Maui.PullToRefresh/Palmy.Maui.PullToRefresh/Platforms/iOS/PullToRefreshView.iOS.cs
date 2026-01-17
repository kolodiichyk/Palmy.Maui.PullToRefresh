using Foundation;
using Palmy.Maui.PullToRefresh.Enums;
using UIKit;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	private UIScrollView? _scrollView;
	private TouchInterceptorGestureRecognizer? _touchInterceptor;
	private UIView? _platformView;

	public void InitializeCollectionView(View view)
	{
		var platformView = view.Handler?.PlatformView as UIView;
		if (platformView == null)
			throw new NotSupportedException("Only UIView is supported");

		_platformView = platformView;
		_touchInterceptor = new TouchInterceptorGestureRecognizer(this);
		platformView.AddGestureRecognizer(_touchInterceptor);

		_scrollView = GetUIScrollView(view);
	}

	public double GetContentScrollOffset(View view)
	{
		return _scrollView?.ContentOffset.Y ?? 0;
	}

	public void SetContentScrollEnable(bool enable)
	{
		if (_scrollView != null)
		{
			_scrollView.ScrollEnabled = enable;
		}
	}

	private UIScrollView? GetUIScrollView(View view)
	{
		var scrollView = view.Handler?.PlatformView;
		if (scrollView is UIScrollView uIScroll)
		{
			return uIScroll;
		}

		if (scrollView is UIView scrollViewUIView && scrollViewUIView.ToString().Contains("UICollectionViewControllerWrapperView", StringComparison.Ordinal))
		{
			foreach (var subview in scrollViewUIView.Subviews.OfType<UIScrollView>())
			{
				return subview;
			}
		}

		return null;
	}

	/// <summary>
	/// Cleans up iOS-specific resources
	/// </summary>
	private void CleanupPlatformResources()
	{
		if (_touchInterceptor != null && _platformView != null)
		{
			_platformView.RemoveGestureRecognizer(_touchInterceptor);
			_touchInterceptor.Dispose();
			_touchInterceptor = null;
		}

		_platformView = null;
		_scrollView = null;
	}
}

public sealed class TouchInterceptorGestureRecognizer : UIGestureRecognizer
{
	private readonly WeakReference<PullToRefreshView>? _pullToRefreshViewRef;

	public TouchInterceptorGestureRecognizer(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshViewRef = new WeakReference<PullToRefreshView>(pullToRefreshView);
		CancelsTouchesInView = false;
		DelaysTouchesBegan = false;
		DelaysTouchesEnded = false;
	}

	private PullToRefreshView? PullToRefreshView =>
		_pullToRefreshViewRef?.TryGetTarget(out var view) == true ? view : null;

	void OnTouches(NSSet touches, GestureStatus gestureStatus)
	{
		float x = -1;
		float y = -1;
		if (touches.AnyObject is UITouch touch)
		{
			var location = touch.LocationInView(View);
			x = (float)location.X;
			y = (float)location.Y;
		}

		PullToRefreshView?.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));
	}

	public override void TouchesBegan(NSSet touches, UIEvent evt)
	{
		base.TouchesBegan(touches, evt);
		OnTouches(touches, GestureStatus.Started);
		State = UIGestureRecognizerState.Possible;
	}

	public override void TouchesMoved(NSSet touches, UIEvent evt)
	{
		base.TouchesMoved(touches, evt);
		OnTouches(touches, GestureStatus.Running);

		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null)
		{
			State = UIGestureRecognizerState.Cancelled;
			return;
		}

		if (pullToRefreshView.State == PullToRefreshState.Finished ||
		    pullToRefreshView.State == PullToRefreshState.Canceled)
		{
			State = UIGestureRecognizerState.Began;
		}
		else
		{
			State = UIGestureRecognizerState.Changed;
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent evt)
	{
		base.TouchesEnded(touches, evt);
		OnTouches(touches, GestureStatus.Completed);
		State = UIGestureRecognizerState.Ended;
	}

	public override void TouchesCancelled(NSSet touches, UIEvent evt)
	{
		base.TouchesCancelled(touches, evt);
		OnTouches(touches, GestureStatus.Canceled);
		State = UIGestureRecognizerState.Cancelled;
	}

	public override bool CanPreventGestureRecognizer(UIGestureRecognizer preventedGestureRecognizer)
	{
		return false; // Don't prevent other gestures
	}

	public override bool CanBePreventedByGestureRecognizer(UIGestureRecognizer preventingGestureRecognizer)
	{
		return false; // Don't let other gestures prevent this one
	}
}