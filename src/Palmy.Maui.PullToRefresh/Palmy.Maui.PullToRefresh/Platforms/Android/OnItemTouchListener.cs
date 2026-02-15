using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace Palmy.Maui.PullToRefresh;

public class OnItemTouchListener(PullToRefreshView pullToRefreshView, GenericTouchListener genericTouchListener)
    : Java.Lang.Object, RecyclerView.IOnItemTouchListener
{
    private bool _disallow;
    private readonly WeakReference<PullToRefreshView>? _pullToRefreshViewRef = new(pullToRefreshView);
    private readonly WeakReference<GenericTouchListener>? _genericTouchListenerRef = new(genericTouchListener);

    private PullToRefreshView? PullToRefreshView =>
        _pullToRefreshViewRef?.TryGetTarget(out var view) == true ? view : null;

    private GenericTouchListener? GenericTouchListener =>
        _genericTouchListenerRef?.TryGetTarget(out var view) == true ? view : null;

    public bool OnInterceptTouchEvent(RecyclerView recyclerView, MotionEvent @event)
    {
        var pullToRefreshView = PullToRefreshView;
        if (pullToRefreshView == null)
            return false;

        if (@event.Action == MotionEventActions.Down)
        {
            GenericTouchListener?.OnTouch(recyclerView, @event);
        }

        return _disallow;
    }

    public void OnRequestDisallowInterceptTouchEvent(bool disallow)
    {
        _disallow = disallow;
    }

    public void OnTouchEvent(RecyclerView recyclerView, MotionEvent @event)
    {
    }
}
