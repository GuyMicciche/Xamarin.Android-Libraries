using Android.App;
using Android.OS;
using Android.Support.V4.App;
using SlidingMenuLibrary;

namespace Example
{
    [Activity(Label = "Attach Example", Theme = "@style/ExampleTheme")]
    public class AttachExample : FragmentActivity
    {
        private SlidingMenu menu;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(Resource.String.attach);

            SetContentView(Resource.Layout.content_frame);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, new SampleListFragment())
                .Commit();

            menu = new SlidingMenu(this)
                {
                    TouchModeAbove = SlidingMenu.TOUCHMODE_FULLSCREEN,
                    ShadowWidthRes = Resource.Dimension.shadow_width,
                    ShadowDrawableResource = Resource.Drawable.shadow,
                    BehindWidthRes = Resource.Dimension.slidingmenu_offset,
                    FadeDegree = 0.35f
                };
            menu.AttachToActivity(this, SlidingMenu.SLIDING_CONTENT);
            menu.SetMenu(Resource.Layout.menu_frame);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.menu_frame, new SampleListFragment())
                .Commit();
        }

        public override void OnBackPressed()
        {
            if (menu.IsMenuShowing)
            {
                menu.ShowContent();
            }
            else
            {
                base.OnBackPressed();
            }
        }
    }
}