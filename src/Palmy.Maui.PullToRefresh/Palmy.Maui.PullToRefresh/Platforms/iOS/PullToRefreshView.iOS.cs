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

		_platformView = platformView ?? throw new NotSupportedException("Only UIView is supported");
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
