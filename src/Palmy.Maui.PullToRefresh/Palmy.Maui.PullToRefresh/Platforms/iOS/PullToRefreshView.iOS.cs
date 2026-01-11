using Foundation;
using Microsoft.Maui.Controls.Handlers.Items2;
using Palmy.Maui.PullToRefresh.Enums;
using UIKit;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	public void InitializeCollectionView(CollectionView collectionView)
	{
		var handler = collectionView.Handler as CollectionViewHandler2;
		if (handler == null)
			throw new NotSupportedException("Only CollectionView is supported");

		var touchInterceptor = new TouchInterceptorGestureRecognizer(this);
		handler.PlatformView.AddGestureRecognizer(touchInterceptor);

		collectionView.Scrolled -= CollectionViewOnScrolled;
		collectionView.Scrolled += CollectionViewOnScrolled;
	}

	private void CollectionViewOnScrolled(object? sender, ItemsViewScrolledEventArgs e)
	{
		IsScrolledOnTop = e.VerticalOffset == 0;
	}
}

public sealed class TouchInterceptorGestureRecognizer : UIGestureRecognizer
{
	readonly PullToRefreshView? _pullToRefreshView;
	public TouchInterceptorGestureRecognizer(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshView = pullToRefreshView;
		CancelsTouchesInView = false;
		DelaysTouchesBegan = false;
		DelaysTouchesEnded = false;
	}

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
		_pullToRefreshView?.OnInterceptPanUpdated(
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

		if (_pullToRefreshView == null)
			throw new NullReferenceException("PullToRefreshView can't be null.");

		if (!(_pullToRefreshView.State == PullToRefreshState.Finished ||
		                                    _pullToRefreshView.State == PullToRefreshState.Canceled))
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
		if (_pullToRefreshView == null)
			return false;

		return !(_pullToRefreshView.State == PullToRefreshState.Finished ||
		         _pullToRefreshView.State == PullToRefreshState.Canceled); // Don't prevent other gestures
	}

	public override bool CanBePreventedByGestureRecognizer(UIGestureRecognizer preventingGestureRecognizer)
	{
		return false; // Don't let other gestures prevent this one
	}
}
