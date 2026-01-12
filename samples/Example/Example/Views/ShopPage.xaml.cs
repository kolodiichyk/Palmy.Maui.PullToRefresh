using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.ViewModels;
using Palmy.Maui.PullToRefresh;
using Palmy.Maui.PullToRefresh.Enums;

namespace Example.Views;

public partial class ShopPage : BaseContentPage<ShopViewModel>
{
    private PullToRefreshState _oldState;

    public ShopPage(ShopViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    private async void PullToRefreshView_OnPulling(object sender, PullToRefreshEventArgs e)
    {
        switch (e.State)
        {
            case PullToRefreshState.Refreshing:
                RefreshViewArrowImage.IsVisible = false;
                AnimationView.Play();
                break;
            case PullToRefreshState.Finished:
            case PullToRefreshState.Canceled:
                AnimationView.Stop();
                RefreshViewArrowImage.IsVisible = true;
                break;
        }

        await RotateRefreshViewArrowImage(e.State, _oldState);
        _oldState = e.State;
    }

    private async Task RotateRefreshViewArrowImage(PullToRefreshState state, PullToRefreshState oldState)
    {
        if (oldState == state)
            return;

        if (state == PullToRefreshState.Finished || state == PullToRefreshState.Canceled || state == PullToRefreshState.Pulling)
        {
            await RefreshViewArrowImage.RotateToAsync(0);
        }

        if (state == PullToRefreshState.ReleaseToRefresh)
        {
            await RefreshViewArrowImage.RotateToAsync(180);
        }
    }
}
