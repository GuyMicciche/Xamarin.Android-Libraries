using Android.App;
using Android.OS;
using Android.Views;

using SlidingMenuLibrary;
using SlidingMenuLibrary.App;

namespace Example.Fragments
{
    [Activity(Label = "Responsive UI", Theme = "@style/ExampleTheme")]
    public class ResponsiveUIActivity : SlidingFragmentActivity
    {
        private Android.Support.V4.App.Fragment content;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(Resource.String.responsive_ui);

            SetContentView(Resource.Layout.responsive_content_frame);

            // check if the content frame contains the menu frame
            if (FindViewById(Resource.Id.menu_frame) == null)
            {
                SetBehindContentView(Resource.Layout.menu_frame);
                GetSlidingMenu().IsSlidingEnabled = true;
                GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_FULLSCREEN;
                ActionBar.SetDisplayHomeAsUpEnabled(true);
            }
            else
            {
                // add a dummy view
                var v = new View(this);
                SetBehindContentView(v);
                GetSlidingMenu().IsSlidingEnabled = false;
                GetSlidingMenu().TouchModeAbove = SlidingMenu.TOUCHMODE_NONE;
            }

            // set the Above View Fragment
            if (savedInstanceState != null)
            {
                content = SupportFragmentManager.GetFragment(savedInstanceState, "_content");
            }
            else
            {
                content = new BirdGridFragment(0);
            }
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, content)
                .Commit();

            // set the Behind View Fragment
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.menu_frame, new BirdMenuFragment())
                .Commit();

            // customize the SlidingMenu
            SlidingMenu sm = GetSlidingMenu();
            sm.BehindOffsetRes = Resource.Dimension.slidingmenu_offset;
            sm.ShadowWidthRes = Resource.Dimension.shadow_width;
            sm.ShadowDrawableResource = Resource.Drawable.shadow;
            sm.BehindScrollScale = 0.25f;
            sm.FadeDegree = 0.25f;

            // show the explanation dialog
            if (null == savedInstanceState)
            {
                new AlertDialog.Builder(this)
                    .SetTitle(Resource.String.what_is_this)
                    .SetMessage(Resource.String.responsive_explanation)
                    .Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Toggle();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            SupportFragmentManager.PutFragment(outState, "_content", content);
        }

        public void SwitchContent(Android.Support.V4.App.Fragment fragment)
        {
            content = fragment;
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, fragment)
                .Commit();
            var h = new Handler();
            h.PostDelayed(() => GetSlidingMenu().ShowContent(), 50);
        }

        public void OnBirdPressed(int pos)
        {
            var intent = BirdActivity.NewInstance(this, pos);
            StartActivity(intent);
        }
    }
}