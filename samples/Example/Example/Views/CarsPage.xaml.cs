using Example.ViewModels;
using Palmy.Maui.PullToRefresh;
using Palmy.Maui.PullToRefresh.Enums;

namespace Example.Views;

public partial class CarsPage : BaseContentPage<CarsViewModel>
{
    public CarsPage(CarsViewModel viewModel) : base(viewModel)
    {
       InitializeComponent();
    }

    private void PullToRefreshView_OnPulling(object sender, PullToRefreshEventArgs e)
    {
        var progress = (float)e.Progress / 100;
        if (progress > 1)
            progress -= 1;

        switch (e.State)
        {
            case PullToRefreshState.Pulling:
            case PullToRefreshState.ReleaseToRefresh:
            case PullToRefreshState.Released:
                AnimationView.Progress = progress;
                break;
            case PullToRefreshState.Refreshing:
                AnimationView.Play();
                break;
            case PullToRefreshState.Finished:
                AnimationView.Stop();
                break;
            case PullToRefreshState.Canceled:
                AnimationView.Stop();
                break;
        }
    }
}
