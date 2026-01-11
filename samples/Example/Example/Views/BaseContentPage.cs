using Example.ViewModels;

namespace Example.Views;

public abstract class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel
{
    public BaseContentPage(TViewModel viewModel)
    {
        base.BindingContext = viewModel;
    }

    public new TViewModel BindingContext => (TViewModel)base.BindingContext;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BindingContext.OnAppearing();
    }
}
