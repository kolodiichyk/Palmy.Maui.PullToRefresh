using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Palmy.Maui.PullToRefresh.Enums;
using View = Android.Views.View;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
	private ExtendedLinearLayoutManager? _extendedLinearLayoutManager;
	private View? _scrollableView;
	private OnItemTouchListener? _itemTouchListener;
	private GenericTouchListener? _genericTouchListener;
	private RecyclerView? _recyclerView;

	public void InitializeCollectionView(Microsoft.Maui.Controls.View view)
	{
		var platformView = view.Handler?.PlatformView as View;

		if (platformView == null)
			throw new NotSupportedException("Platform view is null");

		_scrollableView = platformView switch
		{
			RecyclerView recyclerView => InitializeRecyclerView(recyclerView),
			Android.Webkit.WebView webView => InitializeWebView(webView),
			NestedScrollView nestedScrollView => InitializeNestedScrollView(nestedScrollView),
			Android.Widget.ScrollView scrollView => InitializeScrollView(scrollView),
			SwipeRefreshLayout swipeRefreshLayout => InitializeSwipeRefreshLayout(swipeRefreshLayout),
			ViewGroup viewGroup => InitializeViewGroup(viewGroup),
			_ => throw new NotSupportedException($"View type {platformView.GetType().Name} is not supported")
		};
	}

	private RecyclerView InitializeRecyclerView(RecyclerView recyclerView)
	{
		_recyclerView = recyclerView;
		_extendedLinearLayoutManager = new ExtendedLinearLayoutManager(recyclerView.Context);
		recyclerView.SetLayoutManager(_extendedLinearLayoutManager);
		recyclerView.Invalidate();

		_itemTouchListener = new OnItemTouchListener(this);
		recyclerView.AddOnItemTouchListener(_itemTouchListener);

		return recyclerView;
	}

	private Android.Webkit.WebView InitializeWebView(Android.Webkit.WebView webView)
	{
		_genericTouchListener = new GenericTouchListener(this);
		webView.SetOnTouchListener(_genericTouchListener);
		return webView;
	}

	private NestedScrollView InitializeNestedScrollView(NestedScrollView nestedScrollView)
	{
		_genericTouchListener = new GenericTouchListener(this);
		nestedScrollView.SetOnTouchListener(_genericTouchListener);
		return nestedScrollView;
	}

	private Android.Widget.ScrollView InitializeScrollView(Android.Widget.ScrollView scrollView)
	{
		_genericTouchListener = new GenericTouchListener(this);
		scrollView.SetOnTouchListener(_genericTouchListener);
		return scrollView;
	}

	private SwipeRefreshLayout InitializeSwipeRefreshLayout(SwipeRefreshLayout swipeRefreshLayout)
	{
		_genericTouchListener = new GenericTouchListener(this);
		swipeRefreshLayout.SetOnTouchListener(_genericTouchListener);
		return swipeRefreshLayout;
	}

	private View? InitializeViewGroup(ViewGroup viewGroup)
	{
		var scrollableChild = FindScrollableChild(viewGroup);
		if (scrollableChild != null)
		{
			return scrollableChild switch
			{
				RecyclerView rv => InitializeRecyclerView(rv),
				Android.Webkit.WebView wv => InitializeWebView(wv),
				NestedScrollView nsv => InitializeNestedScrollView(nsv),
				Android.Widget.ScrollView sv => InitializeScrollView(sv),
				_ => InitializeGenericView(scrollableChild)
			};
		}

		return InitializeGenericView(viewGroup);
	}

	private View InitializeGenericView(View view)
	{
		_genericTouchListener = new GenericTouchListener(this);
		view.SetOnTouchListener(_genericTouchListener);
		return view;
	}

	private View? FindScrollableChild(ViewGroup viewGroup)
	{
		for (var i = 0; i < viewGroup.ChildCount; i++)
		{
			var child = viewGroup.GetChildAt(i);
			if (child is RecyclerView or Android.Webkit.WebView or NestedScrollView or Android.Widget.ScrollView)
				return child;

			if (child is ViewGroup childGroup)
			{
				var found = FindScrollableChild(childGroup);
				if (found != null)
					return found;
			}
		}

		return null;
	}

	public void SetContentScrollEnable(bool enable)
	{
		if (_extendedLinearLayoutManager != null)
		{
			_extendedLinearLayoutManager.IsScrollVerticallyEnabled = enable;
			return;
		}

		switch (_scrollableView)
		{
			case NestedScrollView nestedScrollView:
				ViewCompat.SetNestedScrollingEnabled(nestedScrollView, enable);
				break;
			case Android.Widget.ScrollView scrollView:
				ViewCompat.SetNestedScrollingEnabled(scrollView, enable);
				break;
			case Android.Webkit.WebView webView:
				webView.VerticalScrollBarEnabled = enable;
				break;
		}
	}

	public double GetContentScrollOffset(Microsoft.Maui.Controls.View view)
	{
		var scrollView = view.Handler?.PlatformView;
		if (scrollView is not ViewGroup)
			return 0;

		return scrollView switch
		{
			SwipeRefreshLayout layout => layout.ScrollY,
			Android.Webkit.WebView webView => webView.ScrollY,
			RecyclerView recyclerView => recyclerView.ComputeVerticalScrollOffset(),
			View nativeView when view is ScrollView => nativeView.ScrollY,
			_ => 0
		};
	}

	/// <summary>
	/// Cleans up Android-specific resources
	/// </summary>
	private void CleanupPlatformResources()
	{
		// Remove and dispose item touch listener
		if (_itemTouchListener != null && _recyclerView != null)
		{
			_recyclerView.RemoveOnItemTouchListener(_itemTouchListener);
			_itemTouchListener.Dispose();
			_itemTouchListener = null;
		}

		// Remove and dispose generic touch listener
		if (_genericTouchListener != null && _scrollableView != null)
		{
			_scrollableView.SetOnTouchListener(null);
			_genericTouchListener.Dispose();
			_genericTouchListener = null;
		}

		// Dispose extended layout manager
		if (_extendedLinearLayoutManager != null)
		{
			_extendedLinearLayoutManager.Dispose();
			_extendedLinearLayoutManager = null;
		}

		_recyclerView = null;
		_scrollableView = null;
	}
}

public class OnItemTouchListener : Java.Lang.Object, RecyclerView.IOnItemTouchListener
{
	private readonly WeakReference<PullToRefreshView>? _pullToRefreshViewRef;

	public OnItemTouchListener(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshViewRef = new WeakReference<PullToRefreshView>(pullToRefreshView);
	}

	private PullToRefreshView? PullToRefreshView =>
		_pullToRefreshViewRef?.TryGetTarget(out var view) == true ? view : null;

	public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null)
			return false;

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

		pullToRefreshView.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));

		return pullToRefreshView.State == PullToRefreshState.Refreshing;
	}

	public void OnRequestDisallowInterceptTouchEvent(bool disallow)
	{
	}

	public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event)
	{
	}

	private static double ConvertToDp(double value)
	{
		var density = DeviceDisplay.MainDisplayInfo.Density;
		return value / density;
	}
}

public class ExtendedLinearLayoutManager : LinearLayoutManager
{
	public ExtendedLinearLayoutManager(Context? context) : base(context)
	{
	}

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

	public bool IsScrollVerticallyEnabled { get; set; } = true;
	public override bool CanScrollVertically() => IsScrollVerticallyEnabled;
}

public class GenericTouchListener : Java.Lang.Object, View.IOnTouchListener
{
	private readonly WeakReference<PullToRefreshView>? _pullToRefreshViewRef;

	public GenericTouchListener(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshViewRef = new WeakReference<PullToRefreshView>(pullToRefreshView);
	}

	private PullToRefreshView? PullToRefreshView =>
		_pullToRefreshViewRef?.TryGetTarget(out var view) == true ? view : null;

	public bool OnTouch(View? v, MotionEvent? e)
	{
		if (e == null)
			return false;

		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null)
			return false;

		var x = ConvertToDp(e.GetX());
		var y = ConvertToDp(e.GetY());

		GestureStatus gestureStatus = e.Action switch
		{
			MotionEventActions.Down => GestureStatus.Started,
			MotionEventActions.Move => GestureStatus.Running,
			MotionEventActions.Cancel => GestureStatus.Canceled,
			MotionEventActions.Up => GestureStatus.Completed,
			_ => GestureStatus.Canceled
		};

		pullToRefreshView.OnInterceptPanUpdated(
			new PanUpdatedEventArgs(gestureStatus, 1, x, y));

		// Return false to allow touch events to propagate to other handlers
		// Only consume events when actively pulling
		return pullToRefreshView.State == PullToRefreshState.Pulling ||
		       pullToRefreshView.State == PullToRefreshState.ReleaseToRefresh ||
		       pullToRefreshView.State == PullToRefreshState.Refreshing;
	}

	private static double ConvertToDp(double value)
	{
		var density = DeviceDisplay.MainDisplayInfo.Density;
		return value / density;
	}
}