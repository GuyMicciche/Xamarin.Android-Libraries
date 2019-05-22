using Android.App;
using Android.OS;
using Android.Views;

namespace SlidingMenuLibrary.App
{
	public class SlidingActivity : Activity, ISlidingActivityBase
	{

		private SlidingActivityHelper mHelper;

		/* (non-Javadoc)
		 * @see android.app.Activity#onCreate(android.os.Bundle)
		 */
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			mHelper = new SlidingActivityHelper(this);
			mHelper.OnCreate(savedInstanceState);
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#onPostCreate(android.os.Bundle)
		 */
        protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			mHelper.OnPostCreate(savedInstanceState);
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#findViewById(int)
		 */
		public override View FindViewById(int id)
		{
			View v = base.FindViewById(id);
			if (v != null)
			{
				return v;
			}
			return mHelper.FindViewById(id);
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#onSaveInstanceState(android.os.Bundle)
		 */
		protected override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);
			mHelper.OnSaveInstanceState(outState);
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#setContentView(int)
		 */
		public override void SetContentView(int id)
		{
			SetContentView(LayoutInflater.Inflate(id, null));
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#setContentView(android.view.View)
		 */
		public override void SetContentView(View v)
		{
            SetContentView(v, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#setContentView(android.view.View, android.view.ViewGroup.LayoutParams)
		 */
        public override void SetContentView(View v, ViewGroup.LayoutParams lp)
		{
			base.SetContentView(v, lp);
			mHelper.RegisterAboveContentView(v, lp);
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#setBehindContentView(int)
		 */
		public void SetBehindContentView(int id)
		{
			SetBehindContentView(LayoutInflater.Inflate(id, null));
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#setBehindContentView(android.view.View)
		 */
		public void SetBehindContentView(View v)
		{
            SetBehindContentView(v, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#setBehindContentView(android.view.View, android.view.ViewGroup.LayoutParams)
		 */
        public void SetBehindContentView(View v, ViewGroup.LayoutParams lp)
		{
			mHelper.SetBehindContentView(v, lp);
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#getSlidingMenu()
		 */
        public SlidingMenu GetSlidingMenu()
        {
            return mHelper.GetSlidingMenu();
        }

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#toggle()
		 */
		public void Toggle()
		{
			mHelper.Toggle();
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#showAbove()
		 */
		public void ShowContent()
		{
			mHelper.ShowContent();
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#showBehind()
		 */
		public void ShowMenu()
		{
			mHelper.ShowMenu();
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#showSecondaryMenu()
		 */
		public void ShowSecondaryMenu()
		{
			mHelper.ShowSecondaryMenu();
		}

		/* (non-Javadoc)
		 * @see com.jeremyfeinstein.slidingmenu.lib.app.SlidingActivityBase#setSlidingActionBarEnabled(boolean)
		 */
		public bool SlidingActionBarEnabled
		{
			set
			{
				mHelper.SlidingActionBarEnabled = value;
			}
		}

		/* (non-Javadoc)
		 * @see android.app.Activity#onKeyUp(int, android.view.KeyEvent)
		 */
        public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
		{
			bool b = mHelper.OnKeyUp(keyCode, e);
			if (b)
			{
				return b;
			}
			return base.OnKeyUp(keyCode, e);
		}
	}
}