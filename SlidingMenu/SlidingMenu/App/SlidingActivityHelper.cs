using Android.App;
using Android.OS;
using Android.Views;

using Java.Lang;

namespace SlidingMenuLibrary.App
{
	public class SlidingActivityHelper
	{
		private Activity mActivity;
		private SlidingMenu mSlidingMenu;
		private View mViewAbove;
		private View mViewBehind;
		private bool mBroadcasting = false;
		private bool mOnPostCreateCalled = false;
		private bool mEnableSlide = true;

		/// <summary>
		/// Instantiates a new SlidingActivityHelper.
		/// </summary>
		/// <param name="activity"> the associated activity </param>
		public SlidingActivityHelper(Activity activity)
		{
			mActivity = activity;
		}

		/// <summary>
		/// Sets mSlidingMenu as a newly inflated SlidingMenu. Should be called within the activitiy's onCreate()
		/// </summary>
		/// <param name="savedInstanceState"> the saved instance state (unused) </param>
		public void OnCreate(Bundle savedInstanceState)
		{
			mSlidingMenu = (SlidingMenu)LayoutInflater.From(mActivity).Inflate(Resource.Layout.slidingmenumain, null);
		}

		/// <summary>
		/// Further SlidingMenu initialization. Should be called within the activitiy's onPostCreate()
		/// </summary>
		/// <param name="savedInstanceState"> the saved instance state (unused) </param>
		public void OnPostCreate(Bundle savedInstanceState)
		{
			if (mViewBehind == null || mViewAbove == null)
			{
				throw new IllegalStateException("Both setBehindContentView must be called " + "in onCreate in addition to setContentView.");
			}

			mOnPostCreateCalled = true;

			mSlidingMenu.AttachToActivity(mActivity, mEnableSlide ? SlidingMenu.SLIDING_WINDOW : SlidingMenu.SLIDING_CONTENT);

			bool open;
			bool secondary;
			if (savedInstanceState != null)
			{
				open = savedInstanceState.GetBoolean("SlidingActivityHelper.open");
				secondary = savedInstanceState.GetBoolean("SlidingActivityHelper.secondary");
			}
			else
			{
				open = false;
				secondary = false;
			}
            (new Handler()).Post(() =>
                {
                    if (open)
                    {
                        if (secondary)
                        {
                            mSlidingMenu.ShowSecondaryMenu(false);
                        }
                        else
                        {
                            mSlidingMenu.ShowMenu(false);
                        }
                    }
                    else
                    {
                        mSlidingMenu.ShowContent(false);
                    }
                });
		}

		/// <summary>
		/// Controls whether the ActionBar slides along with the above view when the menu is opened,
		/// or if it stays in place.
		/// </summary>
		/// <param name="slidingActionBarEnabled"> True if you want the ActionBar to slide along with the SlidingMenu,
		/// false if you want the ActionBar to stay in place </param>
		public bool SlidingActionBarEnabled
		{
			set
			{
				if (mOnPostCreateCalled)
				{
					throw new IllegalStateException("enableSlidingActionBar must be called in onCreate.");
				}
				mEnableSlide = value;
			}
		}

		/// <summary>
		/// Finds a view that was identified by the id attribute from the XML that was processed in onCreate(Bundle).
		/// </summary>
		/// <param name="id"> the resource id of the desired view </param>
		/// <returns> The view if found or null otherwise. </returns>
		public View FindViewById(int id)
		{
			View v;
			if (mSlidingMenu != null)
			{
				v = mSlidingMenu.FindViewById(id);
				if (v != null)
				{
					return v;
				}
			}
			return null;
		}

		/// <summary>
		/// Called to retrieve per-instance state from an activity before being killed so that the state can be
		/// restored in onCreate(Bundle) or onRestoreInstanceState(Bundle) (the Bundle populated by this method
		/// will be passed to both). 
		/// </summary>
		/// <param name="outState"> Bundle in which to place your saved state. </param>
		public void OnSaveInstanceState(Bundle outState)
		{
			outState.PutBoolean("SlidingActivityHelper.open", mSlidingMenu.IsMenuShowing);
			outState.PutBoolean("SlidingActivityHelper.secondary", mSlidingMenu.IsSecondaryMenuShowing);
		}

		/// <summary>
		/// Register the above content view.
		/// </summary>
		/// <param name="v"> the above content view to register </param>
		/// <param name="params"> LayoutParams for that view (unused) </param>
		public void RegisterAboveContentView(View v, ViewGroup.LayoutParams lp)
		{
			if (!mBroadcasting)
			{
				mViewAbove = v;
			}
		}

		/// <summary>
		/// Set the activity content to an explicit view. This view is placed directly into the activity's view
		/// hierarchy. It can itself be a complex view hierarchy. When calling this method, the layout parameters
		/// of the specified view are ignored. Both the width and the height of the view are set by default to
		/// MatchParent. To use your own layout parameters, invoke setContentView(android.view.View,
		/// android.view.ViewGroup.LayoutParams) instead.
		/// </summary>
		/// <param name="v"> The desired content to display. </param>
		public View ContentView
		{
			set
			{
				mBroadcasting = true;
				mActivity.SetContentView(value);
			}
		}

		/// <summary>
		/// Set the behind view content to an explicit view. This view is placed directly into the behind view 's view hierarchy.
		/// It can itself be a complex view hierarchy.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
		/// <param name="layoutParams"> Layout parameters for the view. (unused) </param>
		public void SetBehindContentView(View view, ViewGroup.LayoutParams layoutParams)
		{
			mViewBehind = view;
			mSlidingMenu.SetMenu(mViewBehind);
		}

		/// <summary>
		/// Gets the SlidingMenu associated with this activity.
		/// </summary>
		/// <returns> the SlidingMenu associated with this activity. </returns>
        public SlidingMenu GetSlidingMenu()
        {
            return mSlidingMenu;
        }

		/// <summary>
		/// Toggle the SlidingMenu. If it is open, it will be closed, and vice versa.
		/// </summary>
		public void Toggle()
		{
			mSlidingMenu.Toggle();
		}

		/// <summary>
		/// Close the SlidingMenu and show the content view.
		/// </summary>
		public void ShowContent()
		{
			mSlidingMenu.ShowContent();
		}

		/// <summary>
		/// Open the SlidingMenu and show the menu view.
		/// </summary>
		public void ShowMenu()
		{
			mSlidingMenu.ShowMenu();
		}

		/// <summary>
		/// Open the SlidingMenu and show the secondary menu view. Will default to the regular menu
		/// if there is only one.
		/// </summary>
		public void ShowSecondaryMenu()
		{
			mSlidingMenu.ShowSecondaryMenu();
		}

		/// <summary>
		/// On key up.
		/// </summary>
		/// <param name="keyCode"> the key code </param>
		/// <param name="e"> the event </param>
		/// <returns> true, if successful </returns>
        public bool OnKeyUp(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back && mSlidingMenu.IsMenuShowing)
			{
				ShowContent();
				return true;
			}
			return false;
		}
	}
}