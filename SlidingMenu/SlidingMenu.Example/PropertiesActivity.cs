using Android.App;
using Android.OS;
using Android.Widget;

using SlidingMenuLibrary;

namespace Example
{
    [Activity(Label = "Properties", Theme = "@style/ExampleTheme")]
    public class PropertiesActivity : BaseActivity
    {
        public PropertiesActivity() 
            : base(Resource.String.properties)
        { 
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SlidingActionBarEnabled = true;

            SetContentView(Resource.Layout.properties);

            // left and right modes
            var mode = FindViewById<RadioGroup>(Resource.Id.mode);
            mode.Check(Resource.Id.left);
            mode.CheckedChange += (sender, args) =>
                {
                    SlidingMenu sm = GetSlidingMenu();
                    switch (args.CheckedId)
                    {
                        case Resource.Id.left:
                            sm.Mode = SlidingMenu.LEFT;
                            sm.ShadowDrawableResource = Resource.Drawable.shadow;
                            break;
                        case Resource.Id.right:
                            sm.Mode = SlidingMenu.RIGHT;
                            sm.ShadowDrawableResource = Resource.Drawable.shadowright;
                            break;
                        case Resource.Id.left_right:
                            sm.Mode = SlidingMenu.LEFT_RIGHT;
                            sm.SetSecondaryMenu(Resource.Layout.menu_frame_two);
                            SupportFragmentManager
                                .BeginTransaction()
                                .Replace(Resource.Id.menu_frame_two, new SampleListFragment())
                                .Commit();
                            sm.SecondaryShadowDrawableResource = Resource.Drawable.shadowright;
                            sm.ShadowDrawableResource = Resource.Drawable.shadow;
                            break;
                    }
                };

            // touch mode stuff
            var touchAbove = FindViewById<RadioGroup>(Resource.Id.touch_above);
            touchAbove.Check(Resource.Id.touch_above_full);
            touchAbove.CheckedChange += (sender, args) =>
                {
                    switch (args.CheckedId)
                    {
                        case Resource.Id.touch_above_full:
                            GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_FULLSCREEN;
                            break;
                        case Resource.Id.touch_above_margin:
                            GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_MARGIN;
                            break;
                        case Resource.Id.touch_above_none:
                            GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_NONE;
                            break;
                    }
                };

            // scroll scale stuff
            var scrollScale = FindViewById<SeekBar>(Resource.Id.scroll_scale);
            scrollScale.Max = 1000;
            scrollScale.Progress = 333;
            scrollScale.StopTrackingTouch += (sender, args) =>
                {
                    GetSlidingMenu().BehindScrollScale = (float)args.SeekBar.Progress / args.SeekBar.Max;
                };

            // behind width stuff
            var behindWidth = FindViewById<SeekBar>(Resource.Id.behind_width);
            behindWidth.Max = 1000;
            behindWidth.Progress = 750;
            behindWidth.StopTrackingTouch += (sender, args) =>
                {
                    var percent = (float) args.SeekBar.Progress / args.SeekBar.Max;
                    GetSlidingMenu().BehindWidth = (int)(percent * GetSlidingMenu().Width);
                    GetSlidingMenu().RequestLayout();
                };

            // shadow stuff
            var shadowEnabled = FindViewById<CheckBox>(Resource.Id.shadow_enabled);
            shadowEnabled.Checked = true;
            shadowEnabled.CheckedChange += (sender, args) =>
                {
                    if (args.IsChecked)
                        GetSlidingMenu().ShadowDrawableResource = GetSlidingMenu().Mode == SlidingMenu.LEFT
                                                            ? Resource.Drawable.shadow
                                                            : Resource.Drawable.shadowright;
                    else
                        GetSlidingMenu().ShadowDrawable = null;
                };
            var shadowWidth = FindViewById<SeekBar>(Resource.Id.shadow_width);
            shadowWidth.Max = 1000;
            shadowWidth.Progress = 75;
            shadowWidth.StopTrackingTouch += (sender, args) =>
                {
                    var percent = (float)args.SeekBar.Progress / args.SeekBar.Max;
                    var width = (int)(percent * GetSlidingMenu().Width);
                    GetSlidingMenu().ShadowWidth = width;
                    GetSlidingMenu().Invalidate();
                };

            // fading stuff
            var fadeEnabled = FindViewById<CheckBox>(Resource.Id.fade_enabled);
            fadeEnabled.Checked = true;
            fadeEnabled.CheckedChange += (sender, args) =>
                {
                    GetSlidingMenu().FadeEnabled = args.IsChecked;
                };

            var fadeDeg = FindViewById<SeekBar>(Resource.Id.fade_degree);
            fadeDeg.Max = 1000;
            fadeDeg.Progress = 666;
            fadeDeg.StopTrackingTouch += (sender, args) =>
                {
                    GetSlidingMenu().FadeDegree = (float)args.SeekBar.Progress / args.SeekBar.Max;
                };
        }
    }
}