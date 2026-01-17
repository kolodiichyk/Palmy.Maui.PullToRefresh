using Palmy.Maui.PullToRefresh.Enums;
using Palmy.Maui.PullToRefresh.Interfaces;

namespace Palmy.Maui.PullToRefresh.Strategies;

internal class Normal : IPullToRefreshStrategy
{
	const double _springConstant = 0.35;
	const string AnimationName = "NormalPullToRefreshAnimation";

	private readonly WeakReference<PullToRefreshView> _pullToRefreshViewRef;
	private Grid? _containerGrid;
	private View? _refreshView;
	private View? _contentView;

	private double _startY;
	private bool _isPulling;
	private bool _wasScrolledOnTop = true;
	private bool _disposed;

	public Normal(PullToRefreshView pullToRefreshView)
	{
		_pullToRefreshViewRef = new WeakReference<PullToRefreshView>(pullToRefreshView);
	}

	private PullToRefreshView? PullToRefreshView =>
		_pullToRefreshViewRef.TryGetTarget(out var view) ? view : null;

	public void Initialize()
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null) return;

		_containerGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Star }
			},
			SafeAreaEdges = pullToRefreshView.SafeAreaEdges
		};
	}

	public void OnRefreshViewSet(View view)
	{
		if (_containerGrid == null) return;

		if (_refreshView != null)
			_containerGrid.Children.Remove(_refreshView);

		_refreshView = view;
		if (_refreshView != null)
		{
			_refreshView.VerticalOptions = LayoutOptions.Start;
			_refreshView.TranslationY = -_refreshView.HeightRequest;
			_containerGrid.Children.Add(_refreshView);
			_refreshView.InvalidateMeasure();
			Grid.SetRow(_refreshView, 0);
		}
	}

	public void OnMainContentViewSet(View view)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null || _containerGrid == null) return;

		// Prevent setting content directly to avoid conflicts
		if (view == _containerGrid)
			return;

		if (_contentView != null)
			_containerGrid.Children.Remove(_contentView);

		var userContent = view;
		pullToRefreshView.Content = _containerGrid;
		_contentView = userContent;
		_containerGrid.Children.Add(_contentView);
		Grid.SetRow(_contentView, 0);
	}

	public void OnHandlerChanged(IViewHandler handler)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null || _contentView == null) return;

		pullToRefreshView.InitializeCollectionView(_contentView);
		_contentView.BindingContext = pullToRefreshView.BindingContext;
	}

	public void HandlePanStarted(double x, double y)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null || _contentView == null) return;

		_wasScrolledOnTop = pullToRefreshView.GetContentScrollOffset(_contentView) == 0;
		_startY = y;
	}

	public PullResult? HandlePanMovement(double x, double y)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null || _contentView == null) return null;

		if (!_isPulling && y > _startY && _wasScrolledOnTop && pullToRefreshView.GetContentScrollOffset(_contentView) == 0)
		{
			_isPulling = true;
		}

		if (!_isPulling || _refreshView == null || _containerGrid == null)
		{
			return null;
		}

		var displacement = _containerGrid.TranslationY + y - _startY;
		// Non-linear spring behavior - gets harder to pull as you go further
		var actualPullDistance = displacement / (1 + _springConstant * displacement / 100);
		// Clamp to maximum pull distance
		actualPullDistance = Math.Min(actualPullDistance, pullToRefreshView.MaxPullDistance);
		var newTranslationY = Math.Max(0, actualPullDistance);

		_containerGrid.TranslationY = newTranslationY;

		if (pullToRefreshView.AutoResize &&
		    _containerGrid.TranslationY > pullToRefreshView.RefreshHeight)
		{
			_refreshView.HeightRequest = Math.Ceiling(_containerGrid.TranslationY) + 2;
			_refreshView.TranslationY = - Math.Ceiling(_containerGrid.TranslationY) - 2;
		}

		var state = newTranslationY < pullToRefreshView.RefreshHeight ? PullToRefreshState.Pulling : PullToRefreshState.ReleaseToRefresh;
		var percentage = newTranslationY / pullToRefreshView.RefreshHeight * 100;

		return new PullResult(state, percentage);
	}

	public PullResult? HandlePanFinished(double x, double y)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null) return null;

		if (!_isPulling || _refreshView == null || _containerGrid == null)
		{
			return null;
		}

		var state = pullToRefreshView.State;
		if (state == PullToRefreshState.ReleaseToRefresh)
		{
			CancelAnimations();

			state = PullToRefreshState.Refreshing;
			var animation = new Animation(v => _containerGrid.TranslationY = v,
				_containerGrid.TranslationY,
				pullToRefreshView.RefreshHeight);
			animation.Add(0, 1, new Animation(v => _refreshView.TranslationY = v,
				_refreshView.TranslationY, -pullToRefreshView.RefreshHeight - 2));
			animation.Add(0, 1, new Animation(v => _refreshView.HeightRequest = v,
				_refreshView.HeightRequest, pullToRefreshView.RefreshHeight + 2));
			animation.Commit(pullToRefreshView, AnimationName, 16, 250, pullToRefreshView.AnimationTransition, finished: (_, _) =>
			{
				if (_refreshView != null)
				{
					_refreshView.HeightRequest = pullToRefreshView.RefreshHeight;
					_refreshView.TranslationY = -pullToRefreshView.RefreshHeight;
				}
			});
			return new PullResult(state, 100);
		}

		if (state == PullToRefreshState.Pulling)
		{
			return new PullResult(PullToRefreshState.Canceled, 0);
		}

		return null;
	}

	public PullResult? OnFinishedRefreshing(PullToRefreshState state)
	{
		var pullToRefreshView = PullToRefreshView;
		if (pullToRefreshView == null) return null;

		_isPulling = false;

		if (_refreshView == null || _containerGrid == null)
			return null;

		CancelAnimations();

		var animation = new Animation(v => _containerGrid.TranslationY = v,
			_containerGrid.TranslationY, 0);
		animation.Add(0, 1, new Animation(v => _refreshView.TranslationY = v,
			_refreshView.TranslationY, -pullToRefreshView.RefreshHeight - 2));
		animation.Add(0, 1, new Animation(v => _refreshView.HeightRequest = v,
			_refreshView.HeightRequest, pullToRefreshView.RefreshHeight + 2));

		animation.Commit(pullToRefreshView, AnimationName, 16, 250,
			pullToRefreshView.AnimationTransition, finished: (_, _) =>
			{
				if (_refreshView != null)
				{
					_refreshView.HeightRequest = pullToRefreshView.RefreshHeight;
					_refreshView.TranslationY = -pullToRefreshView.RefreshHeight;
				}
			});

		return new PullResult(state, 0);
	}

	public void CancelAnimations()
	{
		var pullToRefreshView = PullToRefreshView;
		pullToRefreshView?.AbortAnimation(AnimationName);
	}

	public void Dispose()
	{
		if (_disposed) return;
		_disposed = true;

		CancelAnimations();

		if (_containerGrid != null)
		{
			_containerGrid.Children.Clear();
			_containerGrid = null;
		}

		_refreshView = null;
		_contentView = null;
	}
}
