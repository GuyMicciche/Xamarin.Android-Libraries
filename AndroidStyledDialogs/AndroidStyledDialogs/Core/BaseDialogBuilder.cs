using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Java.Lang;
using System;

namespace AndroidStyledDialogs
{
	public abstract class BaseDialogBuilder<T> where T : BaseDialogBuilder<T>
	{
		public const string ARG_REQUEST_CODE = "request_code";
		public const string ARG_CANCELABLE_ON_TOUCH_OUTSIDE = "cancelable_oto";
		public const string DEFAULT_TAG = "simple_dialog";
		private string mTag = DEFAULT_TAG;
		public const int DEFAULT_REQUEST_CODE = -42;
		private int mRequestCode = DEFAULT_REQUEST_CODE;
		public static string ARG_USE_DARK_THEME = "usedarktheme";
		protected Context mContext;
		protected FragmentManager mFragmentManager;
		protected Class mClass;
		private Fragment mTargetFragment;
		private bool mCancelable = true;
		private bool mCancelableOnTouchOutside = true;
		private bool mUseDarkTheme = false;

        private BaseDialogFragment fragment;

        public BaseDialogBuilder(Context context, FragmentManager fragmentManager, Class clazz)
		{
			mFragmentManager = fragmentManager;
			mContext = context.ApplicationContext;
			mClass = clazz;
		}

		protected abstract T Self();

		protected abstract Bundle PrepareArguments();

		public virtual T SetCancelable(bool cancelable)
		{
			mCancelable = cancelable;
			return Self();
		}

		public virtual T SetCancelableOnTouchOutside(bool cancelable)
		{
			mCancelableOnTouchOutside = cancelable;
			if (cancelable)
			{
				mCancelable = cancelable;
			}
			return Self();
		}

		public virtual T SetTargetFragment(Fragment fragment, int requestCode)
		{
			mTargetFragment = fragment;
			mRequestCode = requestCode;
			return Self();
		}

		public virtual T SetRequestCode(int requestCode)
		{
			mRequestCode = requestCode;
			return Self();
		}

		public virtual T SetTag(string tag)
		{
			mTag = tag;
			return Self();
		}

		public virtual T UseDarkTheme()
		{
			mUseDarkTheme = true;
			return Self();
		}

		public BaseDialogFragment Create()
		{
			Bundle args = PrepareArguments();

			BaseDialogFragment fragment = (BaseDialogFragment)Fragment.Instantiate(mContext, mClass.Name, args);

			args.PutBoolean(ARG_CANCELABLE_ON_TOUCH_OUTSIDE, mCancelableOnTouchOutside);

            args.PutBoolean(ARG_USE_DARK_THEME, mUseDarkTheme);

			if (mTargetFragment != null)
			{
				fragment.SetTargetFragment(mTargetFragment, mRequestCode);
			}
			else
			{
				args.PutInt(ARG_REQUEST_CODE, mRequestCode);
			}
			fragment.Cancelable = mCancelable;

			return fragment;
		}

		public virtual DialogFragment Show()
		{
			fragment = Create();
			fragment.Show(mFragmentManager, mTag);

			return fragment;
		}

        public virtual DialogFragment Dismiss()
        {
            fragment.Dismiss();

            return fragment;
        }

		/// <summary>
		/// Like show() but allows the commit to be executed after an activity's state is saved. This
		/// is dangerous because the commit can be lost if the activity needs to later be restored from
		/// its state, so this should only be used for cases where it is okay for the UI state to change
		/// unexpectedly on the user.
		/// </summary>
		public virtual DialogFragment ShowAllowingStateLoss()
		{
			fragment = Create();
			fragment.ShowAllowingStateLoss(mFragmentManager, mTag);

			return fragment;
		}
	}
}