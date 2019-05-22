using System;

using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Util;
using Android.Graphics.Drawables;

using Java.Lang;

using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace SlidingMenuLibrary
{
	public class CustomViewBehind : ViewGroup
	{
		private const string TAG = "CustomViewBehind";

		private const int MARGIN_THRESHOLD = 48; // dips
		private int mTouchMode = SlidingMenu.TOUCHMODE_MARGIN;

		private CustomViewAbove mViewAbove;

		private View mContent;
		private View mSecondaryContent;
		private int mMarginThreshold;
		private int mWidthOffset;
		private ICanvasTransformer mTransformer;
		private bool mChildrenEnabled;

		public CustomViewBehind(Context context) 
            : this(context, null)
		{
		}

		public CustomViewBehind(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			mMarginThreshold = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, MARGIN_THRESHOLD, Resources.DisplayMetrics);
		}

		public CustomViewAbove CustomViewAbove
		{
			set
			{
				mViewAbove = value;
			}
		}

		public ICanvasTransformer CanvasTransformer
		{
			set
			{
				mTransformer = value;
			}
		}

		public int WidthOffset
		{
			set
			{
				mWidthOffset = value;
				RequestLayout();
			}
		}

		public int MarginThreshold
		{
			set
			{
				mMarginThreshold = value;
			}
			get
			{
				return mMarginThreshold;
			}
		}

		public int BehindWidth
		{
			get
			{
				return mContent.Width;
			}
		}

		public View Content
		{
			set
			{
				if (mContent != null)
				{
					RemoveView(mContent);
				}
				mContent = value;
				AddView(mContent);
			}
			get
			{
				return mContent;
			}
		}

		/// <summary>
		/// Sets the secondary (right) menu for use when setMode is called with SlidingMenu.LEFT_RIGHT. </summary>
		/// <param name="v"> the right menu </param>
		public View SecondaryContent
		{
			set
			{
				if (mSecondaryContent != null)
				{
					RemoveView(mSecondaryContent);
				}
				mSecondaryContent = value;
				AddView(mSecondaryContent);
			}
			get
			{
				return mSecondaryContent;
			}
		}

		public bool ChildrenEnabled
		{
			set
			{
				mChildrenEnabled = value;
			}
		}

		public override void ScrollTo(int x, int y)
		{
			base.ScrollTo(x, y);
			if (mTransformer != null)
			{
				Invalidate();
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent e)
		{
			return !mChildrenEnabled;
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			return !mChildrenEnabled;
		}

		protected override void DispatchDraw(Canvas canvas)
		{
			if (mTransformer != null)
			{
				canvas.Save();
				mTransformer.TransformCanvas(canvas, mViewAbove.PercentOpen);
				base.DispatchDraw(canvas);
				canvas.Restore();
			}
			else
			{
				base.DispatchDraw(canvas);
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			int width = r - l;
			int height = b - t;
			mContent.Layout(0, 0, width - mWidthOffset, height);
			if (mSecondaryContent != null)
			{
				mSecondaryContent.Layout(0, 0, width - mWidthOffset, height);
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			int width = GetDefaultSize(0, widthMeasureSpec);
			int height = GetDefaultSize(0, heightMeasureSpec);
			SetMeasuredDimension(width, height);
			int contentWidth = GetChildMeasureSpec(widthMeasureSpec, 0, width - mWidthOffset);
			int contentHeight = GetChildMeasureSpec(heightMeasureSpec, 0, height);
			mContent.Measure(contentWidth, contentHeight);
			if (mSecondaryContent != null)
			{
				mSecondaryContent.Measure(contentWidth, contentHeight);
			}
		}

		private int mMode;
		private bool mFadeEnabled;
		private readonly Paint mFadePaint = new Paint();
		private float mScrollScale;
		private Drawable mShadowDrawable;
		private Drawable mSecondaryShadowDrawable;
		private int mShadowWidth;
		private float mFadeDegree;

		public int Mode
		{
			set
			{
				if (value == SlidingMenu.LEFT || value == SlidingMenu.RIGHT)
				{
					if (mContent != null)
					{
						mContent.Visibility = ViewStates.Visible;
					}
					if (mSecondaryContent != null)
					{
                        mSecondaryContent.Visibility = ViewStates.Invisible;
					}
				}
				mMode = value;
			}
			get
			{
				return mMode;
			}
		}

		public float ScrollScale
		{
			set
			{
				mScrollScale = value;
			}
			get
			{
				return mScrollScale;
			}
		}


		public Drawable ShadowDrawable
		{
			set
			{
				mShadowDrawable = value;
				Invalidate();
			}
		}

		public Drawable SecondaryShadowDrawable
		{
			set
			{
				mSecondaryShadowDrawable = value;
                Invalidate();
			}
		}

		public int ShadowWidth
		{
			set
			{
				mShadowWidth = value;
                Invalidate();
			}
		}

		public bool FadeEnabled
		{
			set
			{
				mFadeEnabled = value;
			}
		}

		public float FadeDegree
		{
			set
			{
				if (value > 1.0f || value < 0.0f)
				{
					throw new IllegalStateException("The BehindFadeDegree must be between 0.0f and 1.0f");
				}
				mFadeDegree = value;
			}
		}

		public int GetMenuPage(int page)
		{
			page = (page > 1) ? 2 : ((page < 1) ? 0 : page);
			if (mMode == SlidingMenu.LEFT && page > 1)
			{
				return 0;
			}
			else if (mMode == SlidingMenu.RIGHT && page < 1)
			{
				return 2;
			}
			else
			{
				return page;
			}
		}

		public void ScrollBehindTo(View content, int x, int y)
		{
            ViewStates vis = ViewStates.Visible;
			if (mMode == SlidingMenu.LEFT)
			{
				if (x >= content.Left)
				{
                    vis = ViewStates.Invisible;
				}
				ScrollTo((int)((x + BehindWidth) * mScrollScale), y);
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				if (x <= content.Left)
				{
                    vis = ViewStates.Invisible;
				}
				ScrollTo((int)(BehindWidth - Width + (x - BehindWidth) * mScrollScale), y);
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
                mContent.Visibility = x >= content.Left ? ViewStates.Invisible : ViewStates.Visible;
                mSecondaryContent.Visibility = x <= content.Left ? ViewStates.Invisible : ViewStates.Visible;
                vis = x == 0 ? ViewStates.Invisible : ViewStates.Visible;
				if (x <= content.Left)
				{
					ScrollTo((int)((x + BehindWidth) * mScrollScale), y);
				}
				else
				{
					ScrollTo((int)(BehindWidth - Width + (x - BehindWidth) * mScrollScale), y);
				}
			}
            if (vis == ViewStates.Invisible)
			{
				Console.WriteLine(TAG + " behind INVISIBLE");
			}
			Visibility = vis;
		}

		public int GetMenuLeft(View content, int page)
		{
			if (mMode == SlidingMenu.LEFT)
			{
				switch (page)
				{
			        case 0:
				        return content.Left - BehindWidth;
			        case 2:
				        return content.Left;
				}
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				switch (page)
				{
				    case 0:
					    return content.Left;
				    case 2:
					    return content.Left + BehindWidth;
				}
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				switch (page)
				{
				    case 0:
					    return content.Left - BehindWidth;
				    case 2:
					    return content.Left + BehindWidth;
				}
			}

			return content.Left;
		}

		public int GetAbsLeftBound(View content)
		{
			if (mMode == SlidingMenu.LEFT || mMode == SlidingMenu.LEFT_RIGHT)
			{
				return content.Left - BehindWidth;
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				return content.Left;
			}
			return 0;
		}

		public int GetAbsRightBound(View content)
		{
			if (mMode == SlidingMenu.LEFT)
			{
				return content.Left;
			}
			else if (mMode == SlidingMenu.RIGHT || mMode == SlidingMenu.LEFT_RIGHT)
			{
				return content.Left + BehindWidth;
			}
			return 0;
		}

		public bool MarginTouchAllowed(View content, int x)
		{
			int left = content.Left;
			int right = content.Right;
			if (mMode == SlidingMenu.LEFT)
			{
				return (x >= left && x <= mMarginThreshold + left);
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				return (x <= right && x >= right - mMarginThreshold);
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				return (x >= left && x <= mMarginThreshold + left) || (x <= right && x >= right - mMarginThreshold);
			}
			return false;
		}

		public int TouchMode
		{
			set
			{
				mTouchMode = value;
			}
		}

		public bool MenuOpenTouchAllowed(View content, int currPage, float x)
		{
			switch (mTouchMode)
			{
			    case SlidingMenu.TOUCHMODE_FULLSCREEN:
				    return true;
			    case SlidingMenu.TOUCHMODE_MARGIN:
				    return MenuTouchInQuickReturn(content, currPage, x);
			}
			return false;
		}

		public bool MenuTouchInQuickReturn(View content, int currPage, float x)
		{
			if (mMode == SlidingMenu.LEFT || (mMode == SlidingMenu.LEFT_RIGHT && currPage == 0))
			{
				return x >= content.Left;
			}
			else if (mMode == SlidingMenu.RIGHT || (mMode == SlidingMenu.LEFT_RIGHT && currPage == 2))
			{
				return x <= content.Right;
			}
			return false;
		}

		public bool MenuClosedSlideAllowed(float dx)
		{
			if (mMode == SlidingMenu.LEFT)
			{
				return dx > 0;
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				return dx < 0;
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				return true;
			}
			return false;
		}

		public bool MenuOpenSlideAllowed(float dx)
		{
			if (mMode == SlidingMenu.LEFT)
			{
				return dx < 0;
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				return dx > 0;
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				return true;
			}
			return false;
		}

		public void DrawShadow(View content, Canvas canvas)
		{
			if (mShadowDrawable == null || mShadowWidth <= 0)
			{
				return;
			}
			int left = 0;
			if (mMode == SlidingMenu.LEFT)
			{
				left = content.Left - mShadowWidth;
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				left = content.Right;
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				if (mSecondaryShadowDrawable != null)
				{
					left = content.Right;
					mSecondaryShadowDrawable.SetBounds(left, 0, left + mShadowWidth, Height);
					mSecondaryShadowDrawable.Draw(canvas);
				}
				left = content.Left - mShadowWidth;
			}
			mShadowDrawable.SetBounds(left, 0, left + mShadowWidth, Height);
			mShadowDrawable.Draw(canvas);
		}

		public void DrawFade(View content, Canvas canvas, float openPercent)
		{
			if (!mFadeEnabled)
			{
				return;
			}
			int alpha = (int)(mFadeDegree * 255 * System.Math.Abs(1 - openPercent));
			mFadePaint.Color = Color.Argb(alpha, 0, 0, 0);
			int left = 0;
			int right = 0;
			if (mMode == SlidingMenu.LEFT)
			{
				left = content.Left - BehindWidth;
				right = content.Left;
			}
			else if (mMode == SlidingMenu.RIGHT)
			{
				left = content.Right;
				right = content.Right + BehindWidth;
			}
			else if (mMode == SlidingMenu.LEFT_RIGHT)
			{
				left = content.Left - BehindWidth;
				right = content.Left;
				canvas.DrawRect(left, 0, right, Height, mFadePaint);
				left = content.Right;
				right = content.Right + BehindWidth;
			}
            canvas.DrawRect(left, 0, right, Height, mFadePaint);
		}

		private bool mSelectorEnabled = true;
		private Bitmap mSelectorDrawable;
		private View mSelectedView;

		public void DrawSelector(View content, Canvas canvas, float openPercent)
		{
			if (!mSelectorEnabled)
			{
				return;
			}
			if (mSelectorDrawable != null && mSelectedView != null)
			{
				string tag = (string) mSelectedView.GetTag(Resource.Id.selected_view);
				if (tag.Equals(TAG + "SelectedView"))
				{
					canvas.Save();
					int left, right, offset;
					offset = (int)(mSelectorDrawable.Width * openPercent);
					if (mMode == SlidingMenu.LEFT)
					{
						right = content.Left;
						left = right - offset;
						canvas.ClipRect(left, 0, right, Height);
						canvas.DrawBitmap(mSelectorDrawable, left, SelectorTop, null);
					}
					else if (mMode == SlidingMenu.RIGHT)
					{
						left = content.Right;
						right = left + offset;
                        canvas.ClipRect(left, 0, right, Height);
                        canvas.DrawBitmap(mSelectorDrawable, right - mSelectorDrawable.Width, SelectorTop, null);
					}
					canvas.Restore();
				}
			}
		}

		public bool SelectorEnabled
		{
			set
			{
				mSelectorEnabled = value;
			}
		}

		public View SelectedView
		{
			set
			{
				if (mSelectedView != null)
				{
					mSelectedView.SetTag(Resource.Id.selected_view, null);
					mSelectedView = null;
				}
				if (value != null && value.Parent != null)
				{
					mSelectedView = value;
                    mSelectedView.SetTag(Resource.Id.selected_view, TAG + "SelectedView");
					Invalidate();
				}
			}
		}

		private int SelectorTop
		{
			get
			{
				int y = mSelectedView.Top;
				y += (mSelectedView.Height - mSelectorDrawable.Height) / 2;
				return y;
			}
		}

		public Bitmap SelectorBitmap
		{
			set
			{
				mSelectorDrawable = value;
				RefreshDrawableState();
			}
		}
	}
}