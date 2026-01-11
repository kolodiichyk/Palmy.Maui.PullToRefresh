using Palmy.Maui.PullToRefresh.Enums;

namespace Palmy.Maui.PullToRefresh;

public class PullToRefreshEventArgs(PullToRefreshState state, double progress) : EventArgs
{
	public PullToRefreshState State { get; } = state;
	public double Progress { get; } = progress;
}
