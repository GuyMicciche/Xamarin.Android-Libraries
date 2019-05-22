using Android.OS;
using Android.Views;
using SlidingMenuLibrary;
using SlidingMenuLibrary.App;
using ListFragment = Android.Support.V4.App.ListFragment;

namespace Example
{
    public class BaseActivity : SlidingFragmentActivity
    {
        private readonly int mTitleRes;
        protected ListFragment Frag;

        public BaseActivity(int titleRes)
        {
            mTitleRes = titleRes;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetTitle(mTitleRes);

            // set the Behind View
            SetBehindContentView(Resource.Layout.menu_frame);
            if (null == savedInstanceState)
            {
                var t = SupportFragmentManager.BeginTransaction();
                Frag = new SampleListFragment();
                t.Replace(Resource.Id.menu_frame, Frag);
                t.Commit();
            }
            else
            {
                Frag = (ListFragment)SupportFragmentManager.FindFragmentById(Resource.Id.menu_frame);
            }

            // customize the SlidingMenu
            SlidingMenu sm = GetSlidingMenu();
            sm.ShadowWidthRes = Resource.Dimension.shadow_width;
            sm.BehindOffsetRes = Resource.Dimension.slidingmenu_offset;
            sm.ShadowDrawableResource = Resource.Drawable.shadow;
            sm.FadeDegree = 0.25f;
            sm.TouchModeAbove = SlidingMenu.TOUCHMODE_FULLSCREEN;

            ActionBar.SetDisplayHomeAsUpEnabled(true);
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
    }
}