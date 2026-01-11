
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
#if __IOS__
using UIKit;
#endif
using Color = Microsoft.Maui.Graphics.Color;

namespace Example.Helpers;

    public static class ShellEx
    {
        // NavBarBottomLineIsVisible Property
        public static readonly BindableProperty NavBarBottomLineIsVisibleProperty =
            BindableProperty.CreateAttached(
                "NavBarBottomLineIsVisible",
                typeof(bool?),
                typeof(ShellEx),
                null,
                propertyChanged: OnNavBarBottomLineIsVisibleChanged);

        public static bool? GetNavBarBottomLineIsVisible(BindableObject bindable)
            => (bool?)bindable.GetValue(NavBarBottomLineIsVisibleProperty);

        public static void SetNavBarBottomLineIsVisible(BindableObject bindable, bool? value)
            => bindable.SetValue(NavBarBottomLineIsVisibleProperty, value);

        // NavBarIsTransparent Property
        public static readonly BindableProperty NavBarIsTransparentProperty =
            BindableProperty.CreateAttached(
                "NavBarIsTransparent",
                typeof(bool?),
                typeof(ShellEx),
                null,
                propertyChanged: OnNavBarIsTransparentChanged);

        public static bool? GetNavBarIsTransparent(BindableObject bindable)
            => (bool?)bindable.GetValue(NavBarIsTransparentProperty);

        public static void SetNavBarIsTransparent(BindableObject bindable, bool? value)
            => bindable.SetValue(NavBarIsTransparentProperty, value);

        // NavBarBlurEffect Property
        public static readonly BindableProperty NavBarBlurEffectProperty =
            BindableProperty.CreateAttached(
                "NavBarBlurEffect",
                typeof(bool?),
                typeof(ShellEx),
                null,
                propertyChanged: OnNavBarBlurEffectChanged);

        public static bool? GetNavBarBlurEffect(BindableObject bindable)
            => (bool?)bindable.GetValue(NavBarBlurEffectProperty);

        public static void SetNavBarBlurEffect(BindableObject bindable, bool? value)
            => bindable.SetValue(NavBarBlurEffectProperty, value);

        // Property Changed Handlers
	    static void OnNavBarBottomLineIsVisibleChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Page page)
            {
                ApplyNavBarCustomizations(page);
            }
        }

	    static void OnNavBarIsTransparentChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Page page)
            {
                ApplyNavBarCustomizations(page);
            }
        }

	    static void OnNavBarBlurEffectChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Page page)
            {
                ApplyNavBarCustomizations(page);
            }
        }

	    static void ApplyNavBarCustomizations(Page page)
        {
            // If page is already loaded, apply immediately
            if (page.Handler != null)
            {
                ApplyiOSNavBarCustomizations(page);
            }
            else
            {
                // Wait for the page to load
                page.Loaded += (s, e) => ApplyiOSNavBarCustomizations(page);
            }
        }

	    static void ApplyiOSNavBarCustomizations(Page page)
        {
#if IOS
            var pageHandler = page.Handler as PageHandler;
            var viewController = pageHandler?.ViewController;
            var navigationController = viewController?.NavigationController;
            var navBar = navigationController?.NavigationBar;

            if (navBar == null) return;

            var appearance =  navBar.StandardAppearance;

            // Check if bottom line should be visible
            var bottomLineVisible = GetNavBarBottomLineIsVisible(page);
            if (bottomLineVisible == false)
            {
                appearance.ShadowColor = UIColor.Clear;
            }

            // Check blur effect
            /*var blurEffect = GetNavBarBlurEffect(page);
            if (blurEffect == false)
            {
                appearance.BackgroundEffect = null;
            }
            else
            {
	            appearance.BackgroundEffect = new UIBlurEffect();
            }*/

            navBar.StandardAppearance = appearance;
            navBar.ScrollEdgeAppearance = appearance;
#endif
        }
    }

    // Additional helper class for more specific customizations
    public static class ShellNavBarEx
    {
	    // Custom background color with alpha
	    public static readonly BindableProperty BackgroundColorWithAlphaProperty =
		    BindableProperty.CreateAttached(
			    "BackgroundColorWithAlpha",
			    typeof(Color),
			    typeof(ShellNavBarEx),
			    null,
			    propertyChanged: OnBackgroundColorWithAlphaChanged);

	    public static Color? GetBackgroundColorWithAlpha(BindableObject bindable)
		    => (Color?)bindable.GetValue(BackgroundColorWithAlphaProperty);

	    public static void SetBackgroundColorWithAlpha(BindableObject bindable, Color? value)
		    => bindable.SetValue(BackgroundColorWithAlphaProperty, value);

	    // Shadow opacity
	    public static readonly BindableProperty ShadowOpacityProperty =
		    BindableProperty.CreateAttached(
			    "ShadowOpacity",
			    typeof(double?),
			    typeof(ShellNavBarEx),
			    null,
			    propertyChanged: OnShadowOpacityChanged);

	    public static double? GetShadowOpacity(BindableObject bindable)
		    => (double?)bindable.GetValue(ShadowOpacityProperty);

	    public static void SetShadowOpacity(BindableObject bindable, double? value)
		    => bindable.SetValue(ShadowOpacityProperty, value);

	    static void OnBackgroundColorWithAlphaChanged(BindableObject bindable, object oldValue, object newValue)
	    {
		    if (bindable is Page page)
		    {
			    ApplyAdvancedCustomizations(page);
		    }
	    }

	    static void OnShadowOpacityChanged(BindableObject bindable, object oldValue, object newValue)
	    {
		    if (bindable is Page page)
		    {
			    ApplyAdvancedCustomizations(page);
		    }
	    }

	    static void ApplyAdvancedCustomizations(Page page)
	    {
		    if (page.Handler != null)
		    {
			    ApplyAdvancediOSCustomizations(page);
		    }
		    else
		    {
			    page.Loaded += (s, e) => ApplyAdvancediOSCustomizations(page);
		    }
	    }

	    static void ApplyAdvancediOSCustomizations(Page page)
	    {
#if IOS
		    var pageHandler = page.Handler as PageHandler;
		    var viewController = pageHandler?.ViewController;
		    var navigationController = viewController?.NavigationController;
		    var navBar = navigationController?.NavigationBar;

		    if (navBar == null) return;

		    var appearance = navBar.StandardAppearance ?? new UINavigationBarAppearance();

		    // Custom background color with alpha
		    var bgColor = GetBackgroundColorWithAlpha(page);
		    if (bgColor != null)
		    {
			    var uiColor = bgColor.ToPlatform();
			    appearance.BackgroundColor = uiColor;
		    }

		    // Shadow opacity
		    var shadowOpacity = GetShadowOpacity(page);
		    if (shadowOpacity.HasValue)
		    {
			    if (shadowOpacity.Value == 0)
			    {
				    appearance.ShadowColor = UIColor.Clear;
			    }
			    else
			    {
				    appearance.ShadowColor = UIColor.Black.ColorWithAlpha((nfloat)shadowOpacity.Value);
			    }
		    }

		    navBar.StandardAppearance = appearance;
		    navBar.ScrollEdgeAppearance = appearance;
#endif
	    }
    }
