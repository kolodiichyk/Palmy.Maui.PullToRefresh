using System.Collections.ObjectModel;
using Example.Models;
using Example.Services;

namespace Example.ViewModels;

public class CarsViewModel : BaseViewModel
{
    public CarsViewModel()
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

        foreach (var carItem in  CarItemGenerator.GenerateMultiple(5))
        {
            Cars.Insert(0, carItem);
        }

    }

    public bool IsRefreshing { get; set; }
    public ObservableCollection<CarItem> Cars { get; set; } = new ObservableCollection<CarItem>();

    public Command RefreshCommand { get; }

    public override void OnAppearing()
    {
        Cars = new ObservableCollection<CarItem>(CarItemGenerator.GenerateMultiple(25));
        OnPropertyChanged(nameof(Cars));
    }
}
