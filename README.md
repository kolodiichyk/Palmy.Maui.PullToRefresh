# Palmy.Maui.PullToRefresh [![NuGet](https://img.shields.io/nuget/v/Palmy.Maui.PullToRefresh.svg?label=NuGet)](https://www.nuget.org/packages/Palmy.Maui.PullToRefresh/)

A highly customizable pull-to-refresh control for .NET MAUI that allows developers to replace the default refresh indicator with custom animated views.

| Car Demo | Shop Demo | Weather Demo |
|----------|-----------|--------------|
| ![Car](https://github.com/kolodiichyk/Palmy.Maui.PullToRefresh/blob/main/assets/car_refresh.gif) | ![Shop](https://github.com/kolodiichyk/Palmy.Maui.PullToRefresh/blob/main/assets/shop_refresh.gif) | ![Weather](https://github.com/kolodiichyk/Palmy.Maui.PullToRefresh/blob/main/assets/weather_refresh.gif) |

## Features

- Custom refresh indicator — use any MAUI View (Lottie animations, images, custom layouts)
- Two display modes: **Normal** (pushes content down) and **Overlay** (floats above content)
- Fine-grained pull state tracking via the `Pulling` event
- Configurable pull distance, refresh height, and animation easing
- MVVM-friendly with bindable `RefreshCommand` and `IsRefreshing` properties
- Supports Android and iOS

## Installation

```bash
dotnet add package Palmy.Maui.PullToRefresh
```

## Quick Start

### 1. Add the namespace

```xml
xmlns:pullToRefresh="clr-namespace:Palmy.Maui.PullToRefresh;assembly=Palmy.Maui.PullToRefresh"
```

### 2. Basic usage (Normal mode)

Wrap your scrollable content with `PullToRefreshView` and provide a custom `RefreshView`:

```xml
<pullToRefresh:PullToRefreshView IsRefreshing="{Binding IsRefreshing}"
                                 RefreshCommand="{Binding RefreshCommand}"
                                 MaxPullDistance="160"
                                 RefreshHeight="80"
                                 Type="Normal">

    <!-- Custom refresh indicator -->
    <pullToRefresh:PullToRefreshView.RefreshView>
        <Border BackgroundColor="#6200EE"
                HeightRequest="80"
                StrokeThickness="0">
            <ActivityIndicator IsRunning="True"
                               Color="White"
                               HorizontalOptions="Center"
                               VerticalOptions="Center" />
        </Border>
    </pullToRefresh:PullToRefreshView.RefreshView>

    <!-- Your scrollable content -->
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">
            <Label Text="Pull down to refresh!" FontSize="18" />
            <!-- More content here -->
        </VerticalStackLayout>
    </ScrollView>

</pullToRefresh:PullToRefreshView>
```

### 3. Overlay mode

Use `Type="Overlay"` to display the refresh indicator floating above your content:

```xml
<pullToRefresh:PullToRefreshView IsRefreshing="{Binding IsRefreshing}"
                                 RefreshCommand="{Binding RefreshCommand}"
                                 MaxPullDistance="420"
                                 RefreshHeight="120"
                                 Type="Overlay"
                                 AnimationTransition="{x:Static Easing.CubicInOut}">

    <pullToRefresh:PullToRefreshView.RefreshView>
        <Grid HeightRequest="120" IsClippedToBounds="True">
            <Border Padding="0"
                    BackgroundColor="White"
                    HeightRequest="60"
                    HorizontalOptions="Center"
                    StrokeShape="RoundRectangle 30,30,30,30"
                    StrokeThickness="0"
                    VerticalOptions="Center"
                    WidthRequest="60">
                <ActivityIndicator IsRunning="True"
                                   Color="#6200EE"
                                   HeightRequest="40"
                                   WidthRequest="40" />
            </Border>
        </Grid>
    </pullToRefresh:PullToRefreshView.RefreshView>

    <CollectionView ItemsSource="{Binding Items}">
        <!-- Item template -->
    </CollectionView>

</pullToRefresh:PullToRefreshView>
```

### 4. ViewModel

```csharp
public class MyViewModel : INotifyPropertyChanged
{
    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set { _isRefreshing = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }

    public MyViewModel()
    {
        RefreshCommand = new Command(async () => await OnRefreshAsync());
    }

    private async Task OnRefreshAsync()
    {
        IsRefreshing = true;

        // Load your data
        await Task.Delay(2000);

        IsRefreshing = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

### 5. Handling pull states with the `Pulling` event

Subscribe to `Pulling` to react to each phase of the gesture — useful for driving custom animations:

```xml
<pullToRefresh:PullToRefreshView Pulling="OnPulling" ...>
```

```csharp
private void OnPulling(object sender, PullToRefreshEventArgs e)
{
    switch (e.State)
    {
        case PullToRefreshState.Pulling:
            // User is dragging — e.Progress tracks how far (0–100+)
            AnimationView.Progress = (float)e.Progress / 100;
            break;
        case PullToRefreshState.ReleaseToRefresh:
            // Passed the threshold — user can release to trigger refresh
            break;
        case PullToRefreshState.Refreshing:
            // Refresh started — play a looping animation
            AnimationView.Play();
            break;
        case PullToRefreshState.Finished:
        case PullToRefreshState.Canceled:
            // Done or canceled — stop animation
            AnimationView.Stop();
            break;
    }
}
```

## API Reference

| Property | Type | Default | Description |
|---|---|---|---|
| `IsRefreshing` | `bool` | `false` | Controls whether the refresh is active. Set to `false` to complete. |
| `RefreshCommand` | `ICommand` | `null` | Command executed when pull threshold is reached. |
| `RefreshView` | `View` | `null` | Custom view displayed as the refresh indicator. |
| `Type` | `PullToRefreshType` | `Normal` | `Normal` pushes content down; `Overlay` floats above content. |
| `MaxPullDistance` | `double` | `200` | Maximum distance (in pixels) the user can pull. |
| `RefreshHeight` | `double` | `100` | Height of the refresh indicator area. |
| `AnimationTransition` | `Easing` | `Easing.Default` | Easing function for state transition animations. |
| `State` | `PullToRefreshState` | — | *(Read-only)* Current state of the pull gesture. |

| Event | Args | Description |
|---|---|---|
| `Pulling` | `PullToRefreshEventArgs` | Fired during the pull gesture with `State` and `Progress`. |

### PullToRefreshState values

| State | Meaning |
|---|---|
| `Pulling` | User is dragging down |
| `ReleaseToRefresh` | Pulled past the threshold — releasing will trigger refresh |
| `Released` | User released the gesture |
| `Refreshing` | Refresh operation in progress |
| `Canceled` | Pull was canceled (didn't reach threshold) |
| `Finished` | Refresh completed |

## Platforms

| Platform | Minimum Version |
|---|---|
| Android | 21 (Lollipop) |
| iOS | 15.0 |

## License

MIT

