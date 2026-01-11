using System.Collections.ObjectModel;
using Example.Models;
using Example.Services;

namespace Example.ViewModels;

public class ShopViewModel : BaseViewModel
{
    public ShopViewModel()
    {
        RefreshCommand = new Command(OnRefresh);
    }

    private async void OnRefresh()
    {
        IsRefreshing = true;
        OnPropertyChanged(nameof(IsRefreshing));
        await Task.Delay(3000);

        IsRefreshing = false;
        OnPropertyChanged(nameof(IsRefreshing));

        await Task.Delay(250);

        foreach (var item in  ShopItemGenerator.GenerateMultiple(5))
        {
            Products.Insert(0, item);
        }

    }

    public bool IsRefreshing { get; set; }
    public ObservableCollection<ShopItem> Products { get; set; } = new ObservableCollection<ShopItem>();

    public Command RefreshCommand { get; }

    public override void OnAppearing()
    {
        Products = new ObservableCollection<ShopItem>(ShopItemGenerator.GenerateMultiple(25));
        OnPropertyChanged(nameof(Products));
    }
}
