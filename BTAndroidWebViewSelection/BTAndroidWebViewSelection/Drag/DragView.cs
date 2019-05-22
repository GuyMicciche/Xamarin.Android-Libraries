using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// A DragView is a special view used by a DragController. During a drag operation, what is actually moving
	/// on the screen is a DragView. A DragView is constructed using a bitmap of the view the user really
	/// wants to move.
	/// 
	/// </summary>
	public class DragView : View
	{
		// Number of pixels to add to the dragged item for scaling.  Should be even for pixel alignment.
		private const int DRAG_SCALE = 0; // In Launcher, value is 40

		private Bitmap mBitmap;
		private Paint mPaint;
		private int mRegistrationX;
		private int mRegistrationY;

		private float mScale;
		private float mAnimationScale = 1.0f;

		private WindowManagerLayoutParams mLayoutParams;
		private IWindowManager mWindowManager;

		/// <summary>
		/// Construct the drag view.
		/// <p>
		/// The registration point is the point inside our view that the touch events should
		/// be centered upon.
		/// </summary>
		/// <param name="context"> A context </param>
		/// <param name="bitmap"> The view that we're dragging around.  We scale it up when we draw it. </param>
		/// <param name="registrationX"> The x coordinate of the registration point. </param>
		/// <param name="registrationY"> The y coordinate of the registration point. </param>
		public DragView(Context context, Bitmap bitmap, int registrationX, int registrationY, int left, int top, int width, int height) : base(context)
		{

			// mWindowManager = WindowManagerImpl.getDefault();
            var windowService = context.GetSystemService(Context.WindowService);
            mWindowManager = windowService.JavaCast<IWindowManager>();

			Matrix scale = new Matrix();
			float scaleFactor = width;
			scaleFactor = mScale = (scaleFactor + DRAG_SCALE) / scaleFactor;
			scale.SetScale(scaleFactor, scaleFactor);
			mBitmap = Bitmap.CreateBitmap(bitmap, left, top, width, height, scale, true);

			// The point in our scaled bitmap that the touch events are located
			mRegistrationX = registrationX + (DRAG_SCALE / 2);
			mRegistrationY = registrationY + (DRAG_SCALE / 2);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			SetMeasuredDimension(mBitmap.Width, mBitmap.Height);
		}

		protected override void OnDraw(Canvas canvas)
		{
			/*if (true) {
			    // for debugging
			    Paint p = new Paint();
			    p.setStyle(Paint.Style.FILL);
			    p.setColor(0x88dd0011);
			    canvas.drawRect(0, 0, getWidth(), getHeight(), p);
			}*/
			float scale = mAnimationScale;
			if (scale < 0.999f) // allow for some float error
			{
				float width = mBitmap.Width;
				float offset = (width - (width * scale)) / 2;
				canvas.Translate(offset, offset);
				canvas.Scale(scale, scale);
			}
			canvas.DrawBitmap(mBitmap, 0.0f, 0.0f, mPaint);
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			mBitmap.Recycle();
		}

		public Paint Paint
		{
			set
			{
				mPaint = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Create a window containing this view and show it.
		/// </summary>
		/// <param name="windowToken"> obtained from v.getWindowToken() from one of your views </param>
		/// <param name="touchX"> the x coordinate the user touched in screen coordinates </param>
		/// <param name="touchY"> the y coordinate the user touched in screen coordinates </param>
		public void Show(IBinder windowToken, int touchX, int touchY)
		{
            WindowManagerLayoutParams lp;
            Format pixelFormat;

			pixelFormat = Format.Translucent;

            lp = new WindowManagerLayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent, touchX - mRegistrationX, touchY - mRegistrationY, WindowManagerTypes.ApplicationSubPanel, WindowManagerFlags.LayoutInScreen | WindowManagerFlags.LayoutNoLimits, pixelFormat);
						/*| WindowManager.LayoutParams.FLAG_ALT_FOCUSABLE_IM*/
	//        lp.token = mStatusBarView.getWindowToken();
			lp.Gravity = GravityFlags.Left | GravityFlags.Top;
            lp.Token = windowToken;
			lp.Title = "DragView";
			mLayoutParams = lp;

			mWindowManager.AddView(this, lp);
		}

		/// <summary>
		/// Move the window containing this view.
		/// </summary>
		/// <param name="touchX"> the x coordinate the user touched in screen coordinates </param>
		/// <param name="touchY"> the y coordinate the user touched in screen coordinates </param>
		public void Move(int touchX, int touchY)
		{
			// This is what was done in the Launcher code.
            WindowManagerLayoutParams lp = mLayoutParams;
			lp.X = touchX - mRegistrationX;
			lp.Y = touchY - mRegistrationY;
			mWindowManager.UpdateViewLayout(this, lp);
		}

        public void Remove()
		{
			mWindowManager.RemoveView(this);
		}
	}
}