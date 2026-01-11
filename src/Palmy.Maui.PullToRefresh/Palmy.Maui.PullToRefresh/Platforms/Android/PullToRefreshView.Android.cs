using Android.Content;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls.Handlers.Items;
using Palmy.Maui.PullToRefresh.Enums;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	public void InitializeCollectionView(CollectionView collectionView)
	{
		var handler = collectionView.Handler as CollectionViewHandler;
		if (handler?.PlatformView is not { } recyclerView)
		{
			throw new NotSupportedException("Only CollectionView is supported");
		}

		recyclerView.SetLayoutManager(new ExtendedLinearLayoutManager(recyclerView.Context));
		recyclerView.Invalidate();

		var itemTouchListener = new OnItemTouchListener(this);
		recyclerView.AddOnItemTouchListener(itemTouchListener);

		recyclerView.ClearOnScrollListeners();
		recyclerView.AddOnScrollListener(new ExtendedScrollListener(this));
	}
}

public class OnItemTouchListener(PullToRefreshView pullToRefreshView)
	: Java.Lang.Object, RecyclerView.IOnItemTouchListener
{
	readonly PullToRefreshView? _pullToRefreshView = pullToRefreshView;

	public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
		var x = ConvertToDp(@event.GetX());
		var y = ConvertToDp(@event.GetY());

		GestureStatus gestureStatus = @event.Action switch
		{
			MotionEventActions.Down => GestureStatus.Started,
			MotionEventActions.Move => GestureStatus.Running,
			MotionEventActions.Cancel => GestureStatus.Canceled,
			MotionEventActions.Up => GestureStatus.Completed,
			_ => GestureStatus.Canceled
		};

		_pullToRefreshView?.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));

		if (_pullToRefreshView == null)
			return false;

		var linearLayoutManager = recyclerView.GetLayoutManager();
		if (linearLayoutManager is ExtendedLinearLayoutManager extendedLinearLayoutManager)
		{
			extendedLinearLayoutManager.IsScrollVerticallyEnabled = !(_pullToRefreshView.State == PullToRefreshState.Pulling ||
			                                                          _pullToRefreshView.State == PullToRefreshState.ReleaseToRefresh);
		}

		return _pullToRefreshView.State == PullToRefreshState.Refreshing;
	}

	public void OnRequestDisallowInterceptTouchEvent(bool disallow)
	{
	}

	public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
	}

	double ConvertToDp(double value)
	{
		var density = DeviceDisplay.MainDisplayInfo.Density;
		return value / density;
	}
}

public class ExtendedLinearLayoutManager(Context? context) : LinearLayoutManager(context)
{
	public override void OnLayoutChildren(RecyclerView.Recycler? recycler, RecyclerView.State? state)
	{
		try
		{
			base.OnLayoutChildren(recycler, state);
		}
		catch (Java.Lang.IndexOutOfBoundsException)
		{
			// Fix rare crash when disabling scroll
		}
	}

	public bool IsScrollVerticallyEnabled { get; set; }= true;
	public override bool CanScrollVertically() => IsScrollVerticallyEnabled;
}

public class ExtendedScrollListener(PullToRefreshView pullToRefreshView) : RecyclerView.OnScrollListener
{
	private int _verticalOffset;

	public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
	{
		_verticalOffset += dy;
		if (_verticalOffset < 0)
			_verticalOffset = 0;

		pullToRefreshView.IsScrolledOnTop = _verticalOffset == 0;
		base.OnScrolled(recyclerView, dx, dy);
	}
}
