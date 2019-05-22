using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Custom popup window.
	/// </summary>
	public class PopupWindows
	{
		protected Context mContext;
		protected PopupWindow mWindow;
		protected View mRootView;
		protected Drawable mBackground = null;
		protected IWindowManager mWindowManager;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="context"> Context </param>
		public PopupWindows(Context context)
		{
			mContext = context;
			mWindow = new PopupWindow(context);

            mWindow.TouchIntercepted += mWindow_TouchIntercepted;

            var windowService = context.GetSystemService(Context.WindowService);
            mWindowManager = windowService.JavaCast<IWindowManager>();
		}

        void mWindow_TouchIntercepted(object sender, View.TouchEventArgs e)
        {
            if (e.Event.Action == MotionEventActions.Outside)
            {
                mWindow.Dismiss();
            }
        }

		/// <summary>
		/// On dismiss
		/// </summary>
		protected void OnDismiss()
		{
		}

		/// <summary>
		/// On show
		/// </summary>
		protected void OnShow()
		{
		}

		/// <summary>
		/// On pre show
		/// </summary>
		protected void PreShow()
		{
			if (mRootView == null)
			{
				throw new IllegalStateException("setContentView was not called with a view to display.");
			}

			OnShow();

			if (mBackground == null)
			{
				mWindow.SetBackgroundDrawable(new BitmapDrawable());
			}
			else
			{
                mWindow.SetBackgroundDrawable(mBackground);
			}

			mWindow.Width = WindowManagerLayoutParams.WrapContent;
            mWindow.Height = WindowManagerLayoutParams.WrapContent;
			mWindow.Touchable = true;

			//TODO: make sure this doesn't crash somehow.
			//mWindow.setFocusable(true);
			mWindow.OutsideTouchable = true;

			mWindow.ContentView = mRootView;
		}

		/// <summary>
		/// Set background drawable.
		/// </summary>
		/// <param name="background"> Background drawable </param>
		public Drawable BackgroundDrawable
		{
			set
			{
				mBackground = value;
			}
		}

		/// <summary>
		/// Set content view.
		/// </summary>
		/// <param name="root"> Root view </param>
		public View ContentView
		{
			set
			{
				mRootView = value;
    
				mWindow.ContentView = value;
			}
		}

		/// <summary>
		/// Set content view.
		/// </summary>
		/// <param name="layoutResID"> Resource id </param>
		public void SetContentView(int resId)
		{
			LayoutInflater inflator = (LayoutInflater)mContext.GetSystemService(Context.LayoutInflaterService);
    
			ContentView = inflator.Inflate(resId, null);
		}

		/// <summary>
		/// Set listener on window dismissed.
		/// </summary>
		/// <param name="listener"> </param>
		public PopupWindow.IOnDismissListener OnDismissListener
		{
			set
			{
				mWindow.SetOnDismissListener(value);
			}
		}

		/// <summary>
		/// Dismiss the popup window.
		/// </summary>
		public void Dismiss()
		{
            mWindow.Dismiss();
		}
	}
}