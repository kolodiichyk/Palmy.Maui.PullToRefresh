using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using View = Android.Views.View;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView
{
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

		_genericTouchListener = new GenericTouchListener(this);
		// Fix Down gesture and Scroll disable
		_itemTouchListener = new OnItemTouchListener(this, _genericTouchListener);

		recyclerView.AddOnItemTouchListener(_itemTouchListener);
		recyclerView.SetOnTouchListener(_genericTouchListener);

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

	private View InitializeViewGroup(ViewGroup viewGroup)
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
			case RecyclerView recyclerView:
				ViewCompat.SetNestedScrollingEnabled(recyclerView, enable);
				recyclerView.VerticalScrollBarEnabled = enable;
				_itemTouchListener?.OnRequestDisallowInterceptTouchEvent(!enable);
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

		_recyclerView = null;
		_scrollableView = null;
	}
}
