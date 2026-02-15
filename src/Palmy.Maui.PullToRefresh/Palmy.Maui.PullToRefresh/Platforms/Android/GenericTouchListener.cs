using Android.Views;
using View = Android.Views.View;

namespace Palmy.Maui.PullToRefresh;

public class GenericTouchListener(PullToRefreshView pullToRefreshView) : Java.Lang.Object, View.IOnTouchListener
{
    private readonly WeakReference<PullToRefreshView>? _pullToRefreshViewRef = new(pullToRefreshView);

    private PullToRefreshView? PullToRefreshView =>
        _pullToRefreshViewRef?.TryGetTarget(out var view) == true ? view : null;

    public bool OnTouch(View? v, MotionEvent? e)
    {
        if (e == null)
            return false;

        var pullToRefreshView = PullToRefreshView;
        if (pullToRefreshView == null)
            return false;

        var x = ConvertToDp(e.GetX());
        var y = ConvertToDp(e.GetY());

        GestureStatus gestureStatus = e.Action switch
        {
            MotionEventActions.Down => GestureStatus.Started,
            MotionEventActions.Move => GestureStatus.Running,
            MotionEventActions.Cancel => GestureStatus.Canceled,
            MotionEventActions.Up => GestureStatus.Completed,
            _ => GestureStatus.Canceled
        };

        pullToRefreshView.OnInterceptPanUpdated(
            new PanUpdatedEventArgs(gestureStatus, 1, x, y));

        return false;
    }

    private static double ConvertToDp(double value)
    {
        var density = DeviceDisplay.MainDisplayInfo.Density;
        return value / density;
    }
}
