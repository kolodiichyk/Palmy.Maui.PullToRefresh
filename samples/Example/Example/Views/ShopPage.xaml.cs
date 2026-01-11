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
    public ShopPage(ShopViewModel viewModel) : base(viewModel)
    {
        InitializeComponent();
    }

    private void PullToRefreshView_OnPulling(object sender, PullToRefreshEventArgs e)
    {
        switch (e.State)
        {
            case PullToRefreshState.Pulling:
                RefreshViewFontImageSource.Glyph = "circle-down";
                break;
            case PullToRefreshState.ReleaseToRefresh:
                RefreshViewFontImageSource.Glyph = "circle-up";
                break;
            case PullToRefreshState.Released:
                break;
            case PullToRefreshState.Refreshing:
                RefreshViewFontImageSource.Glyph = null;
                AnimationView.Play();
                break;
            case PullToRefreshState.Finished:
            case PullToRefreshState.Canceled:
                AnimationView.Stop();
                RefreshViewFontImageSource.Glyph = "circle-down";
                break;
        }
    }
}
