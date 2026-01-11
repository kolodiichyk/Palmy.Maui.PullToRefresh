using Palmy.Maui.PullToRefresh;
using Palmy.Maui.PullToRefresh.Enums;
using Palmy.Maui.PullToRefresh.Interfaces;
using Palmy.Maui.PullToRefresh.Strategies;

internal class Overlay(PullToRefreshView pullToRefreshView) : IPullToRefreshStrategy
{
    const double springConstant = 0.35;

    private Grid _containerGrid;
    private View _refreshView;
    private View _contentView;

    private double _startY;
    private bool _isPulling;
    private bool _wasScrolledOnTop = true;

    public void Initialize()
    {
        _containerGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Star }
            },
            SafeAreaEdges = pullToRefreshView.SafeAreaEdges,
            IsClippedToBounds =  true,
        };
    }

    public void OnRefreshViewSet(View view)
    {
        if (_refreshView != null)
            _containerGrid.Children.Remove(_refreshView);

        _refreshView = view;
        if (_refreshView != null)
        {
            _refreshView.ZIndex = int.MaxValue;
            _refreshView.VerticalOptions = LayoutOptions.Start;
            _containerGrid.Children.Add(_refreshView);
            Grid.SetRow(_refreshView, 0);
            _refreshView.TranslationY = -pullToRefreshView.RefreshHeight;
        }
    }

    public void OnMainContentViewSet(View view)
    {
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
        if (handler != null && _contentView is CollectionView collectionView)
        {
            pullToRefreshView.InitializeCollectionView(collectionView);
            collectionView.BindingContext = pullToRefreshView.BindingContext;
        }
    }

    public void HandlePanStarted(double x, double y)
    {
        _wasScrolledOnTop = pullToRefreshView.IsScrolledOnTop;
        _startY = y;
    }

    public PullResult HandlePanMovement(double x, double y)
    {
        if (!_isPulling && y > _startY && _wasScrolledOnTop && pullToRefreshView.IsScrolledOnTop)
        {
            _isPulling = true;
        }

        if (!_isPulling || _refreshView == null)
        {
            return null;
        }

        var displacement = y - _startY;
        // Non-linear spring behavior - gets harder to pull as you go further
        var actualPullDistance = -pullToRefreshView.RefreshHeight + displacement / (1 + Math.Abs(springConstant * displacement / 100));
        // Clamp to maximum pull distance
        actualPullDistance = Math.Min(actualPullDistance, pullToRefreshView.MaxPullDistance);
        var newTranslationY = Math.Max(-pullToRefreshView.RefreshHeight, actualPullDistance);

        Console.WriteLine($"displacement  = {displacement}");
        Console.WriteLine($"actualPullDistance  = {actualPullDistance}");
        Console.WriteLine($"newTranslationY  = {newTranslationY}");

        _refreshView.TranslationY = newTranslationY;

        var state = newTranslationY < 0 ?  PullToRefreshState.Pulling : PullToRefreshState.ReleaseToRefresh;

        var percentage = newTranslationY / pullToRefreshView.RefreshHeight * 100;

        return new PullResult(state, percentage);
    }

    public PullResult HandlePanFinished(double x, double y)
    {
        if (!_isPulling || _refreshView == null)
        {
            return null;
        }

        var state = pullToRefreshView.State;
        if (state == PullToRefreshState.ReleaseToRefresh)
        {
           state = PullToRefreshState.Refreshing;
           var animation = new Animation(v => _refreshView.TranslationY = v,
               _refreshView.TranslationY,
               0);
           animation.Commit(pullToRefreshView, "TranslationYAnimation", 16, 250, Easing.CubicInOut);
           return new PullResult(state, 100);
        }

        if (state == PullToRefreshState.Pulling)
        {
            return new PullResult(PullToRefreshState.Canceled, 0);
        }

        return null;
    }

    public PullResult OnFinishedRefreshing(PullToRefreshState state)
    {
        _isPulling = false;

        if (_refreshView != null)
        {
            var animation = new Animation(v => _refreshView.TranslationY = v,
                _refreshView.TranslationY,
                -pullToRefreshView.RefreshHeight);
            animation.Commit(pullToRefreshView, "TranslationYAnimation", 16, 250, Easing.CubicInOut, finished: (_, _) =>
            {
                if (_contentView is CollectionView collectionView)
                {
                    MainThread.BeginInvokeOnMainThread(() => collectionView.ScrollTo(0));
                }
            });
            return new PullResult(PullToRefreshState.Canceled, 0);
        }

        return null;
    }
}
