using System;
using System.Collections.Generic;

namespace SlidingMenuLibrary
{
    using Android.Content;
    using Android.Graphics;
    using Android.OS;
    using Android.Support.V4.View;
    using Android.Util;
    using Android.Views;
    using Android.Views.Animations;
    using Android.Widget;

    using IOnClosedListener = SlidingMenu.IOnClosedListener;
    using IOnOpenedListener = SlidingMenu.IOnOpenedListener;
    //using IOnCloseListener = SlidingMenu.IOnCloseListener;
    //using IOnOpenListener = SlidingMenu.IOnOpenListener;

	public class CustomViewAbove : ViewGroup
	{
		private const string TAG = "CustomViewAbove";
		private const bool DEBUG = false;

		private const bool USE_CACHE = false;

		private const int MAX_SETTLE_DURATION = 600; // ms
		private const int MIN_DISTANCE_FOR_FLING = 25; // dips

		private readonly IInterpolator sInterpolator = new CustomViewAboveInterpolator();

		private class CustomViewAboveInterpolator : Java.Lang.Object, IInterpolator
		{
			public float GetInterpolation(float t)
			{
				t -= 1.0f;
				return t * t * t * t * t + 1.0f;
			}
		}

		private View mContent;

		private int mCurItem;
		private Scroller mScroller;

		private bool mScrollingCacheEnabled;

		private bool mScrolling;

		private bool mIsBeingDragged;
		private bool mIsUnableToDrag;
		private int mTouchSlop;
		private float mInitialMotionX;
		/// <summary>
		/// Position of the last motion event.
		/// </summary>
		private float mLastMotionX;
		private float mLastMotionY;
		/// <summary>
		/// ID of the active pointer. This is used to retain consistency during
		/// drags/flings if multiple pointers are used.
		/// </summary>
		protected int mActivePointerId = INVALID_POINTER;
		/// <summary>
		/// Sentinel value for no current active pointer.
		/// Used by <seealso cref="#mActivePointerId"/>.
		/// </summary>
		private const int INVALID_POINTER = -1;

		/// <summary>
		/// Determines speed during touch scrolling
		/// </summary>
		protected VelocityTracker mVelocityTracker;
		private int mMinimumVelocity;
		protected internal int mMaximumVelocity;
		private int mFlingDistance;

		private CustomViewBehind mViewBehind;
		//	private int mMode;
		private bool mEnabled = true;

		private IOnPageChangeListener mOnPageChangeListener;
		private IOnPageChangeListener mInternalPageChangeListener;

		//	private IOnCloseListener mCloseListener;
		//	private IOnOpenListener mOpenListener;
		private IOnClosedListener mClosedListener;
		private IOnOpenedListener mOpenedListener;

		private IList<View> mIgnoredViews = new List<View>();

        //  private int mScrollState = ScrollState.Idle;

		/// <summary>
		/// Callback interface for responding to changing state of the selected page.
		/// </summary>
		public interface IOnPageChangeListener
		{

			/// <summary>
			/// This method will be invoked when the current page is scrolled, either as part
			/// of a programmatically initiated smooth scroll or a user initiated touch scroll.
			/// </summary>
			/// <param name="position"> Position index of the first page currently being displayed.
			///                 Page position+1 will be visible if positionOffset is nonzero. </param>
			/// <param name="positionOffset"> Value from [0, 1) indicating the offset from the page at position. </param>
			/// <param name="positionOffsetPixels"> Value in pixels indicating the offset from position. </param>
			void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels);

			/// <summary>
			/// This method will be invoked when a new page becomes selected. Animation is not
			/// necessarily complete.
			/// </summary>
			/// <param name="position"> Position index of the new selected page. </param>
			void OnPageSelected(int position);

		}

		/// <summary>
		/// Simple implementation of the <seealso cref="IOnPageChangeListener"/> interface with stub
		/// implementations of each method. Extend this if you do not intend to override
		/// every method of <seealso cref="IOnPageChangeListener"/>.
		/// </summary>
		public class SimpleOnPageChangeListener : IOnPageChangeListener
		{
			public virtual void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
				// This space for rent
			}

			public virtual void OnPageSelected(int position)
			{
				// This space for rent
			}

			public virtual void OnPageScrollStateChanged(int state)
			{
				// This space for rent
			}

		}

		public CustomViewAbove(Context context) 
            : this(context, null)
		{
		}

		public CustomViewAbove(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			InitCustomViewAbove();
		}

		void InitCustomViewAbove()
		{
			SetWillNotDraw(false);
            DescendantFocusability = DescendantFocusability.AfterDescendants;
			Focusable = true;
			Context context = Context;
			mScroller = new Scroller(context, sInterpolator);
			ViewConfiguration configuration = ViewConfiguration.Get(context);
			mTouchSlop = ViewConfigurationCompat.GetScaledPagingTouchSlop(configuration);
			mMinimumVelocity = configuration.ScaledMinimumFlingVelocity;
			mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;
			SetInternalPageChangeListener(new MySimpleOnPageChangeListener(this));

			float density = context.Resources.DisplayMetrics.Density;
			mFlingDistance = (int)(MIN_DISTANCE_FOR_FLING * density);
		}

		private class MySimpleOnPageChangeListener : SimpleOnPageChangeListener
		{
			private readonly CustomViewAbove view;

            public MySimpleOnPageChangeListener(CustomViewAbove view)
			{
                this.view = view;
			}

			public override void OnPageSelected(int position)
			{
				if (view.mViewBehind != null)
				{
					switch (position)
					{
					    case 0:
					    case 2:
						    view.mViewBehind.ChildrenEnabled = true;
						    break;
					    case 1:
						    view.mViewBehind.ChildrenEnabled = false;
						    break;
					}
				}
			}
		}

		/// <summary>
		/// Set the currently selected page. If the CustomViewPager has already been through its first
		/// layout there will be a smooth animated transition between the current item and the
		/// specified item.
		/// </summary>
		/// <param name="item"> Item index to select </param>
		public int CurrentItem
		{
			set
			{
				SetCurrentItemInternal(value, true, false);
			}
			get
			{
				return mCurItem;
			}
		}

		/// <summary>
		/// Set the currently selected page.
		/// </summary>
		/// <param name="item"> Item index to select </param>
		/// <param name="smoothScroll"> True to smoothly scroll to the new item, false to transition immediately </param>
		public void SetCurrentItem(int item, bool smoothScroll)
		{
			SetCurrentItemInternal(item, smoothScroll, false);
		}


		void SetCurrentItemInternal(int item, bool smoothScroll, bool always)
		{
			SetCurrentItemInternal(item, smoothScroll, always, 0);
		}

		void SetCurrentItemInternal(int item, bool smoothScroll, bool always, int velocity)
		{
			if (!always && mCurItem == item)
			{
				ScrollingCacheEnabled = false;
				return;
			}

			item = mViewBehind.GetMenuPage(item);

			bool dispatchSelected = mCurItem != item;
			mCurItem = item;
			int destX = GetDestScrollX(mCurItem);
			if (dispatchSelected && mOnPageChangeListener != null)
			{
				mOnPageChangeListener.OnPageSelected(item);
			}
			if (dispatchSelected && mInternalPageChangeListener != null)
			{
				mInternalPageChangeListener.OnPageSelected(item);
			}
			if (smoothScroll)
			{
				SmoothScrollTo(destX, 0, velocity);
			}
			else
			{
				CompleteScroll();
				ScrollTo(destX, 0);
			}
		}

		/// <summary>
		/// Set a listener that will be invoked whenever the page changes or is incrementally
		/// scrolled. See <seealso cref="IOnPageChangeListener"/>.
		/// </summary>
		/// <param name="listener"> Listener to set </param>
		public IOnPageChangeListener OnPageChangeListener
		{
			set
			{
				mOnPageChangeListener = value;
			}
		}
		/*
		public void setOnOpenListener(OnOpenListener l) {
			mOpenListener = l;
		}
	
		public void setOnCloseListener(OnCloseListener l) {
			mCloseListener = l;
		}
		 */
		public IOnOpenedListener OnOpenedListener
		{
			set
			{
				mOpenedListener = value;
			}
		}

		public IOnClosedListener OnClosedListener
		{
			set
			{
				mClosedListener = value;
			}
		}

		/// <summary>
		/// Set a separate OnPageChangeListener for internal use by the support library.
		/// </summary>
		/// <param name="listener"> Listener to set </param>
		/// <returns> The old listener that was set, if any. </returns>
		internal IOnPageChangeListener SetInternalPageChangeListener(IOnPageChangeListener listener)
		{
			IOnPageChangeListener oldListener = mInternalPageChangeListener;
			mInternalPageChangeListener = listener;
			return oldListener;
		}

		public void AddIgnoredView(View v)
		{
			if (!mIgnoredViews.Contains(v))
			{
				mIgnoredViews.Add(v);
			}
		}

		public void RemoveIgnoredView(View v)
		{
			mIgnoredViews.Remove(v);
		}

		public void ClearIgnoredViews()
		{
			mIgnoredViews.Clear();
		}

		// We want the duration of the page snap animation to be influenced by the distance that
		// the screen has to travel, however, we don't want this duration to be effected in a
		// purely linear fashion. Instead, we use this method to moderate the effect that the distance
		// of travel has on the overall snap duration.
		static float DistanceInfluenceForSnapDuration(float f)
		{
			f -= 0.5f; // center the values about 0.
			f *= 0.3f * (float)Math.PI / 2.0f;
			return (float)FloatMath.Sin(f);
		}

		public int GetDestScrollX(int page)
		{
			switch (page)
			{
			    case 0:
			    case 2:
				    return mViewBehind.GetMenuLeft(mContent, page);
			    case 1:
				    return mContent.Left;
			}
			return 0;
		}

		private int LeftBound
		{
			get
			{
				return mViewBehind.GetAbsLeftBound(mContent);
			}
		}

		private int RightBound
		{
			get
			{
				return mViewBehind.GetAbsRightBound(mContent);
			}
		}

		public int ContentLeft
		{
			get
			{
				return mContent.Left + mContent.PaddingLeft;
			}
		}

		public bool IsMenuOpen
		{
			get
			{
				return mCurItem == 0 || mCurItem == 2;
			}
		}

		private bool IsInIgnoredView(MotionEvent ev)
		{
			Rect rect = new Rect();
			foreach (View v in mIgnoredViews)
			{
				v.GetHitRect(rect);
				if (rect.Contains((int)ev.GetX(), (int)ev.GetY()))
				{
					return true;
				}
			}
			return false;
		}

		public int BehindWidth
		{
			get
			{
				if (mViewBehind == null)
				{
					return 0;
				}
				else
				{
					return mViewBehind.BehindWidth;
				}
			}
		}

		public int GetChildWidth(int i)
		{
			switch (i)
			{
			    case 0:
				    return BehindWidth;
			    case 1:
				    return mContent.Width;
			    default:
				    return 0;
			}
		}

		public bool IsSlidingEnabled
		{
			get
			{
				return mEnabled;
			}
			set
			{
				mEnabled = value;
			}
		}


		/// <summary>
		/// Like <seealso cref="View#scrollBy"/>, but scroll smoothly instead of immediately.
		/// </summary>
		/// <param name="x"> the number of pixels to scroll by on the X axis </param>
		/// <param name="y"> the number of pixels to scroll by on the Y axis </param>
		void SmoothScrollTo(int x, int y)
		{
			SmoothScrollTo(x, y, 0);
		}

		/// <summary>
		/// Like <seealso cref="View#scrollBy"/>, but scroll smoothly instead of immediately.
		/// </summary>
		/// <param name="x"> the number of pixels to scroll by on the X axis </param>
		/// <param name="y"> the number of pixels to scroll by on the Y axis </param>
		/// <param name="velocity"> the velocity associated with a fling, if applicable. (0 otherwise) </param>
		void SmoothScrollTo(int x, int y, int velocity)
		{
			if (ChildCount == 0)
			{
				// Nothing to do.
				ScrollingCacheEnabled = false;
				return;
			}
			int sx = ScrollX;
			int sy = ScrollY;
			int dx = x - sx;
			int dy = y - sy;
			if (dx == 0 && dy == 0)
			{
				CompleteScroll();
				if (IsMenuOpen)
				{
					if (mOpenedListener != null)
					{
						mOpenedListener.OnOpened();
					}
				}
				else
				{
					if (mClosedListener != null)
					{
						mClosedListener.OnClosed();
					}
				}
				return;
			}

			ScrollingCacheEnabled = true;
			mScrolling = true;

			int width = BehindWidth;
			int halfWidth = width / 2;
			float distanceRatio = Math.Min(1f, 1.0f * Math.Abs(dx) / width);
			float distance = halfWidth + halfWidth * DistanceInfluenceForSnapDuration(distanceRatio);

			int duration = 0;
			velocity = Math.Abs(velocity);
			if (velocity > 0)
			{
				duration = 4 * (int)Math.Round(1000 * Math.Abs(distance / velocity));
			}
			else
			{
				float pageDelta = (float) Math.Abs(dx) / width;
				duration = (int)((pageDelta + 1) * 100);
				duration = MAX_SETTLE_DURATION;
			}
			duration = Math.Min(duration, MAX_SETTLE_DURATION);

			mScroller.StartScroll(sx, sy, dx, dy, duration);
			Invalidate();
		}

		public View Content
		{
			set
			{
				if (mContent != null)
				{
					this.RemoveView(mContent);
				}
				mContent = value;
				AddView(mContent);
			}
			get
			{
				return mContent;
			}
		}


		public CustomViewBehind CustomViewBehind
		{
			set
			{
				mViewBehind = value;
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{

			int width = GetDefaultSize(0, widthMeasureSpec);
			int height = GetDefaultSize(0, heightMeasureSpec);
			SetMeasuredDimension(width, height);

			int contentWidth = GetChildMeasureSpec(widthMeasureSpec, 0, width);
			int contentHeight = GetChildMeasureSpec(heightMeasureSpec, 0, height);
			mContent.Measure(contentWidth, contentHeight);
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.OnSizeChanged(w, h, oldw, oldh);
			// Make sure scroll position is set correctly.
			if (w != oldw)
			{
				// [ChrisJ] - This fixes the onConfiguration change for orientation issue..
				// maybe worth having a look why the recomputeScroll pos is screwing
				// up?
				CompleteScroll();
				ScrollTo(GetDestScrollX(mCurItem), ScrollY);
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			int width = r - l;
			int height = b - t;
			mContent.Layout(0, 0, width, height);
		}

		public int AboveOffset
		{
			set
			{
				//		RelativeLayout.LayoutParams params = ((RelativeLayout.LayoutParams)mContent.getLayoutParams());
				//		params.setMargins(value, params.topMargin, params.rightMargin, params.bottomMargin);
				mContent.SetPadding(value, mContent.PaddingTop, mContent.PaddingRight, mContent.PaddingBottom);
			}
		}

		public override void ComputeScroll()
		{
			if (!mScroller.IsFinished)
			{
				if (mScroller.ComputeScrollOffset())
				{
					int oldX = ScrollX;
					int oldY = ScrollY;
					int x = mScroller.CurrX;
					int y = mScroller.CurrY;

					if (oldX != x || oldY != y)
					{
						ScrollTo(x, y);
						PageScrolled(x);
					}

					// Keep on drawing until the animation has finished.
					Invalidate();
					return;
				}
			}

			// Done with scroll, clean up state.
			CompleteScroll();
		}

        protected void PageScrolled(int xpos)
		{
			int widthWithMargin = Width;
			int position = xpos / widthWithMargin;
			int offsetPixels = xpos % widthWithMargin;
			float offset = (float) offsetPixels / widthWithMargin;

			OnPageScrolled(position, offset, offsetPixels);
		}

		/// <summary>
		/// This method will be invoked when the current page is scrolled, either as part
		/// of a programmatically initiated smooth scroll or a user initiated touch scroll.
		/// If you override this method you must call through to the superclass implementation
		/// (e.g. super.onPageScrolled(position, offset, offsetPixels)) before onPageScrolled
		/// returns.
		/// </summary>
		/// <param name="position"> Position index of the first page currently being displayed.
		///                 Page position+1 will be visible if positionOffset is nonzero. </param>
		/// <param name="offset"> Value from [0, 1) indicating the offset from the page at position. </param>
		/// <param name="offsetPixels"> Value in pixels indicating the offset from position. </param>
		protected void OnPageScrolled(int position, float offset, int offsetPixels)
		{
			if (mOnPageChangeListener != null)
			{
				mOnPageChangeListener.OnPageScrolled(position, offset, offsetPixels);
			}
			if (mInternalPageChangeListener != null)
			{
				mInternalPageChangeListener.OnPageScrolled(position, offset, offsetPixels);
			}
		}

		private void CompleteScroll()
		{
			bool needPopulate = mScrolling;
			if (needPopulate)
			{
				// Done with scroll, no longer want to cache view drawing.
				ScrollingCacheEnabled = false;
				mScroller.AbortAnimation();
				int oldX = ScrollX;
				int oldY = ScrollY;
				int x = mScroller.CurrX;
				int y = mScroller.CurrY;
				if (oldX != x || oldY != y)
				{
					ScrollTo(x, y);
				}
				if (IsMenuOpen)
				{
					if (mOpenedListener != null)
					{
						mOpenedListener.OnOpened();
					}
				}
				else
				{
					if (mClosedListener != null)
					{
						mClosedListener.OnClosed();
					}
				}
			}
			mScrolling = false;
		}

		protected int mTouchMode = SlidingMenu.TOUCHMODE_MARGIN;

		public int TouchMode
		{
			set
			{
				mTouchMode = value;
			}
			get
			{
				return mTouchMode;
			}
		}

		private bool ThisTouchAllowed(MotionEvent ev)
		{
			int x = (int)(ev.GetX() + mScrollX);
			if (IsMenuOpen)
			{
				return mViewBehind.MenuOpenTouchAllowed(mContent, mCurItem, x);
			}
			else
			{
				switch (mTouchMode)
				{
				    case SlidingMenu.TOUCHMODE_FULLSCREEN:
					    return !IsInIgnoredView(ev);
				    case SlidingMenu.TOUCHMODE_NONE:
					    return false;
				    case SlidingMenu.TOUCHMODE_MARGIN:
					    return mViewBehind.MarginTouchAllowed(mContent, x);
				}
			}
			return false;
		}

		private bool ThisSlideAllowed(float dx)
		{
			bool allowed = false;
			if (IsMenuOpen)
			{
				allowed = mViewBehind.MenuOpenSlideAllowed(dx);
			}
			else
			{
				allowed = mViewBehind.MenuClosedSlideAllowed(dx);
			}
			if (DEBUG)
			{
				Console.WriteLine(TAG + " this slide allowed " + allowed + " dx: " + dx);
			}
			return allowed;
		}

		private int GetPointerIndex(MotionEvent ev, int id)
		{
			int activePointerIndex = MotionEventCompat.FindPointerIndex(ev, id);
			if (activePointerIndex == -1)
			{
				mActivePointerId = INVALID_POINTER;
			}
			return activePointerIndex;
		}

		private bool mQuickReturn = false;

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{

			if (!mEnabled)
			{
				return false;
			}

            MotionEventActions action = ev.Action & (MotionEventActions)MotionEventCompat.ActionMask;

			if (DEBUG)
			{
				if (action == MotionEventActions.Down)
				{
					Console.WriteLine(TAG + " Received ACTION_DOWN");
				}
			}

            if (action == MotionEventActions.Cancel || action == MotionEventActions.Up || (action != MotionEventActions.Down && mIsUnableToDrag))
			{
				EndDrag();
				return false;
			}

			switch (action)
			{
                case MotionEventActions.Move:
				    DetermineDrag(ev);
				    break;
                case MotionEventActions.Down:
				    int index = MotionEventCompat.GetActionIndex(ev);
				    mActivePointerId = MotionEventCompat.GetPointerId(ev, index);
				    if (mActivePointerId == INVALID_POINTER)
				    {
					    break;
				    }
				    mLastMotionX = mInitialMotionX = MotionEventCompat.GetX(ev, index);
				    mLastMotionY = MotionEventCompat.GetY(ev, index);
				    if (ThisTouchAllowed(ev))
				    {
					    mIsBeingDragged = false;
					    mIsUnableToDrag = false;
					    if (IsMenuOpen && mViewBehind.MenuTouchInQuickReturn(mContent, mCurItem, ev.GetX() + mScrollX))
					    {
						    mQuickReturn = true;
					    }
				    }
				    else
				    {
					    mIsUnableToDrag = true;
				    }
				    break;
                case (MotionEventActions)MotionEventCompat.ActionPointerUp:
				    OnSecondaryPointerUp(ev);
				    break;
			}

			if (!mIsBeingDragged)
			{
				if (mVelocityTracker == null)
				{
					mVelocityTracker = VelocityTracker.Obtain();
				}
				mVelocityTracker.AddMovement(ev);
			}
			return mIsBeingDragged || mQuickReturn;
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{

			if (!mEnabled)
			{
				return false;
			}

			if (!mIsBeingDragged && !ThisTouchAllowed(ev))
			{
				return false;
			}

			//		if (!mIsBeingDragged && !mQuickReturn)
			//			return false;

            MotionEventActions action = ev.Action;

			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
			mVelocityTracker.AddMovement(ev);

			switch (action & (MotionEventActions)MotionEventCompat.ActionMask)
			{
			    case MotionEventActions.Down:
				    /*
				        * If being flinged and user touches, stop the fling. isFinished
				        * will be false if being flinged.
				        */
				    CompleteScroll();

				    // Remember where the motion event started
				    int index = MotionEventCompat.GetActionIndex(ev);
				    mActivePointerId = MotionEventCompat.GetPointerId(ev, index);
				    mLastMotionX = mInitialMotionX = ev.GetX();
				    break;
			    case MotionEventActions.Move:
				    if (!mIsBeingDragged)
				    {
					    DetermineDrag(ev);
					    if (mIsUnableToDrag)
					    {
						    return false;
					    }
				    }
				    if (mIsBeingDragged)
				    {
					    // Scroll to follow the motion event
					    int activePointerIndex = GetPointerIndex(ev, mActivePointerId);
					    if (mActivePointerId == INVALID_POINTER)
					    {
						    break;
					    }
					    float x = MotionEventCompat.GetX(ev, activePointerIndex);
					    float deltaX = mLastMotionX - x;
					    mLastMotionX = x;
					    float oldScrollX = ScrollX;
					    float scrollX = oldScrollX + deltaX;
					    float leftBound = LeftBound;
					    float rightBound = RightBound;
					    if (scrollX < leftBound)
					    {
						    scrollX = leftBound;
					    }
					    else if (scrollX > rightBound)
					    {
						    scrollX = rightBound;
					    }
					    // Don't lose the rounded component
					    mLastMotionX += scrollX - (int) scrollX;
					    ScrollTo((int) scrollX, ScrollY);
					    PageScrolled((int) scrollX);
				    }
				    break;
			    case MotionEventActions.Up:
				    if (mIsBeingDragged)
				    {
					    VelocityTracker velocityTracker = mVelocityTracker;
					    velocityTracker.ComputeCurrentVelocity(1000, mMaximumVelocity);
					    int initialVelocity = (int) VelocityTrackerCompat.GetXVelocity(velocityTracker, mActivePointerId);
					    int scrollX = ScrollX;
					    float pageOffset = (float)(scrollX - GetDestScrollX(mCurItem)) / BehindWidth;
					    int activePointerIndex = GetPointerIndex(ev, mActivePointerId);
					    if (mActivePointerId != INVALID_POINTER)
					    {
						    float x = MotionEventCompat.GetX(ev, activePointerIndex);
						    int totalDelta = (int)(x - mInitialMotionX);
						    int nextPage = DetermineTargetPage(pageOffset, initialVelocity, totalDelta);
						    SetCurrentItemInternal(nextPage, true, true, initialVelocity);
					    }
					    else
					    {
						    SetCurrentItemInternal(mCurItem, true, true, initialVelocity);
					    }
					    mActivePointerId = INVALID_POINTER;
					    EndDrag();
				    }
				    else if (mQuickReturn && mViewBehind.MenuTouchInQuickReturn(mContent, mCurItem, ev.GetX() + mScrollX))
				    {
					    // close the menu
					    CurrentItem = 1;
					    EndDrag();
				    }
				    break;
			    case MotionEventActions.Cancel:
				    if (mIsBeingDragged)
				    {
					    SetCurrentItemInternal(mCurItem, true, true);
					    mActivePointerId = INVALID_POINTER;
					    EndDrag();
				    }
				    break;
			    case (MotionEventActions)MotionEventCompat.ActionPointerDown:
			    {
				    int indexx = MotionEventCompat.GetActionIndex(ev);
				    mLastMotionX = MotionEventCompat.GetX(ev, indexx);
				    mActivePointerId = MotionEventCompat.GetPointerId(ev, indexx);
				    break;
			    }
			    case (MotionEventActions)MotionEventCompat.ActionPointerUp:
				    OnSecondaryPointerUp(ev);
				    int pointerIndex = GetPointerIndex(ev, mActivePointerId);
				    if (mActivePointerId == INVALID_POINTER)
				    {
					    break;
				    }
				    mLastMotionX = MotionEventCompat.GetX(ev, pointerIndex);
				    break;
			}
			return true;
		}

		private void DetermineDrag(MotionEvent ev)
		{
			int activePointerId = mActivePointerId;
			int pointerIndex = GetPointerIndex(ev, activePointerId);
			if (activePointerId == INVALID_POINTER || pointerIndex == INVALID_POINTER)
			{
				return;
			}
			float x = MotionEventCompat.GetX(ev, pointerIndex);
			float dx = x - mLastMotionX;
			float xDiff = Math.Abs(dx);
			float y = MotionEventCompat.GetY(ev, pointerIndex);
			float dy = y - mLastMotionY;
			float yDiff = Math.Abs(dy);
			if (xDiff > (IsMenuOpen?mTouchSlop / 2:mTouchSlop) && xDiff > yDiff && ThisSlideAllowed(dx))
			{
				StartDrag();
				mLastMotionX = x;
				mLastMotionY = y;
				ScrollingCacheEnabled = true;
				// TODO add back in touch slop check
			}
			else if (xDiff > mTouchSlop)
			{
				mIsUnableToDrag = true;
			}
		}

		public override void ScrollTo(int x, int y)
		{
			base.ScrollTo(x, y);
			mScrollX = x;
			mViewBehind.ScrollBehindTo(mContent, x, y);
			((SlidingMenu)Parent).ManageLayers(PercentOpen);
		}

		private int DetermineTargetPage(float pageOffset, int velocity, int deltaX)
		{
			int targetPage = mCurItem;
			if (Math.Abs(deltaX) > mFlingDistance && Math.Abs(velocity) > mMinimumVelocity)
			{
				if (velocity > 0 && deltaX > 0)
				{
					targetPage -= 1;
				}
				else if (velocity < 0 && deltaX < 0)
				{
					targetPage += 1;
				}
			}
			else
			{
				targetPage = (int)Math.Round(mCurItem + pageOffset);
			}
			return targetPage;
		}

        public float PercentOpen
		{
			get
			{
				return Math.Abs(mScrollX - mContent.Left) / BehindWidth;
			}
		}

		protected override void DispatchDraw(Canvas canvas)
		{
			base.DispatchDraw(canvas);
			// Draw the margin drawable if needed.
			mViewBehind.DrawShadow(mContent, canvas);
			mViewBehind.DrawFade(mContent, canvas, PercentOpen);
			mViewBehind.DrawSelector(mContent, canvas, PercentOpen);
		}

		// variables for drawing
		private float mScrollX = 0.0f;

		private void OnSecondaryPointerUp(MotionEvent ev)
		{
			if (DEBUG)
			{
				Console.WriteLine(TAG + " OnSecondaryPointerUp called");
			}
			int pointerIndex = MotionEventCompat.GetActionIndex(ev);
			int pointerId = MotionEventCompat.GetPointerId(ev, pointerIndex);
			if (pointerId == mActivePointerId)
			{
				// This was our active pointer going up. Choose a new
				// active pointer and adjust accordingly.
				int newPointerIndex = pointerIndex == 0 ? 1 : 0;
				mLastMotionX = MotionEventCompat.GetX(ev, newPointerIndex);
				mActivePointerId = MotionEventCompat.GetPointerId(ev, newPointerIndex);
				if (mVelocityTracker != null)
				{
					mVelocityTracker.Clear();
				}
			}
		}

		private void StartDrag()
		{
			mIsBeingDragged = true;
			mQuickReturn = false;
		}

		private void EndDrag()
		{
			mQuickReturn = false;
			mIsBeingDragged = false;
			mIsUnableToDrag = false;
			mActivePointerId = INVALID_POINTER;

			if (mVelocityTracker != null)
			{
				mVelocityTracker.Recycle();
				mVelocityTracker = null;
			}
		}

		private bool ScrollingCacheEnabled
		{
			set
			{
				if (mScrollingCacheEnabled != value)
				{
					mScrollingCacheEnabled = value;
					if (USE_CACHE)
					{
						int size = ChildCount;
						for (int i = 0; i < size; ++i)
						{
							View child = GetChildAt(i);
							if (child.Visibility != ViewStates.Gone)
							{
								child.DrawingCacheEnabled = value;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Tests scrollability within child views of v given a delta of dx.
		/// </summary>
		/// <param name="v"> View to test for horizontal scrollability </param>
		/// <param name="checkV"> Whether the view v passed should itself be checked for scrollability (true),
		///               or just its children (false). </param>
		/// <param name="dx"> Delta scrolled in pixels </param>
		/// <param name="x"> X coordinate of the active touch point </param>
		/// <param name="y"> Y coordinate of the active touch point </param>
		/// <returns> true if child views of v can be scrolled by delta of dx. </returns>
		protected bool CanScroll(View v, bool checkV, int dx, int x, int y)
		{
			if (v is ViewGroup)
			{
				ViewGroup group = (ViewGroup) v;
				int scrollX = v.ScrollX;
				int scrollY = v.ScrollY;
				int count = group.ChildCount;
				// Count backwards - let topmost views consume scroll distance first.
				for (int i = count - 1; i >= 0; i--)
				{
					View child = group.GetChildAt(i);
					if (x + scrollX >= child.Left && x + scrollX < child.Right && y + scrollY >= child.Top && y + scrollY < child.Bottom && CanScroll(child, true, dx, x + scrollX - child.Left, y + scrollY - child.Top))
					{
						return true;
					}
				}
			}

			return checkV && ViewCompat.CanScrollHorizontally(v, -dx);
		}

		public override bool DispatchKeyEvent(KeyEvent e)
		{
			// Let the focused view and/or our descendants get the key first
			return base.DispatchKeyEvent(e) || ExecuteKeyEvent(e);
		}

		/// <summary>
		/// You can call this function yourself to have the scroll view perform
		/// scrolling from a key event, just as if the event had been dispatched to
		/// it by the view hierarchy.
		/// </summary>
		/// <param name="e"> The key event to execute. </param>
		/// <returns> Return true if the event was handled, else false. </returns>
		public bool ExecuteKeyEvent(KeyEvent e)
		{
			bool handled = false;
			if (e.Action == KeyEventActions.Down)
			{
				switch (e.KeyCode)
				{
				case Keycode.DpadLeft:
                    handled = ArrowScroll(FocusSearchDirection.Left);
					break;
                case Keycode.DpadRight:
                    handled = ArrowScroll(FocusSearchDirection.Right);
					break;
                case Keycode.Tab:
					if (Build.VERSION.SdkInt >= (BuildVersionCodes)11)
					{
						// The focus finder had a bug handling FOCUS_FORWARD and FOCUS_BACKWARD
						// before Android 3.0. Ignore the tab key on those devices.
						if (KeyEventCompat.HasNoModifiers(e))
						{
                            handled = ArrowScroll(FocusSearchDirection.Forward);
						}
						else if (KeyEventCompat.HasModifiers(e, (int)MetaKeyStates.ShiftOn))
						{
                            handled = ArrowScroll(FocusSearchDirection.Backward);
						}
					}
					break;
				}
			}
			return handled;
		}

		public bool ArrowScroll(FocusSearchDirection direction)
		{
			View currentFocused = FindFocus();
			if (currentFocused == this)
			{
				currentFocused = null;
			}

			bool handled = false;

			View nextFocused = FocusFinder.Instance.FindNextFocus(this, currentFocused, direction);
			if (nextFocused != null && nextFocused != currentFocused)
			{
                if (direction == FocusSearchDirection.Left)
				{
					handled = nextFocused.RequestFocus();
				}
                else if (direction == FocusSearchDirection.Right)
				{
					// If there is nothing to the right, or this is causing us to
					// jump to the left, then what we really want to do is page right.
					if (currentFocused != null && nextFocused.Left <= currentFocused.Left)
					{
						handled = PageRight();
					}
					else
					{
						handled = nextFocused.RequestFocus();
					}
				}
			}
            else if (direction == FocusSearchDirection.Left || direction == FocusSearchDirection.Backward)
			{
				// Trying to move left and nothing there; try to page.
				handled = PageLeft();
			}
            else if (direction == FocusSearchDirection.Right || direction == FocusSearchDirection.Forward)
			{
				// Trying to move right and nothing there; try to page.
				handled = PageRight();
			}
			if (handled)
			{
				PlaySoundEffect(SoundEffectConstants.GetContantForFocusDirection(direction));
			}
			return handled;
		}

		bool PageLeft()
		{
			if (mCurItem > 0)
			{
				SetCurrentItem(mCurItem - 1, true);
				return true;
			}
			return false;
		}

		bool PageRight()
		{
			if (mCurItem < 1)
			{
				SetCurrentItem(mCurItem + 1, true);
				return true;
			}
			return false;
		}
	}
}