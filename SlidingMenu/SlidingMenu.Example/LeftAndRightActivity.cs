using Android.App;
using Android.OS;

using SlidingMenuLibrary;

namespace Example
{
    [Activity(Label = "Left and Right", Theme = "@style/ExampleTheme")]
    public class LeftAndRightActivity : BaseActivity
    {
        public LeftAndRightActivity() 
            : base(Resource.String.left_and_right)
        {
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            GetSlidingMenu().Mode = SlidingMenu.LEFT_RIGHT;
            GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_FULLSCREEN;
            
            SetContentView(Resource.Layout.content_frame);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, new SampleListFragment())
                .Commit();

            GetSlidingMenu().SetSecondaryMenu(Resource.Layout.menu_frame_two);
            GetSlidingMenu().SecondaryShadowDrawableResource = Resource.Drawable.shadowright;
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.menu_frame_two, new SampleListFragment())
                .Commit();
        }
    }
}