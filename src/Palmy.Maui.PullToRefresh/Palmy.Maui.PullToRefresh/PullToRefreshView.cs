using System.Runtime.CompilerServices;
using System.Windows.Input;
using Palmy.Maui.PullToRefresh.Enums;
using Palmy.Maui.PullToRefresh.Interfaces;
using Palmy.Maui.PullToRefresh.Strategies;

namespace Palmy.Maui.PullToRefresh;

public partial class PullToRefreshView : ContentView
{
	IPullToRefreshStrategy _strategy;
	internal bool IsScrolledOnTop { get; set; } = true;

	public PullToRefreshState State { get; private set; } = PullToRefreshState.Finished;

	public static readonly BindableProperty RefreshCommandProperty =
		BindableProperty.Create(nameof(RefreshCommand), typeof(ICommand), typeof(PullToRefreshView));

	public static readonly BindableProperty IsRefreshingProperty =
		BindableProperty.Create(nameof(IsRefreshing), typeof(bool), typeof(PullToRefreshView), false,
			propertyChanged: OnIsRefreshingChanged);

	public static readonly BindableProperty MaxPullDistanceProperty =
    		BindableProperty.Create(nameof(MaxPullDistance), typeof(double), typeof(PullToRefreshView), 200d);

	public static readonly BindableProperty RefreshHeightProperty =
		BindableProperty.Create(nameof(RefreshHeight), typeof(double), typeof(PullToRefreshView), 100d);

	public static readonly BindableProperty RefreshViewProperty =
		BindableProperty.Create(
			nameof(RefreshView),
			typeof(View),
			typeof(PullToRefreshView),
			propertyChanged: OnRefreshViewChanged);

	public static readonly BindableProperty TypeProperty =
		BindableProperty.Create(nameof(Type), typeof(PullToRefreshType), typeof(PullToRefreshView), PullToRefreshType.Normal, propertyChanged:OnTypePropertyChanged);

	public PullToRefreshView()
	{
		SetStrategy(new Normal(this));
	}

	public event EventHandler<PullToRefreshEventArgs>? Pulling;

	public ICommand RefreshCommand
	{
		get => (ICommand)GetValue(RefreshCommandProperty);
		set => SetValue(RefreshCommandProperty, value);
	}

	public bool IsRefreshing
	{
		get => (bool)GetValue(IsRefreshingProperty);
		set => SetValue(IsRefreshingProperty, value);
	}

	public double MaxPullDistance
	{
		get => (double)GetValue(MaxPullDistanceProperty);
		set => SetValue(MaxPullDistanceProperty, value);
	}

	public PullToRefreshType Type
	{
		get => (PullToRefreshType)GetValue(TypeProperty);
		set => SetValue(TypeProperty, value);
	}

	public double RefreshHeight
	{
		get => (double)GetValue(RefreshHeightProperty);
		set => SetValue(RefreshHeightProperty, value);
	}

	public View RefreshView
	{
		get => (View)GetValue(RefreshViewProperty);
		set => SetValue(RefreshViewProperty, value);
	}

	static void OnIsRefreshingChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is PullToRefreshView { IsRefreshing: false } pullToRefreshView)
			 pullToRefreshView.OnFinishedRefreshing(PullToRefreshState.Finished);
	}

	protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName == nameof(Content))
		{
			_strategy.OnMainContentViewSet(Content);
		}

		if (propertyName == nameof(SafeAreaEdges) && Content is Layout layout)
		{
			layout.SafeAreaEdges = SafeAreaEdges;
		}
	}

	static void OnRefreshViewChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is PullToRefreshView control)
		{
			control._strategy.OnRefreshViewSet((View)newValue);
		}
	}

	private static void OnTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is PullToRefreshView control)
		{
			switch ((PullToRefreshType)newValue)
			{
				case PullToRefreshType.Normal:
					control.SetStrategy(new Normal(control));
					break;
				case PullToRefreshType.Overlay:
					control.SetStrategy(new Overlay(control));
					break;
			}
		}
	}

	private void SetStrategy(IPullToRefreshStrategy strategy)
	{
		_strategy = strategy;
		_strategy.Initialize();
	}

	internal void OnInterceptPanUpdated(PanUpdatedEventArgs e)
	{
		if (State == PullToRefreshState.Refreshing)
			return;

		switch (e.StatusType)
		{
			case GestureStatus.Started:
				_strategy.HandlePanStarted(e.TotalX, e.TotalY);
				break;
			case GestureStatus.Running:
				var runningResult = _strategy.HandlePanMovement(e.TotalX, e.TotalY);
				if (runningResult != null)
				{
					State = runningResult.State;
					Pulling?.Invoke(this, new PullToRefreshEventArgs(runningResult.State, runningResult.Percentage));
				}

				break;
			case GestureStatus.Completed:
			case GestureStatus.Canceled:
				var finishedResult = _strategy.HandlePanFinished(e.TotalX, e.TotalY);
				if (finishedResult != null)
				{
					State = finishedResult.State;
					Pulling?.Invoke(this, new PullToRefreshEventArgs(finishedResult.State, finishedResult.Percentage));

					if (State == PullToRefreshState.Refreshing)
					{
						RefreshCommand?.Execute(null);
					}

					if (State == PullToRefreshState.Canceled)
					{
						OnFinishedRefreshing(PullToRefreshState.Canceled);
					}
				}
				break;
		}
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();
		_strategy.OnHandlerChanged(Handler);
	}

	void OnFinishedRefreshing(PullToRefreshState state)
	{
		var result = _strategy.OnFinishedRefreshing(state);
		if (result != null)
		{
			State = result.State;
			Pulling?.Invoke(this, new PullToRefreshEventArgs(result.State, result.Percentage));
		}
	}
}
