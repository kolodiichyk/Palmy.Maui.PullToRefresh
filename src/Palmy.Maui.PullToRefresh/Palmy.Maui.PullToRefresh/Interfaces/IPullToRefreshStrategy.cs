using Palmy.Maui.PullToRefresh.Enums;
using Palmy.Maui.PullToRefresh.Strategies;

namespace Palmy.Maui.PullToRefresh.Interfaces;

internal interface IPullToRefreshStrategy : IDisposable
{
    void Initialize();
    void OnRefreshViewSet(View view);
    void OnMainContentViewSet(View view);
    void OnHandlerChanged(IViewHandler handler);

    void HandlePanStarted(double x, double y);
    PullResult? HandlePanMovement(double x, double y);
    PullResult? HandlePanFinished(double x, double y);
    PullResult? OnFinishedRefreshing(PullToRefreshState state);

    /// <summary>
    /// Cancels any running animations on the strategy
    /// </summary>
    void CancelAnimations();
}
