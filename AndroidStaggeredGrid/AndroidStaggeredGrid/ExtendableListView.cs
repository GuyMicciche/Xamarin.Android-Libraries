using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Util;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using Java.Interop;
using Android.Runtime;

namespace AndroidStaggeredGrid
{
	/// <summary>
	/// An extendable implementation of the Android <seealso cref="android.widget.ListView"/>
	/// <p/>
	/// This is partly inspired by the incomplete StaggeredGridView supplied in the
	/// Android 4.2+ source & the <seealso cref="android.widget.AbsListView"/> & <seealso cref="android.widget.ListView"/> source;
	/// however this is intended to have a smaller simplified
	/// scope of functionality and hopefully therefore be a workable solution.
	/// <p/>
	/// Some things that this doesn't support (yet)
	/// - Dividers (We don't use them in our Etsy grid)
	/// - Edge effect
	/// - Fading edge - yuck
	/// - Item selection
	/// - Focus
	/// <p/>
	/// Note: we only really extend <seealso cref="android.widget.AbsListView"/> so we can appear to be one of its direct subclasses.
	/// However most of the code we need to modify is either 1. hidden or 2. package private
	/// So a lot of it's code and some <seealso cref="android.widget.AdapterView"/> code is repeated here
	/// Be careful with this - not everything may be how you expect if you assume this to be
	/// a regular old <seealso cref="android.widget.ListView"/>
	/// </summary>
	public abstract class ExtendableListView : AbsListView
	{
		private const string TAG = "ExtendableListView";

		private const bool DBG = false;

		private const int TOUCH_MODE_IDLE = 0;
		private const int TOUCH_MODE_SCROLLING = 1;
		private const int TOUCH_MODE_FLINGING = 2;
		private const int TOUCH_MODE_DOWN = 3;
		private const int TOUCH_MODE_TAP = 4;
		private const int TOUCH_MODE_DONE_WAITING = 5;

		private const int INVALID_POINTER = -1;

		// Layout using our default existing state
		private const int LAYOUT_NORMAL = 0;
		// Layout from the first item down
		private const int LAYOUT_FORCE_TOP = 1;
		// Layout from the saved instance state data
		private const int LAYOUT_SYNC = 2;

		private int mLayoutMode;

		private int mTouchMode;
        private ScrollState mScrollState = ScrollState.Idle;

		// Rectangle used for hit testing children
		// private Rect mTouchFrame;
		// TODO : ItemClick support from AdapterView

		// For managing scrolling
		private VelocityTracker mVelocityTracker = null;

		private int mTouchSlop;
		private int mMaximumVelocity;
		private int mFlingVelocity;

		// TODO : Edge effect handling
		// private EdgeEffectCompat mEdgeGlowTop;
		// private EdgeEffectCompat mEdgeGlowBottom;

		// blocker for when we're in a layout pass
		private bool mInLayout;

		public IListAdapter mAdapter;

		private int mMotionY;
		private int mMotionX;
		private int mMotionCorrection;
		private int mMotionPosition;

		private int mLastY;

		private int mActivePointerId = INVALID_POINTER;

		protected int mFirstPosition;

		// are we attached to a window - we shouldn't handle any touch events if we're not!
		private bool mIsAttached;

		/// <summary>
		/// When set to true, calls to requestLayout() will not propagate up the parent hierarchy.
		/// This is used to layout the children during a layout pass.
		/// </summary>
		private bool mBlockLayoutRequests = false;

		// has our data changed - and should we react to it
		private bool mDataChanged;
		private int mItemCount;
		private int mOldItemCount;

		public bool[] mIsScrap = new bool[1];

		private RecycleBin mRecycleBin;

		private AdapterDataSetObserver mObserver;
		private int mWidthMeasureSpec;
		private FlingRunnable mFlingRunnable;

		protected bool mClipToPadding;
		private PerformClick mPerformClick;

		private IRunnable mPendingCheckForTap;
		private CheckForLongPress mPendingCheckForLongPress;

		private class CheckForLongPress : WindowRunnnable, IRunnable
		{
			private ExtendableListView listview;

            public CheckForLongPress(ExtendableListView listview)
                : base(listview)
			{
                this.listview = listview;
			}

			public void Run()
			{
				int motionPosition = listview.mMotionPosition;
                View child = listview.GetChildAt(motionPosition);
				if (child != null)
				{
					int longPressPosition = listview.mMotionPosition;
					long longPressId = listview.mAdapter.GetItemId(listview.mMotionPosition + listview.mFirstPosition);

					bool handled = false;
					if (SameWindow() && !listview.mDataChanged)
					{
						handled = listview.PerformLongPress(child, longPressPosition + listview.mFirstPosition, longPressId);
					}
					if (handled)
					{
						listview.mTouchMode = TOUCH_MODE_IDLE;
                        listview.Pressed = false;
						child.Pressed = false;
					}
					else
					{
						listview.mTouchMode = TOUCH_MODE_DONE_WAITING;
					}

				}
			}
		}

		/// <summary>
		/// A class that represents a fixed view in a list, for example a header at the top
		/// or a footer at the bottom.
		/// </summary>
		public class FixedViewInfo
		{
            private readonly ExtendableListView listview;

            public FixedViewInfo(ExtendableListView listview)
			{
                this.listview = listview;
			}

			/// <summary>
			/// The view to add to the list
			/// </summary>
			public View view;
			/// <summary>
			/// The data backing the view. This is returned from <seealso cref="android.widget.ListAdapter#getItem(int)"/>.
			/// </summary>
			public Java.Lang.Object Data;
			/// <summary>
			/// <code>true</code> if the fixed view should be selectable in the list
			/// </summary>
			public bool IsSelectable;
		}

		private List<FixedViewInfo> mHeaderViewInfos;
		private List<FixedViewInfo> mFooterViewInfos;


		public ExtendableListView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
		{

			// setting up to be a scrollable view group
			SetWillNotDraw(false);
			SetClipToPadding(false);
			FocusableInTouchMode = false;

			ViewConfiguration viewConfiguration = ViewConfiguration.Get(context);
			mTouchSlop = viewConfiguration.ScaledTouchSlop;
			mMaximumVelocity = viewConfiguration.ScaledMaximumFlingVelocity;
			mFlingVelocity = viewConfiguration.ScaledMinimumFlingVelocity;

			mRecycleBin = new RecycleBin(this);
			mObserver = new AdapterDataSetObserver(this);

			mHeaderViewInfos = new List<FixedViewInfo>();
			mFooterViewInfos = new List<FixedViewInfo>();

			// start our layout mode drawing from the top
			mLayoutMode = LAYOUT_NORMAL;
		}

        public ExtendableListView(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {

        }


		// //////////////////////////////////////////////////////////////////////////////////////////
		// MAINTAINING SOME STATE
		//

		protected override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();

			if (mAdapter != null)
			{
				// Data may have changed while we were detached. Refresh.
				mDataChanged = true;
				mOldItemCount = mItemCount;
				mItemCount = mAdapter.Count;
			}
			mIsAttached = true;
		}

		protected override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();

			// Detach any view left in the scrap heap
			mRecycleBin.Clear();

			if (mFlingRunnable != null)
			{
				RemoveCallbacks(mFlingRunnable);
			}

			mIsAttached = false;
		}

        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            //    // TODO : handle focus and its impact on selection - if we add item selection support
        }

		public override void OnWindowFocusChanged(bool hasWindowFocus)
		{
			// TODO : handle focus and its impact on selection - if we add item selection support
		}

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
			OnSizeChanged(w, h);
		}

		protected virtual void OnSizeChanged(int w, int h)
		{
			if (ChildCount > 0)
			{
				StopFlingRunnable();
				mRecycleBin.Clear();
				mDataChanged = true;
				RememberSyncState();
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// ADAPTER
		//

		public override IListAdapter Adapter
		{
			get
			{
				return mAdapter;
			}
			set
			{
				if (mAdapter != null)
				{
					mAdapter.UnregisterDataSetObserver(mObserver);
				}
    
				// use a wrapper list value if we have a header or footer
				if (mHeaderViewInfos.Count > 0 || mFooterViewInfos.Count > 0)
				{
					mAdapter = new HeaderViewListAdapter(mHeaderViewInfos, mFooterViewInfos, value);
				}
				else
				{
					mAdapter = value;
				}
    
				mDataChanged = true;
				mItemCount = mAdapter != null ? mAdapter.Count : 0;
    
				if (mAdapter != null)
				{
					mAdapter.RegisterDataSetObserver(mObserver);
					mRecycleBin.ViewTypeCount = mAdapter.ViewTypeCount;
				}
    
				RequestLayout();
			}
		}

		public override int Count
		{
			get
			{
				return mItemCount;
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// ADAPTER VIEW - UNSUPPORTED
		//

		public override View SelectedView
		{
			get
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "getSelectedView() is not supported in ExtendableListView yet");
				}
				return null;
			}
		}

        public override void SetSelection(int position)
        {
            if (position >= 0)
            {
                mLayoutMode = LAYOUT_SYNC;
                mSpecificTop = ListPaddingTop;
    
                mFirstPosition = 0;
                if (mNeedSync)
                {
                    mSyncPosition = position;
                    mSyncRowId = mAdapter.GetItemId(position);
                }
                RequestLayout();
            }
        }

		// //////////////////////////////////////////////////////////////////////////////////////////
		// HEADER & FOOTER
		//

		/// <summary>
		/// Add a fixed view to appear at the top of the list. If addHeaderView is
		/// called more than once, the views will appear in the order they were
		/// added. Views added using this call can take focus if they want.
		/// <p/>
		/// NOTE: Call this before calling setAdapter. This is so ListView can wrap
		/// the supplied cursor with one that will also account for header and footer
		/// views.
		/// </summary>
		/// <param name="v">            The view to add. </param>
		/// <param name="data">         Data to associate with this view </param>
		/// <param name="isSelectable"> whether the item is selectable </param>
		public void AddHeaderView(View v, Java.Lang.Object data, bool isSelectable)
		{

			if (mAdapter != null && !(mAdapter is HeaderViewListAdapter))
			{
				throw new IllegalStateException("Cannot add header view to list -- setAdapter has already been called.");
			}

			FixedViewInfo info = new FixedViewInfo(this);
			info.view = v;
			info.Data = data;
			info.IsSelectable = isSelectable;
			mHeaderViewInfos.Add(info);

			// in the case of re-adding a header view, or adding one later on,
			// we need to notify the observer
			if (mAdapter != null && mObserver != null)
			{
				mObserver.OnChanged();
			}
		}

		/// <summary>
		/// Add a fixed view to appear at the top of the list. If addHeaderView is
		/// called more than once, the views will appear in the order they were
		/// added. Views added using this call can take focus if they want.
		/// <p/>
		/// NOTE: Call this before calling setAdapter. This is so ListView can wrap
		/// the supplied cursor with one that will also account for header and footer
		/// views.
		/// </summary>
		/// <param name="v"> The view to add. </param>
		public void AddHeaderView(View v)
		{
			AddHeaderView(v, null, true);
		}

		public int HeaderViewsCount
		{
			get
			{
				return mHeaderViewInfos.Count;
			}
		}

		/// <summary>
		/// Removes a previously-added header view.
		/// </summary>
		/// <param name="v"> The view to remove </param>
		/// <returns> true if the view was removed, false if the view was not a header
		/// view </returns>
		public bool RemoveHeaderView(View v)
		{
			if (mHeaderViewInfos.Count > 0)
			{
				bool result = false;
				if (mAdapter != null && ((HeaderViewListAdapter) mAdapter).RemoveHeader(v))
				{
					if (mObserver != null)
					{
						mObserver.OnChanged();
					}
					result = true;
				}
				RemoveFixedViewInfo(v, mHeaderViewInfos);
				return result;
			}
			return false;
		}

		private void RemoveFixedViewInfo(View v, List<FixedViewInfo> list)
		{
			int len = list.Count;
			for (int i = 0; i < len; ++i)
			{
				FixedViewInfo info = list[i];
				if (info.view == v)
				{
					list.RemoveAt(i);
					break;
				}
			}
		}

		/// <summary>
		/// Add a fixed view to appear at the bottom of the list. If addFooterView is
		/// called more than once, the views will appear in the order they were
		/// added. Views added using this call can take focus if they want.
		/// <p/>
		/// NOTE: Call this before calling setAdapter. This is so ListView can wrap
		/// the supplied cursor with one that will also account for header and footer
		/// views.
		/// </summary>
		/// <param name="v">            The view to add. </param>
		/// <param name="data">         Data to associate with this view </param>
		/// <param name="isSelectable"> true if the footer view can be selected </param>
		public void AddFooterView(View v, Java.Lang.Object data, bool isSelectable)
		{

			// NOTE: do not enforce the adapter being null here, since unlike in
			// addHeaderView, it was never enforced here, and so existing apps are
			// relying on being able to add a footer and then calling setAdapter to
			// force creation of the HeaderViewListAdapter wrapper

			FixedViewInfo info = new FixedViewInfo(this);
			info.view = v;
			info.Data = data;
			info.IsSelectable = isSelectable;
			mFooterViewInfos.Add(info);

			// in the case of re-adding a footer view, or adding one later on,
			// we need to notify the observer
			if (mAdapter != null && mObserver != null)
			{
				mObserver.OnChanged();
			}
		}

		/// <summary>
		/// Add a fixed view to appear at the bottom of the list. If addFooterView is called more
		/// than once, the views will appear in the order they were added. Views added using
		/// this call can take focus if they want.
		/// <p>NOTE: Call this before calling setAdapter. This is so ListView can wrap the supplied
		/// cursor with one that will also account for header and footer views.
		/// </summary>
		/// <param name="v"> The view to add. </param>
		public void AddFooterView(View v)
		{
			AddFooterView(v, null, true);
		}

		public int FooterViewsCount
		{
			get
			{
				return mFooterViewInfos.Count;
			}
		}

		/// <summary>
		/// Removes a previously-added footer view.
		/// </summary>
		/// <param name="v"> The view to remove </param>
		/// <returns> true if the view was removed, false if the view was not a footer view </returns>
		public bool RemoveFooterView(View v)
		{
			if (mFooterViewInfos.Count > 0)
			{
				bool result = false;
				if (mAdapter != null && ((HeaderViewListAdapter) mAdapter).RemoveFooter(v))
				{
					if (mObserver != null)
					{
						mObserver.OnChanged();
					}
					result = true;
				}
				RemoveFixedViewInfo(v, mFooterViewInfos);
				return result;
			}
			return false;
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// Property Overrides
		//

        public override void SetClipToPadding(bool clipToPadding)
        {
            base.SetClipToPadding(clipToPadding);
            mClipToPadding = clipToPadding;
        }

		// //////////////////////////////////////////////////////////////////////////////////////////
		// LAYOUT
		//

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public override void RequestLayout()
		{
			if (!mBlockLayoutRequests && !mInLayout)
			{
				base.RequestLayout();
			}
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			// super.onLayout(changed, l, t, r, b); - skipping base AbsListView implementation on purpose
			// haven't set an adapter yet? get to it
			if (mAdapter == null)
			{
				return;
			}

			if (changed)
			{
				int childCount = ChildCount;
				for (int i = 0; i < childCount; i++)
				{
					GetChildAt(i).ForceLayout();
				}
				mRecycleBin.MarkChildrenDirty();
			}

			// TODO get the height of the view??
			mInLayout = true;
			LayoutChildren();
			mInLayout = false;
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		protected override void LayoutChildren()
		{
			if (mBlockLayoutRequests)
			{
				return;
			}
			mBlockLayoutRequests = true;

			try
			{
				base.LayoutChildren();
				Invalidate();

				if (mAdapter == null)
				{
					ClearState();
					InvokeOnItemScrollListener();
					return;
				}

				int childrenTop = ListPaddingTop;

				int childCount = ChildCount;
				View oldFirst = null;

				// our last state so we keep our position
				if (mLayoutMode == LAYOUT_NORMAL)
				{
					oldFirst = GetChildAt(0);
				}

				bool dataChanged = mDataChanged;
				if (dataChanged)
				{
					HandleDataChanged();
				}

				// safety check!
				// Handle the empty set by removing all views that are visible
				// and calling it a day
				if (mItemCount == 0)
				{
					ClearState();
					InvokeOnItemScrollListener();
					return;
				}
				else if (mItemCount != mAdapter.Count)
				{
					throw new IllegalStateException("The content of the adapter has changed but " + "ExtendableListView did not receive a notification. Make sure the content of " + "your adapter is not modified from a background thread, but only " + "from the UI thread. [in ExtendableListView(" + Id + ", " + this.GetType() + ") with Adapter(" + mAdapter.GetType() + ")]");
				}

				// Pull all children into the RecycleBin.
				// These views will be reused if possible
				int firstPosition = mFirstPosition;
				RecycleBin recycleBin = mRecycleBin;

				if (dataChanged)
				{
					for (int i = 0; i < childCount; i++)
					{
						recycleBin.AddScrapView(GetChildAt(i), firstPosition + i);
					}
				}
				else
				{
					recycleBin.FillActiveViews(childCount, firstPosition);
				}

				// Clear out old views
				DetachAllViewsFromParent();
				recycleBin.RemoveSkippedScrap();

				switch (mLayoutMode)
				{
					case LAYOUT_FORCE_TOP:
					{
						mFirstPosition = 0;
						ResetToTop();
						AdjustViewsUpOrDown();
						FillFromTop(childrenTop);
						AdjustViewsUpOrDown();
						break;
					}
					case LAYOUT_SYNC:
					{
						FillSpecific(mSyncPosition, mSpecificTop);
						break;
					}
					case LAYOUT_NORMAL:
					default:
					{
						if (childCount == 0)
						{
							FillFromTop(childrenTop);
						}
						else if (mFirstPosition < mItemCount)
						{
							FillSpecific(mFirstPosition, oldFirst == null ? childrenTop : oldFirst.Top);
						}
						else
						{
							FillSpecific(0, childrenTop);
						}
						break;
					}
				}

				// Flush any cached views that did not get reused above
				recycleBin.ScrapActiveViews();
				mDataChanged = false;
				mNeedSync = false;
				mLayoutMode = LAYOUT_NORMAL;
				InvokeOnItemScrollListener();
			}
			finally
			{
				mBlockLayoutRequests = false;
			}
		}


		protected override void HandleDataChanged()
		{
			base.HandleDataChanged();

			int count = mItemCount;

			if (count > 0 && mNeedSync)
			{
				mNeedSync = false;
				mSyncState = null;

				mLayoutMode = LAYOUT_SYNC;
				mSyncPosition = System.Math.Min(System.Math.Max(0, mSyncPosition), count - 1);
				return;
			}

			mLayoutMode = LAYOUT_FORCE_TOP;
			mNeedSync = false;
			mSyncState = null;

			// TODO : add selection handling here
		}

		public virtual void ResetToTop()
		{
			// TO override
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// MEASUREMENT
		//

        /// <summary>
        /// {@inheritDoc}
        /// </summary>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            int heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            SetMeasuredDimension(widthSize, heightSize);
            mWidthMeasureSpec = widthMeasureSpec;
        }

		// //////////////////////////////////////////////////////////////////////////////////////////
		// ON TOUCH
		//

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public override bool OnTouchEvent(MotionEvent e)
		{
			// we're not passing this down as
			// all the touch handling is right here
			// super.onTouchEvent(event);

			if (!Enabled)
			{
				// A disabled view that is clickable still consumes the touch
				// events, it just doesn't respond to them.
				return Clickable || LongClickable;
			}

			InitVelocityTrackerIfNotExists();
			mVelocityTracker.AddMovement(e);

			if (!HasChildren())
			{
				return false;
			}

			bool handled;
            MotionEventActions action = e.Action & (MotionEventActions)MotionEventCompat.ActionMask;
			switch (action)
			{
				case MotionEventActions.Down:
					handled = OnTouchDown(e);
					break;

                case MotionEventActions.Move:
					handled = OnTouchMove(e);
					break;

                case MotionEventActions.Cancel:
					handled = OnTouchCancel(e);
					break;

                case MotionEventActions.PointerUp:
					handled = OnTouchPointerUp(e);
					break;

                case MotionEventActions.Up:
					handled = OnTouchUp(e);
					break;

				default:
					handled = false;
					break;
			}

			NotifyTouchMode();

			return handled;
		}


		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
            MotionEventActions action = ev.Action;

			if (!mIsAttached)
			{
				// Something isn't right.
				// Since we rely on being attached to get data set change notifications,
				// don't risk doing anything where we might try to resync and find things
				// in a bogus state.
				return false;
			}

			switch (action & MotionEventActions.Mask)
			{
                case MotionEventActions.Down:
				{
					int touchMode = mTouchMode;

					// TODO : overscroll
	//                if (touchMode == TOUCH_MODE_OVERFLING || touchMode == TOUCH_MODE_OVERSCROLL) {
	//                    mMotionCorrection = 0;
	//                    return true;
	//                }

					int x = (int) ev.GetX();
					int y = (int) ev.GetY();
					mActivePointerId = ev.GetPointerId(0);

					int motionPosition = FindMotionRow(y);
					if (touchMode != TOUCH_MODE_FLINGING && motionPosition >= 0)
					{
						// User clicked on an actual view (and was not stopping a fling).
						// Remember where the motion event started
						mMotionX = x;
						mMotionY = y;
						mMotionPosition = motionPosition;
						mTouchMode = TOUCH_MODE_DOWN;
					}
					mLastY = int.MinValue;
					InitOrResetVelocityTracker();
					mVelocityTracker.AddMovement(ev);
					if (touchMode == TOUCH_MODE_FLINGING)
					{
						return true;
					}
					break;
				}

                case MotionEventActions.Move:
				{
					switch (mTouchMode)
					{
						case TOUCH_MODE_DOWN:
							int pointerIndex = ev.FindPointerIndex(mActivePointerId);
							if (pointerIndex == -1)
							{
								pointerIndex = 0;
								mActivePointerId = ev.GetPointerId(pointerIndex);
							}
							int y = (int) ev.GetY(pointerIndex);
							InitVelocityTrackerIfNotExists();
							mVelocityTracker.AddMovement(ev);
							if (StartScrollIfNeeded(y))
							{
								return true;
							}
							break;
					}
					break;
				}

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
				{
					mTouchMode = TOUCH_MODE_IDLE;
					mActivePointerId = INVALID_POINTER;
					RecycleVelocityTracker();
					ReportScrollStateChange(ScrollState.Idle);
					break;
				}

                case MotionEventActions.PointerUp:
				{
					OnSecondaryPointerUp(ev);
					break;
				}
			}

			return false;
		}

		public override void RequestDisallowInterceptTouchEvent(bool disallowIntercept)
		{
			if (disallowIntercept)
			{
				RecycleVelocityTracker();
			}
			base.RequestDisallowInterceptTouchEvent(disallowIntercept);
		}

		public class CheckForTap : Java.Lang.Object, IRunnable
		{
			private readonly ExtendableListView listview;

            public CheckForTap(ExtendableListView listview)
			{
                this.listview = listview;
			}

			public void Run()
			{
				if (listview.mTouchMode == TOUCH_MODE_DOWN)
				{
					listview.mTouchMode = TOUCH_MODE_TAP;
                    View child = listview.GetChildAt(listview.mMotionPosition);
					if (child != null && !child.HasFocusable)
					{
						listview.mLayoutMode = LAYOUT_NORMAL;

						if (!listview.mDataChanged)
						{
							listview.LayoutChildren();
							child.Pressed = true;
                            listview.Pressed = true;

							int longPressTimeout = ViewConfiguration.LongPressTimeout;
                            bool longClickable = listview.LongClickable;

							if (longClickable)
							{
								if (listview.mPendingCheckForLongPress == null)
								{
									listview.mPendingCheckForLongPress = new CheckForLongPress(listview);
								}
								listview.mPendingCheckForLongPress.RememberWindowAttachCount();
                                listview.PostDelayed(listview.mPendingCheckForLongPress, longPressTimeout);
							}
							else
							{
								listview.mTouchMode = TOUCH_MODE_DONE_WAITING;
							}
						}
						else
						{
							listview.mTouchMode = TOUCH_MODE_DONE_WAITING;
						}
					}
				}
			}
		}

		private bool OnTouchDown(MotionEvent e)
		{
			int x = (int)e.GetX();
			int y = (int)e.GetY();
			int motionPosition = PointToPosition(x, y);

			mVelocityTracker.Clear();
			mActivePointerId = MotionEventCompat.GetPointerId(e, 0);

			// TODO : use the motion position for fling support
			// TODO : support long press!
			// startLongPressCheck();

			if ((mTouchMode != TOUCH_MODE_FLINGING) && !mDataChanged && motionPosition >= 0 && Adapter.IsEnabled(motionPosition))
			{
				// is it a tap or a scroll .. we don't know yet!
				mTouchMode = TOUCH_MODE_DOWN;

				if (mPendingCheckForTap == null)
				{
					mPendingCheckForTap = new CheckForTap(this);
				}
				PostDelayed(mPendingCheckForTap, ViewConfiguration.TapTimeout);

				if (e.EdgeFlags != 0 && motionPosition < 0)
				{
					// If we couldn't find a view to click on, but the down event was touching
					// the edge, we will bail out and try again. This allows the edge correcting
					// code in ViewRoot to try to find a nearby view to select
					return false;
				}
			}
			else if (mTouchMode == TOUCH_MODE_FLINGING)
			{
				mTouchMode = TOUCH_MODE_SCROLLING;
				mMotionCorrection = 0;
				motionPosition = FindMotionRow(y);
			}

			mMotionX = x;
			mMotionY = y;
			mMotionPosition = motionPosition;
			mLastY = int.MinValue;

			return true;
		}

		private bool OnTouchMove(MotionEvent @event)
		{
			int index = MotionEventCompat.FindPointerIndex(@event, mActivePointerId);
			if (index < 0)
			{
				Console.WriteLine(TAG, "onTouchMove could not find pointer with id " + mActivePointerId + " - did ExtendableListView receive an inconsistent " + "event stream?");
				return false;
			}
			int y = (int)MotionEventCompat.GetY(@event, index);

			// our data's changed so we need to do a layout before moving any further
			if (mDataChanged)
			{
				LayoutChildren();
			}

			switch (mTouchMode)
			{
				case TOUCH_MODE_DOWN:
				case TOUCH_MODE_TAP:
				case TOUCH_MODE_DONE_WAITING:
					// Check if we have moved far enough that it looks more like a
					// scroll than a tap
					StartScrollIfNeeded(y);
					break;
				case TOUCH_MODE_SCROLLING:
	//            case TOUCH_MODE_OVERSCROLL:
					ScrollIfNeeded(y);
					break;
			}

			return true;
		}


		private bool OnTouchCancel(MotionEvent @event)
		{
			mTouchMode = TOUCH_MODE_IDLE;
			Pressed = false;
			Invalidate(); // redraw selector
			Handler handler = Handler;

			if (handler != null)
			{
				handler.RemoveCallbacks(mPendingCheckForLongPress);
			}

			RecycleVelocityTracker();
			mActivePointerId = INVALID_POINTER;
			return true;
		}

		private bool OnTouchUp(MotionEvent @event)
		{
			switch (mTouchMode)
			{
				case TOUCH_MODE_DOWN:
				case TOUCH_MODE_TAP:
				case TOUCH_MODE_DONE_WAITING:
					return OnTouchUpTap(@event);

				case TOUCH_MODE_SCROLLING:
					return OnTouchUpScrolling(@event);
			}

			Pressed = false;
			Invalidate(); // redraw selector

			Handler handler = Handler;
			if (handler != null)
			{
				handler.RemoveCallbacks(mPendingCheckForLongPress);
			}

			RecycleVelocityTracker();

			mActivePointerId = INVALID_POINTER;
			return true;
		}

		private bool OnTouchUpScrolling(MotionEvent @event)
		{
			if (HasChildren())
			{
				// 2 - Are we at the top or bottom?
				int top = FirstChildTop;
				int bottom = LastChildBottom;
				bool atEdge = mFirstPosition == 0 && top >= ListPaddingTop && mFirstPosition + ChildCount < mItemCount && bottom <= Height - ListPaddingBottom;

				if (!atEdge)
				{
					mVelocityTracker.ComputeCurrentVelocity(1000, mMaximumVelocity);
					float velocity = mVelocityTracker.GetYVelocity(mActivePointerId);

					if (System.Math.Abs(velocity) > mFlingVelocity)
					{
						StartFlingRunnable(velocity);
						mTouchMode = TOUCH_MODE_FLINGING;
						mMotionY = 0;
						Invalidate();
						return true;
					}
				}
			}

			StopFlingRunnable();
			RecycleVelocityTracker();
			mTouchMode = TOUCH_MODE_IDLE;
			return true;
		}

		private bool OnTouchUpTap(MotionEvent @event)
		{
			int motionPosition = mMotionPosition;
			if (motionPosition >= 0)
			{
				View child = GetChildAt(motionPosition);
				if (child != null && !child.HasFocusable)
				{
					if (mTouchMode != TOUCH_MODE_DOWN)
					{
						child.Pressed = false;
					}

					if (mPerformClick == null)
					{
						Invalidate();
						mPerformClick = new PerformClick(this);
					}

					PerformClick performClick = mPerformClick;
					performClick.mClickMotionPosition = motionPosition;
					performClick.RememberWindowAttachCount();

		//            mResurrectToPosition = motionPosition;

					if (mTouchMode == TOUCH_MODE_DOWN || mTouchMode == TOUCH_MODE_TAP)
					{
						Handler handler = Handler;
						if (handler != null)
						{
							handler.RemoveCallbacks(mTouchMode == TOUCH_MODE_DOWN ? mPendingCheckForTap : mPendingCheckForLongPress);
						}
						mLayoutMode = LAYOUT_NORMAL;
						if (!mDataChanged && motionPosition >= 0 && mAdapter.IsEnabled(motionPosition))
						{
							mTouchMode = TOUCH_MODE_TAP;
							LayoutChildren();
							child.Pressed = true;
							Pressed = true;
                            PostDelayed(() =>
                                {
                                    child.Pressed = false;
                                    Pressed = false;
                                    if (!mDataChanged)
                                    {
                                        Post(performClick);
                                    }
                                    mTouchMode = TOUCH_MODE_IDLE;
                                }, 0);
						}
						else
						{
							mTouchMode = TOUCH_MODE_IDLE;
						}
						return true;
					}
					else if (!mDataChanged && motionPosition >= 0 && mAdapter.IsEnabled(motionPosition))
					{
						Post(performClick);
					}
				}
			}
			mTouchMode = TOUCH_MODE_IDLE;

			return true;
		}

		private bool OnTouchPointerUp(MotionEvent e)
		{
			OnSecondaryPointerUp(e);
			int x = mMotionX;
			int y = mMotionY;
			int motionPosition = PointToPosition(x, y);
			if (motionPosition >= 0)
			{
				mMotionPosition = motionPosition;
			}
			mLastY = y;
			return true;
		}

		private void OnSecondaryPointerUp(MotionEvent e)
		{
            int pointerIndex = ((int)e.Action & MotionEventCompat.ActionPointerIndexMask) >> MotionEventCompat.ActionPointerIndexShift;
			int pointerId = e.GetPointerId(pointerIndex);
			if (pointerId == mActivePointerId)
			{
				// This was our active pointer going up. Choose a new
				// active pointer and adjust accordingly.
				// TODO: Make this decision more intelligent.
				int newPointerIndex = pointerIndex == 0 ? 1 : 0;
				mMotionX = (int) e.GetX(newPointerIndex);
				mMotionY = (int) e.GetY(newPointerIndex);
				mActivePointerId = e.GetPointerId(newPointerIndex);
				RecycleVelocityTracker();
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// SCROLL HELPERS
		//

		/// <summary>
		/// Starts a scroll that moves the difference between y and our last motions y
		/// if it's a movement that represents a big enough scroll.
		/// </summary>
		private bool StartScrollIfNeeded(int y)
		{
			int deltaY = y - mMotionY;
			int distance = System.Math.Abs(deltaY);
			// TODO : Overscroll?
			// final boolean overscroll = mScrollY != 0;
			const bool overscroll = false;
			if (overscroll || distance > mTouchSlop)
			{
				if (overscroll)
				{
					mMotionCorrection = 0;
				}
				else
				{
					mTouchMode = TOUCH_MODE_SCROLLING;
					mMotionCorrection = deltaY > 0 ? mTouchSlop : -mTouchSlop;
				}

				Handler handler = Handler;
				if (handler != null)
				{
					handler.RemoveCallbacks(mPendingCheckForLongPress);
				}
				Pressed = false;
				View motionView = GetChildAt(mMotionPosition - mFirstPosition);
				if (motionView != null)
				{
					motionView.Pressed = false;
				}
				IViewParent parent = Parent;
				if (parent != null)
				{
					parent.RequestDisallowInterceptTouchEvent(true);
				}

				ScrollIfNeeded(y);
				return true;
			}
			return false;
		}

		private void ScrollIfNeeded(int y)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "scrollIfNeeded y: " + y);
			}
			int rawDeltaY = y - mMotionY;
			int deltaY = rawDeltaY - mMotionCorrection;
			int incrementalDeltaY = mLastY != int.MinValue ? y - mLastY : deltaY;

			if (mTouchMode == TOUCH_MODE_SCROLLING)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "scrollIfNeeded TOUCH_MODE_SCROLLING");
				}
				if (y != mLastY)
				{
					// stop our parent
					if (System.Math.Abs(rawDeltaY) > mTouchSlop)
					{
						IViewParent parent = Parent;
						if (parent != null)
						{
							parent.RequestDisallowInterceptTouchEvent(true);
						}
					}

					int motionIndex;
					if (mMotionPosition >= 0)
					{
						motionIndex = mMotionPosition - mFirstPosition;
					}
					else
					{
						// If we don't have a motion position that we can reliably track,
						// pick something in the middle to make a best guess at things below.
						motionIndex = ChildCount / 2;
					}

					// No need to do all this work if we're not going to move anyway
					bool atEdge = false;
					if (incrementalDeltaY != 0)
					{
						atEdge = MoveTheChildren(deltaY, incrementalDeltaY);
					}

					// Check to see if we have bumped into the scroll limit
					View motionView = this.GetChildAt(motionIndex);
					if (motionView != null)
					{
						if (atEdge)
						{
							// TODO : edge effect & overscroll
						}
						mMotionY = y;
					}
					mLastY = y;
				}

			}
			// TODO : ELSE SUPPORT OVERSCROLL!
		}

		private int FindMotionRow(int y)
		{
			int childCount = ChildCount;
			if (childCount > 0)
			{
				// always from the top
				for (int i = 0; i < childCount; i++)
				{
					View v = GetChildAt(i);
					if (y <= v.Bottom)
					{
						return mFirstPosition + i;
					}
				}
			}
			return InvalidPosition;
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// MOVING STUFF!
		//
		// It's not scrolling - we're just moving views!
		// Move our views and implement view recycling to show new views if necessary

		// move our views by deltaY - what's the incrementalDeltaY?
		private bool MoveTheChildren(int deltaY, int incrementalDeltaY)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "moveTheChildren deltaY: " + deltaY + "incrementalDeltaY: " + incrementalDeltaY);
			}
			// there's nothing to move!
			if (!HasChildren())
			{
				return true;
			}

			int firstTop = HighestChildTop;
			int lastBottom = LowestChildBottom;

			// "effective padding" In this case is the amount of padding that affects
			// how much space should not be filled by items. If we don't clip to padding
			// there is no effective padding.
			int effectivePaddingTop = 0;
			int effectivePaddingBottom = 0;
			if (mClipToPadding)
			{
				effectivePaddingTop = ListPaddingTop;
				effectivePaddingBottom = ListPaddingBottom;
			}

			int gridHeight = Height;
			int spaceAbove = effectivePaddingTop - FirstChildTop;
			int end = gridHeight - effectivePaddingBottom;
			int spaceBelow = LastChildBottom - end;

			int height = gridHeight - ListPaddingBottom - ListPaddingTop;

			if (incrementalDeltaY < 0)
			{
				incrementalDeltaY = System.Math.Max(-(height - 1), incrementalDeltaY);
			}
			else
			{
				incrementalDeltaY = System.Math.Min(height - 1, incrementalDeltaY);
			}

			int firstPosition = mFirstPosition;

			int maxTop = ListPaddingTop;
			int maxBottom = gridHeight - ListPaddingBottom;
			int childCount = ChildCount;

			bool cannotScrollDown = (firstPosition == 0 && firstTop >= maxTop && incrementalDeltaY >= 0);
			bool cannotScrollUp = (firstPosition + childCount == mItemCount && lastBottom <= maxBottom && incrementalDeltaY <= 0);

			if (DBG)
			{
				Console.WriteLine(TAG, "moveTheChildren " + " firstTop " + firstTop + " maxTop " + maxTop + " incrementalDeltaY " + incrementalDeltaY);
				Console.WriteLine(TAG, "moveTheChildren " + " lastBottom " + lastBottom + " maxBottom " + maxBottom + " incrementalDeltaY " + incrementalDeltaY);
			}

			if (cannotScrollDown)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "moveTheChildren cannotScrollDown " + cannotScrollDown);
				}
				return incrementalDeltaY != 0;
			}

			if (cannotScrollUp)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "moveTheChildren cannotScrollUp " + cannotScrollUp);
				}
				return incrementalDeltaY != 0;
			}

			bool isDown = incrementalDeltaY < 0;

			int headerViewsCount = HeaderViewsCount;
			int footerViewsStart = mItemCount - FooterViewsCount;

			int start = 0;
			int count = 0;

			if (isDown)
			{
				int top = -incrementalDeltaY;
				if (mClipToPadding)
				{
					top += ListPaddingTop;
				}
				for (int i = 0; i < childCount; i++)
				{
					View child = GetChildAt(i);
					if (child.Bottom >= top)
					{
						break;
					}
					else
					{
						count++;
						int position = firstPosition + i;
						if (position >= headerViewsCount && position < footerViewsStart)
						{
							mRecycleBin.AddScrapView(child, position);
						}
					}
				}
			}
			else
			{
				int bottom = gridHeight - incrementalDeltaY;
				if (mClipToPadding)
				{
					bottom -= ListPaddingBottom;
				}
				for (int i = childCount - 1; i >= 0; i--)
				{
					View child = GetChildAt(i);
					if (child.Top <= bottom)
					{
						break;
					}
					else
					{
						start = i;
						count++;
						int position = firstPosition + i;
						if (position >= headerViewsCount && position < footerViewsStart)
						{
							mRecycleBin.AddScrapView(child, position);
						}
					}
				}
			}

			mBlockLayoutRequests = true;

			if (count > 0)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "scrap - detachViewsFromParent start:" + start + " count:" + count);
				}
				DetachViewsFromParent(start, count);
				mRecycleBin.RemoveSkippedScrap();
				OnChildrenDetached(start, count);
			}

			// invalidate before moving the children to avoid unnecessary invalidate
			// calls to bubble up from the children all the way to the top
			if (!AwakenScrollBars())
			{
				Invalidate();
			}

			OffsetChildrenTopAndBottom(incrementalDeltaY);

			if (isDown)
			{
				mFirstPosition += count;
			}

			int absIncrementalDeltaY = System.Math.Abs(incrementalDeltaY);
			if (spaceAbove < absIncrementalDeltaY || spaceBelow < absIncrementalDeltaY)
			{
				FillGap(isDown);
			}

			// TODO : touch mode selector handling
			mBlockLayoutRequests = false;
			InvokeOnItemScrollListener();

			return false;
		}

        protected virtual void OnChildrenDetached(int start, int count)
		{

		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// FILLING THE GRID!
		//

		/// <summary>
		/// As we move and scroll and recycle views we want to fill the gap created with new views
		/// </summary>
		protected void FillGap(bool down)
		{
			int count = ChildCount;
			if (down)
			{
				// fill down from the top of the position below our last
				int position = mFirstPosition + count;
				int startOffset = GetChildTop(position);
				FillDown(position, startOffset);
			}
			else
			{
				// fill up from the bottom of the position above our first.
				int position = mFirstPosition - 1;
				int startOffset = GetChildBottom(position);
				FillUp(position, startOffset);
			}
			AdjustViewsAfterFillGap(down);
		}

        protected virtual void AdjustViewsAfterFillGap(bool down)
		{
			if (down)
			{
				CorrectTooHigh(ChildCount);
			}
			else
			{
				CorrectTooLow(ChildCount);
			}
		}

		private View FillDown(int pos, int nextTop)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "fillDown - pos:" + pos + " nextTop:" + nextTop);
			}

			View selectedView = null;

			int end = Height;
			if (mClipToPadding)
			{
				end -= ListPaddingBottom;
			}

			while ((nextTop < end || HasSpaceDown()) && pos < mItemCount)
			{
				// TODO : add selection support
				MakeAndAddView(pos, nextTop, true, false);
				pos++;
				nextTop = GetNextChildDownsTop(pos); // = child.getBottom();
			}

			return selectedView;
		}

		/// <summary>
		///*
		/// Override to tell filling flow to continue to fill up as we have space.
		/// </summary>
		protected bool HasSpaceDown()
		{
			return false;
		}

		private View FillUp(int pos, int nextBottom)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "fillUp - position:" + pos + " nextBottom:" + nextBottom);
			}
			View selectedView = null;

			int end = mClipToPadding ? ListPaddingTop : 0;

			while ((nextBottom > end || HasSpaceUp()) && pos >= 0)
			{
				// TODO : add selection support
				MakeAndAddView(pos, nextBottom, false, false);
				pos--;
				nextBottom = GetNextChildUpsBottom(pos);
				if (DBG)
				{
					Console.WriteLine(TAG, "fillUp next - position:" + pos + " nextBottom:" + nextBottom);
				}
			}

			mFirstPosition = pos + 1;
			return selectedView;
		}

		/// <summary>
		///*
		/// Override to tell filling flow to continue to fill up as we have space.
		/// </summary>
		protected virtual bool HasSpaceUp()
		{
			return false;
		}

		/// <summary>
		/// Fills the list from top to bottom, starting with mFirstPosition
		/// </summary>
		private View FillFromTop(int nextTop)
		{
			mFirstPosition = System.Math.Min(mFirstPosition, mItemCount - 1);
			if (mFirstPosition < 0)
			{
				mFirstPosition = 0;
			}
			return FillDown(mFirstPosition, nextTop);
		}

		/// <summary>
		/// Put a specific item at a specific location on the screen and then build
		/// up and down from there.
		/// </summary>
		/// <param name="position"> The reference view to use as the starting point </param>
		/// <param name="top">      Pixel offset from the top of this view to the top of the
		///                 reference view. </param>
		/// <returns> The selected view, or null if the selected view is outside the
		/// visible area. </returns>
		private View FillSpecific(int position, int top)
		{
			bool tempIsSelected = false; // ain't no body got time for that @ Etsy
			View temp = MakeAndAddView(position, top, true, tempIsSelected);
			// Possibly changed again in fillUp if we add rows above this one.
			mFirstPosition = position;

			View above;
			View below;

			int nextBottom = GetNextChildUpsBottom(position - 1);
			int nextTop = GetNextChildDownsTop(position + 1);

			above = FillUp(position - 1, nextBottom);
			// This will correct for the top of the first view not touching the top of the list
			AdjustViewsUpOrDown();
			below = FillDown(position + 1, nextTop);
			int childCount = ChildCount;
			if (childCount > 0)
			{
				CorrectTooHigh(childCount);
			}

			if (tempIsSelected)
			{
				return temp;
			}
			else if (above != null)
			{
				return above;
			}
			else
			{
				return below;
			}
		}

		/// <summary>
		/// Gets a view either a new view an unused view?? or a recycled view and adds it to our children
		/// </summary>
		private View MakeAndAddView(int position, int y, bool flowDown, bool selected)
		{
			View child;

			OnChildCreated(position, flowDown);

			if (!mDataChanged)
			{
				// Try to use an existing view for this position
				child = mRecycleBin.GetActiveView(position);
				if (child != null)
				{

					// Found it -- we're using an existing child
					// This just needs to be positioned
					SetupChild(child, position, y, flowDown, selected, true);
					return child;
				}
			}

			// Make a new view for this position, or convert an unused view if possible
			child = ObtainView(position, mIsScrap);
			// This needs to be positioned and measured
			SetupChild(child, position, y, flowDown, selected, mIsScrap[0]);

			return child;
		}

		/// <summary>
		/// Add a view as a child and make sure it is measured (if necessary) and
		/// positioned properly.
		/// </summary>
		/// <param name="child">    The view to add </param>
		/// <param name="position"> The position of this child </param>
		/// <param name="y">        The y position relative to which this view will be positioned </param>
		/// <param name="flowDown"> If true, align top edge to y. If false, align bottom
		///                 edge to y. </param>
		/// <param name="selected"> Is this position selected? </param>
		/// <param name="recycled"> Has this view been pulled from the recycle bin? If so it
		///                 does not need to be remeasured. </param>
		private void SetupChild(View child, int position, int y, bool flowDown, bool selected, bool recycled)
		{
			const bool isSelected = false; // TODO : selected && shouldShowSelector();
			bool updateChildSelected = isSelected != child.Selected;
			int mode = mTouchMode;
			bool isPressed = mode > TOUCH_MODE_DOWN && mode < TOUCH_MODE_SCROLLING && mMotionPosition == position;
			bool updateChildPressed = isPressed != child.Pressed;
			bool needToMeasure = !recycled || updateChildSelected || child.IsLayoutRequested;

			int itemViewType = mAdapter.GetItemViewType(position);

			LayoutParams layoutParams;
			if (itemViewType == ItemViewTypeHeaderOrFooter)
			{
				layoutParams = GenerateWrapperLayoutParams(child);
			}
			else
			{
				layoutParams = GenerateChildLayoutParams(child);
			}

			layoutParams.viewType = itemViewType;
			layoutParams.position = position;

			if (recycled || (layoutParams.recycledHeaderFooter && layoutParams.viewType == AdapterView.ItemViewTypeHeaderOrFooter))
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "setupChild attachViewToParent position:" + position);
				}
				AttachViewToParent(child, flowDown ? - 1 : 0, layoutParams);
			}
			else
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "setupChild addViewInLayout position:" + position);
				}
				if (layoutParams.viewType == AdapterView.ItemViewTypeHeaderOrFooter)
				{
					layoutParams.recycledHeaderFooter = true;
				}
				AddViewInLayout(child, flowDown ? - 1 : 0, layoutParams, true);
			}

			if (updateChildSelected)
			{
				child.Selected = isSelected;
			}

			if (updateChildPressed)
			{
				child.Pressed = isPressed;
			}

			if (needToMeasure)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "setupChild onMeasureChild position:" + position);
				}
				OnMeasureChild(child, layoutParams);
			}
			else
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "setupChild cleanupLayoutState position:" + position);
				}
				CleanupLayoutState(child);
			}

			int w = child.MeasuredWidth;
			int h = child.MeasuredHeight;
			int childTop = flowDown ? y : y - h;

			if (DBG)
			{
				Console.WriteLine(TAG, "setupChild position:" + position + " h:" + h + " w:" + w);
			}

			int childrenLeft = GetChildLeft(position);

			if (needToMeasure)
			{
				int childRight = childrenLeft + w;
				int childBottom = childTop + h;
				OnLayoutChild(child, position, flowDown, childrenLeft, childTop, childRight, childBottom);
			}
			else
			{
				OnOffsetChild(child, position, flowDown, childrenLeft, childTop);
			}

		}

        protected virtual LayoutParams GenerateChildLayoutParams(View child)
		{
			return GenerateWrapperLayoutParams(child);
		}

		protected LayoutParams GenerateWrapperLayoutParams(View child)
		{
			LayoutParams layoutParams = null;

			ViewGroup.LayoutParams childParams = child.LayoutParameters;
			if (childParams != null)
			{
				if (childParams is LayoutParams)
				{
					layoutParams = (LayoutParams) childParams;
				}
				else
				{
					layoutParams = new LayoutParams(childParams);
				}
			}
			if (layoutParams == null)
			{
				layoutParams = GenerateDefaultLayoutParams();
			}

			return layoutParams;
		}


		/// <summary>
		/// Measures a child view in the list. Should call
		/// </summary>
		protected virtual void OnMeasureChild(View child, LayoutParams layoutParams)
		{
			int childWidthSpec = ViewGroup.GetChildMeasureSpec(mWidthMeasureSpec, ListPaddingLeft + ListPaddingRight, layoutParams.Width);
			int lpHeight = layoutParams.Height;
			int childHeightSpec;
			if (lpHeight > 0)
			{
				childHeightSpec = MeasureSpec.MakeMeasureSpec(lpHeight, MeasureSpecMode.Exactly);
			}
			else
			{
				childHeightSpec = MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
			}
			child.Measure(childWidthSpec, childHeightSpec);
		}

		protected LayoutParams GenerateDefaultLayoutParams()
		{
			return new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0);
		}

		protected LayoutParams GenerateHeaderFooterLayoutParams(View child)
		{
            return new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 0);
		}

		/// <summary>
		/// Get a view and have it show the data associated with the specified
		/// position. This is called when we have already discovered that the view is
		/// not available for reuse in the recycle bin. The only choices left are
		/// converting an old view or making a new one.
		/// </summary>
		/// <param name="position"> The position to display </param>
		/// <param name="isScrap">  Array of at least 1 boolean, the first entry will become true if
		///                 the returned view was taken from the scrap heap, false if otherwise. </param>
		/// <returns> A view displaying the data associated with the specified position </returns>
		private View ObtainView(int position, bool[] isScrap)
		{
			isScrap[0] = false;
			View scrapView;

			scrapView = mRecycleBin.GetScrapView(position);

			View child;
			if (scrapView != null)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "getView from scrap position:" + position);
				}
				child = mAdapter.GetView(position, scrapView, this);

				if (child != scrapView)
				{
					mRecycleBin.AddScrapView(scrapView, position);
				}
				else
				{
					isScrap[0] = true;
				}
			}
			else
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "getView position:" + position);
				}
				child = mAdapter.GetView(position, null, this);
			}

			return child;
		}


		/// <summary>
		/// Check if we have dragged the bottom of the list too high (we have pushed the
		/// top element off the top of the screen when we did not need to). Correct by sliding
		/// everything back down.
		/// </summary>
		/// <param name="childCount"> Number of children </param>
		private void CorrectTooHigh(int childCount)
		{
			// First see if the last item is visible. If it is not, it is OK for the
			// top of the list to be pushed up.
			int lastPosition = mFirstPosition + childCount - 1;
			if (lastPosition == mItemCount - 1 && childCount > 0)
			{

				// ... and its bottom edge
				int lastBottom = LowestChildBottom;

				// This is bottom of our drawable area
				int end = (Bottom - Top) - ListPaddingBottom;

				// This is how far the bottom edge of the last view is from the bottom of the
				// drawable area
				int bottomOffset = end - lastBottom;

				int firstTop = HighestChildTop;

				// Make sure we are 1) Too high, and 2) Either there are more rows above the
				// first row or the first row is scrolled off the top of the drawable area
				if (bottomOffset > 0 && (mFirstPosition > 0 || firstTop < ListPaddingTop))
				{
					if (mFirstPosition == 0)
					{
						// Don't pull the top too far down
						bottomOffset = System.Math.Min(bottomOffset, ListPaddingTop - firstTop);
					}
					// Move everything down
					OffsetChildrenTopAndBottom(bottomOffset);
					if (mFirstPosition > 0)
					{
						// Fill the gap that was opened above mFirstPosition with more rows, if
						// possible
						int previousPosition = mFirstPosition - 1;
						FillUp(previousPosition, GetNextChildUpsBottom(previousPosition));
						// Close up the remaining gap
						AdjustViewsUpOrDown();
					}

				}
			}
		}

		/// <summary>
		/// Check if we have dragged the bottom of the list too low (we have pushed the
		/// bottom element off the bottom of the screen when we did not need to). Correct by sliding
		/// everything back up.
		/// </summary>
		/// <param name="childCount"> Number of children </param>
		private void CorrectTooLow(int childCount)
		{
			// First see if the first item is visible. If it is not, it is OK for the
			// bottom of the list to be pushed down.
			if (mFirstPosition == 0 && childCount > 0)
			{

				// ... and its top edge
				int firstTop = HighestChildTop;

				// This is top of our drawable area
				int start = ListPaddingTop;

				// This is bottom of our drawable area
				int end = (Top - Bottom) - ListPaddingBottom;

				// This is how far the top edge of the first view is from the top of the
				// drawable area
				int topOffset = firstTop - start;
				int lastBottom = LowestChildBottom;

				int lastPosition = mFirstPosition + childCount - 1;

				// Make sure we are 1) Too low, and 2) Either there are more rows below the
				// last row or the last row is scrolled off the bottom of the drawable area
				if (topOffset > 0)
				{
					if (lastPosition < mItemCount - 1 || lastBottom > end)
					{
						if (lastPosition == mItemCount - 1)
						{
							// Don't pull the bottom too far up
							topOffset = System.Math.Min(topOffset, lastBottom - end);
						}
						// Move everything up
						OffsetChildrenTopAndBottom(-topOffset);
						if (lastPosition < mItemCount - 1)
						{
							// Fill the gap that was opened below the last position with more rows, if
							// possible
							int nextPosition = lastPosition + 1;
							FillDown(nextPosition, GetNextChildDownsTop(nextPosition));
							// Close up the remaining gap
							AdjustViewsUpOrDown();
						}
					}
					else if (lastPosition == mItemCount - 1)
					{
						AdjustViewsUpOrDown();
					}
				}
			}
		}

		/// <summary>
		/// Make sure views are touching the top or bottom edge, as appropriate for
		/// our gravity
		/// </summary>
		private void AdjustViewsUpOrDown()
		{
			int childCount = ChildCount;
			int delta;

			if (childCount > 0)
			{
				// Uh-oh -- we came up short. Slide all views up to make them
				// align with the top
				delta = HighestChildTop - ListPaddingTop;
				if (delta < 0)
				{
					// We only are looking to see if we are too low, not too high
					delta = 0;
				}

				if (delta != 0)
				{
					OffsetChildrenTopAndBottom(-delta);
				}
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// PROTECTED POSITIONING EXTENSABLES
		//

		/// <summary>
		/// Override
		/// </summary>
		protected virtual void OnChildCreated(int position, bool flowDown)
		{

		}

		/// <summary>
		/// Override to position the child as you so wish
		/// </summary>
		protected virtual void OnLayoutChild(View child, int position, bool flowDown, int childrenLeft, int childTop, int childRight, int childBottom)
		{
			child.Layout(childrenLeft, childTop, childRight, childBottom);
		}

		/// <summary>
		/// Override to offset the child as you so wish
		/// </summary>
        protected virtual void OnOffsetChild(View child, int position, bool flowDown, int childrenLeft, int childTop)
		{
			child.OffsetLeftAndRight(childrenLeft - child.Left);
			child.OffsetTopAndBottom(childTop - child.Top);
		}

		/// <summary>
		/// Override to set you custom listviews child to a specific left location
		/// </summary>
		/// <returns> the left location to layout the child for the given position </returns>
        protected virtual int GetChildLeft(int position)
		{
			return ListPaddingLeft;
		}

		/// <summary>
		/// Override to set you custom listviews child to a specific top location
		/// </summary>
		/// <returns> the top location to layout the child for the given position </returns>
        protected virtual int GetChildTop(int position)
		{
			int count = ChildCount;
			int paddingTop = 0;
			if (mClipToPadding)
			{
				paddingTop = ListPaddingTop;
			}
			return count > 0 ? GetChildAt(count - 1).Bottom : paddingTop;
		}

		/// <summary>
		/// Override to set you custom listviews child to a bottom top location
		/// </summary>
		/// <returns> the bottom location to layout the child for the given position </returns>
		protected virtual int GetChildBottom(int position)
		{
			int count = ChildCount;
			int paddingBottom = 0;
			if (mClipToPadding)
			{
				paddingBottom = ListPaddingBottom;
			}
			return count > 0 ? GetChildAt(0).Top : Height - paddingBottom;
		}

		protected virtual int GetNextChildDownsTop(int position)
		{
			int count = ChildCount;
			return count > 0 ? GetChildAt(count - 1).Bottom : 0;
		}

        protected virtual int GetNextChildUpsBottom(int position)
		{
			int count = ChildCount;
			if (count == 0)
			{
				return 0;
			}
			return count > 0 ? GetChildAt(0).Top : 0;
		}

        protected virtual int FirstChildTop
		{
			get
			{
				return HasChildren() ? GetChildAt(0).Top : 0;
			}
		}

        protected virtual int HighestChildTop
		{
			get
			{
				return HasChildren() ? GetChildAt(0).Top : 0;
			}
		}

		protected virtual int LastChildBottom
		{
			get
			{
				return HasChildren() ? GetChildAt(ChildCount - 1).Bottom : 0;
			}
		}

        protected virtual int LowestChildBottom
		{
			get
			{
				return HasChildren() ? GetChildAt(ChildCount - 1).Bottom : 0;
			}
		}

		protected bool HasChildren()
		{
			return ChildCount > 0;
		}

		protected virtual void OffsetChildrenTopAndBottom(int offset)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "offsetChildrenTopAndBottom: " + offset);
			}
			int count = ChildCount;
			for (int i = 0; i < count; i++)
			{
				View v = GetChildAt(i);
				v.OffsetTopAndBottom(offset);
			}
		}

		public override int FirstVisiblePosition
		{
			get
			{
				return System.Math.Max(0, mFirstPosition - HeaderViewsCount);
			}
		}

		public override int LastVisiblePosition
		{
			get
			{
				return System.Math.Min(mFirstPosition + ChildCount - 1, mAdapter != null ? mAdapter.Count - 1 : 0);
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// FLING
		//

		private void InitOrResetVelocityTracker()
		{
			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
			else
			{
				mVelocityTracker.Clear();
			}
		}

		private void InitVelocityTrackerIfNotExists()
		{
			if (mVelocityTracker == null)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}
		}

		private void RecycleVelocityTracker()
		{
			if (mVelocityTracker != null)
			{
				mVelocityTracker.Recycle();
				mVelocityTracker = null;
			}
		}

		private void StartFlingRunnable(float velocity)
		{
			if (mFlingRunnable == null)
			{
				mFlingRunnable = new FlingRunnable(this);
			}
			mFlingRunnable.Start((int) - velocity);
		}

		private void StopFlingRunnable()
		{
			if (mFlingRunnable != null)
			{
				mFlingRunnable.EndFling();
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// FLING RUNNABLE
		//

		/// <summary>
		/// Responsible for fling behavior. Use <seealso cref="#start(int)"/> to
		/// initiate a fling. Each frame of the fling is handled in <seealso cref="#run()"/>.
		/// A FlingRunnable will keep re-posting itself until the fling is done.
		/// </summary>
		private class FlingRunnable : Java.Lang.Object, IRunnable
		{
			private readonly ExtendableListView listview;

			/// <summary>
			/// Tracks the decay of a fling scroll
			/// </summary>
			public Scroller mScroller;

			/// <summary>
			/// Y value reported by mScroller on the previous fling
			/// </summary>
            public int mLastFlingY;

            public FlingRunnable(ExtendableListView listview)
			{
                this.listview = listview;
                mScroller = new Scroller(listview.Context);
			}

            public void Start(int initialVelocity)
			{
				int initialY = initialVelocity < 0 ? int.MaxValue : 0;
				mLastFlingY = initialY;
				mScroller.ForceFinished(true);
				mScroller.Fling(0, initialY, 0, initialVelocity, 0, int.MaxValue, 0, int.MaxValue);
				listview.mTouchMode = TOUCH_MODE_FLINGING;
				listview.PostOnAnimate(this);
			}

            public void StartScroll(int distance, int duration)
			{
				int initialY = distance < 0 ? int.MaxValue : 0;
				mLastFlingY = initialY;
				mScroller.StartScroll(0, initialY, 0, distance, duration);
				listview.mTouchMode = TOUCH_MODE_FLINGING;
				listview.PostOnAnimate(this);
			}

            public void EndFling()
			{
				mLastFlingY = 0;
				listview.mTouchMode = TOUCH_MODE_IDLE;

				listview.ReportScrollStateChange(ScrollState.Idle);
                listview.RemoveCallbacks(this);

				mScroller.ForceFinished(true);
			}

			public void Run()
			{
				switch (listview.mTouchMode)
				{
					default:
						return;

					case TOUCH_MODE_FLINGING:
					{
                        if (listview.mItemCount == 0 || listview.ChildCount == 0)
						{
							EndFling();
							return;
						}

						Scroller scroller = mScroller;
						bool more = scroller.ComputeScrollOffset();
						int y = scroller.CurrY;

						// Flip sign to convert finger direction to list items direction
						// (e.g. finger moving down means list is moving towards the top)
						int delta = mLastFlingY - y;

						// Pretend that each frame of a fling scroll is a touch scroll
						if (delta > 0)
						{
							// List is moving towards the top. Use first view as mMotionPosition
							listview.mMotionPosition = listview.mFirstPosition;
							// Don't fling more than 1 screen
                            delta = System.Math.Min(listview.Height - listview.PaddingBottom - listview.PaddingTop - 1, delta);
						}
						else
						{
							// List is moving towards the bottom. Use last view as mMotionPosition
                            int offsetToLast = listview.ChildCount - 1;
							listview.mMotionPosition = listview.mFirstPosition + offsetToLast;

							// Don't fling more than 1 screen
                            delta = System.Math.Max(-(listview.Height - listview.PaddingBottom - listview.PaddingTop - 1), delta);
						}

						bool atEnd = listview.MoveTheChildren(delta, delta);

						if (more && !atEnd)
						{
                            listview.Invalidate();
							mLastFlingY = y;
							listview.PostOnAnimate(this);
						}
						else
						{
							EndFling();
						}
						break;
					}
				}
			}
		}

		private void PostOnAnimate(IRunnable runnable)
		{
			ViewCompat.PostOnAnimation(this, runnable);
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// SCROLL LISTENER
		//

		/// <summary>
		/// Notify any scroll listeners of our current touch mode
		/// </summary>
		public void NotifyTouchMode()
		{
			// only tell the scroll listener about some things we want it to know
			switch (mTouchMode)
			{
				case TOUCH_MODE_SCROLLING:
					ReportScrollStateChange(ScrollState.TouchScroll);
					break;
				case TOUCH_MODE_FLINGING:
                    ReportScrollStateChange(ScrollState.Fling);
					break;
				case TOUCH_MODE_IDLE:
                    ReportScrollStateChange(ScrollState.Idle);
					break;
			}
		}

		private IOnScrollListener mOnScrollListener;

		public IOnScrollListener OnScrollListener
		{
			set
			{
				base.SetOnScrollListener(value);
				mOnScrollListener = value;
			}
		}

		public void ReportScrollStateChange(ScrollState newState)
		{
			if (newState != mScrollState)
			{
				mScrollState = newState;
				if (mOnScrollListener != null)
				{
					mOnScrollListener.OnScrollStateChanged(this, newState);
				}
			}
		}

		public void InvokeOnItemScrollListener()
		{
			if (mOnScrollListener != null)
			{
				mOnScrollListener.OnScroll(this, mFirstPosition, ChildCount, mItemCount);
			}
		}

		/// <summary>
		/// Update the status of the list based on the empty parameter.  If empty is true and
		/// we have an empty view, display it.  In all the other cases, make sure that the listview
		/// is VISIBLE and that the empty view is GONE (if it's not null).
		/// </summary>
		private void UpdateEmptyStatus()
		{
			bool empty = Adapter == null || Adapter.IsEmpty;
			if (IsInFilterMode)
			{
				empty = false;
			}

			View emptyView = EmptyView;
			if (empty)
			{
				if (emptyView != null)
				{
                    emptyView.Visibility = ViewStates.Visible;
					Visibility = ViewStates.Gone;
				}
				else
				{
					// If the caller just removed our empty view, make sure the list view is visible
                    Visibility = ViewStates.Visible;
				}

				// We are now GONE, so pending layouts will not be dispatched.
				// Force one here to make sure that the state of the list matches
				// the state of the adapter.
				if (mDataChanged)
				{
					this.OnLayout(false, Left, Top, Right, Bottom);
				}
			}
			else
			{
				if (emptyView != null)
				{
                    emptyView.Visibility = ViewStates.Gone;
				}
                Visibility = ViewStates.Visible;
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// ADAPTER OBSERVER
		//

		public class AdapterDataSetObserver : DataSetObserver
		{
			private readonly ExtendableListView listview;

            public AdapterDataSetObserver(ExtendableListView listview)
			{
                this.listview = listview;
			}


            public IParcelable mInstanceState = null;

			public override void OnChanged()
			{
				listview.mDataChanged = true;
				listview.mOldItemCount = listview.mItemCount;
				listview.mItemCount = listview.Adapter.Count;

				listview.mRecycleBin.ClearTransientStateViews();

				// Detect the case where a cursor that was previously invalidated has
				// been repopulated with new data.
				if (listview.Adapter.HasStableIds && mInstanceState != null && listview.mOldItemCount == 0 && listview.mItemCount > 0)
				{
					listview.OnRestoreInstanceState(mInstanceState);
					mInstanceState = null;
				}
				else
				{
					listview.RememberSyncState();
				}

				listview.UpdateEmptyStatus();
				listview.RequestLayout();
			}

			public override void OnInvalidated()
			{
				listview.mDataChanged = true;

				if (listview.Adapter.HasStableIds)
				{
					// Remember the current state for the case where our hosting activity is being
					// stopped and later restarted
					mInstanceState = listview.OnSaveInstanceState();
				}

				// Data is invalid so we should reset our state
				listview.mOldItemCount = listview.mItemCount;
				listview.mItemCount = 0;
				listview.mNeedSync = false;

				listview.UpdateEmptyStatus();
				listview.RequestLayout();
			}

			public void ClearSavedState()
			{
				mInstanceState = null;
			}
		}


		// //////////////////////////////////////////////////////////////////////////////////////////
		// LAYOUT PARAMS
		//

		/// <summary>
		/// Re-implementing some properties in <seealso cref="android.view.ViewGroup.LayoutParams"/> since they're package
		/// private but we want to appear to be an extension of the existing class.
		/// </summary>
		public class LayoutParams : AbsListView.LayoutParams
		{

			public bool recycledHeaderFooter;

			// Position of the view in the data
            public int position;

			// adapter ID the view represents fetched from the adapter if it's stable
            public long itemId = -1;

			// adapter view type
            public int viewType;

			public LayoutParams(Context c, IAttributeSet attrs) : base(c, attrs)
			{
			}

			public LayoutParams(int w, int h) : base(w, h)
			{
			}

			public LayoutParams(int w, int h, int viewType) : base(w, h)
			{
				this.viewType = viewType;
			}

			public LayoutParams(ViewGroup.LayoutParams source) : base(source)
			{
			}

		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// RecycleBin
		//

		/// <summary>
		/// Note there's no RecyclerListener. The caller shouldn't have a need and we can add it later.
		/// </summary>
		public class RecycleBin
		{
			private readonly ExtendableListView listview;

            public RecycleBin(ExtendableListView listview)
			{
                this.listview = listview;
			}


			/// <summary>
			/// The position of the first view stored in mActiveViews.
			/// </summary>
			public int mFirstActivePosition;

			/// <summary>
			/// Views that were on screen at the start of layout. This array is populated at the start of
			/// layout, and at the end of layout all view in mActiveViews are moved to mScrapViews.
			/// Views in mActiveViews represent a contiguous range of Views, with position of the first
			/// view store in mFirstActivePosition.
			/// </summary>
            public View[] mActiveViews = new View[0];

			/// <summary>
			/// Unsorted views that can be used by the adapter as a convert view.
			/// </summary>
            public List<View>[] mScrapViews;

            public int mViewTypeCount;

            public List<View> mCurrentScrap;

            public List<View> mSkippedScrap;

            public SparseArrayCompat mTransientStateViews;

			public int ViewTypeCount
			{
				set
				{
					if (value < 1)
					{
						throw new System.ArgumentException("Can't have a viewTypeCount < 1");
					}
					//noinspection unchecked
                    List<View>[] scrapViews = new List<View>[value];
					for (int i = 0; i < value; i++)
					{
						scrapViews[i] = new List<View>();
					}
					mViewTypeCount = value;
					mCurrentScrap = scrapViews[0];
					mScrapViews = scrapViews;
				}
			}

			public void MarkChildrenDirty()
			{
				if (mViewTypeCount == 1)
				{
					List<View> scrap = mCurrentScrap;
					int scrapCount = scrap.Count;
					for (int i = 0; i < scrapCount; i++)
					{
						scrap[i].ForceLayout();
					}
				}
				else
				{
					int typeCount = mViewTypeCount;
					for (int i = 0; i < typeCount; i++)
					{
						List<View> scrap = mScrapViews[i];
						int scrapCount = scrap.Count;
						for (int j = 0; j < scrapCount; j++)
						{
							scrap[j].ForceLayout();
						}
					}
				}
				if (mTransientStateViews != null)
				{
					int count = mTransientStateViews.Size();
					for (int i = 0; i < count; i++)
					{
						((View)mTransientStateViews.ValueAt(i)).ForceLayout();
					}
				}
			}

			public bool ShouldRecycleViewType(int viewType)
			{
				return viewType >= 0;
			}

			/// <summary>
			/// Clears the scrap heap.
			/// </summary>
			public void Clear()
			{
				if (mViewTypeCount == 1)
				{
					List<View> scrap = mCurrentScrap;
					int scrapCount = scrap.Count;
					for (int i = 0; i < scrapCount; i++)
					{
                        View v = scrap.ElementAt(scrapCount - 1 - i);
                        scrap.Remove(v);
                        listview.RemoveDetachedView(v, false);
					}
				}
				else
				{
					int typeCount = mViewTypeCount;
					for (int i = 0; i < typeCount; i++)
					{
						List<View> scrap = mScrapViews[i];
						int scrapCount = scrap.Count;
						for (int j = 0; j < scrapCount; j++)
						{
                            View v = scrap.ElementAt(scrapCount - 1 - j);
                            listview.RemoveDetachedView(v, false);
                            scrap.Remove(v);
						}
					}
				}
				if (mTransientStateViews != null)
				{
					mTransientStateViews.Clear();
				}
			}

			/// <summary>
			/// Fill ActiveViews with all of the children of the AbsListView.
			/// </summary>
			/// <param name="childCount">          The minimum number of views mActiveViews should hold </param>
			/// <param name="firstActivePosition"> The position of the first view that will be stored in
			///                            mActiveViews </param>
			public void FillActiveViews(int childCount, int firstActivePosition)
			{
				if (mActiveViews.Length < childCount)
				{
					mActiveViews = new View[childCount];
				}
				mFirstActivePosition = firstActivePosition;

				View[] activeViews = mActiveViews;
				for (int i = 0; i < childCount; i++)
				{
                    View child = listview.GetChildAt(i);
					LayoutParams lp = (LayoutParams)child.LayoutParameters;
					// Don't put header or footer views into the scrap heap
					if (lp != null && lp.viewType != ItemViewTypeHeaderOrFooter)
					{
						// Note:  We do place AdapterView.ITEM_VIEW_TYPE_IGNORE in active views.
						//        However, we will NOT place them into scrap views.
						activeViews[i] = child;
					}
				}
			}

			/// <summary>
			/// Get the view corresponding to the specified position. The view will be removed from
			/// mActiveViews if it is found.
			/// </summary>
			/// <param name="position"> The position to look up in mActiveViews </param>
			/// <returns> The view if it is found, null otherwise </returns>
            public View GetActiveView(int position)
			{
				int index = position - mFirstActivePosition;
				View[] activeViews = mActiveViews;
				if (index >= 0 && index < activeViews.Length)
				{
					View match = activeViews[index];
					activeViews[index] = null;
					return match;
				}
				return null;
			}

            public View GetTransientStateView(int position)
			{
				if (mTransientStateViews == null)
				{
					return null;
				}
				int index = mTransientStateViews.IndexOfKey(position);
				if (index < 0)
				{
					return null;
				}
				View result = (View)mTransientStateViews.ValueAt(index);
				mTransientStateViews.RemoveAt(index);
				return result;
			}

			/// <summary>
			/// Dump any currently saved views with transient state.
			/// </summary>
			public void ClearTransientStateViews()
			{
				if (mTransientStateViews != null)
				{
					mTransientStateViews.Clear();
				}
			}

			/// <returns> A view from the ScrapViews collection. These are unordered. </returns>
            public View GetScrapView(int position)
			{
				if (mViewTypeCount == 1)
				{
					return RetrieveFromScrap(mCurrentScrap, position);
				}
				else
				{
					int whichScrap = listview.mAdapter.GetItemViewType(position);
					if (whichScrap >= 0 && whichScrap < mScrapViews.Length)
					{
						return RetrieveFromScrap(mScrapViews[whichScrap], position);
					}
				}
				return null;
			}

			/// <summary>
			/// Put a view into the ScrapViews list. These views are unordered.
			/// </summary>
			/// <param name="scrap"> The view to add </param>
			public void AddScrapView(View scrap, int position)
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "addScrapView position = " + position);
				}

				LayoutParams lp = (LayoutParams) scrap.LayoutParameters;
				if (lp == null)
				{
					return;
				}

				lp.position = position;

				// Don't put header or footer views or views that should be ignored
				// into the scrap heap
				int viewType = lp.viewType;
				bool scrapHasTransientState = ViewCompat.HasTransientState(scrap);
				if (!ShouldRecycleViewType(viewType) || scrapHasTransientState)
				{
					if (viewType != ItemViewTypeHeaderOrFooter || scrapHasTransientState)
					{
						if (mSkippedScrap == null)
						{
							mSkippedScrap = new List<View>();
						}
						mSkippedScrap.Add(scrap);
					}
					if (scrapHasTransientState)
					{
						if (mTransientStateViews == null)
						{
							mTransientStateViews = new SparseArrayCompat();
						}
						mTransientStateViews.Put(position, scrap);
					}
					return;
				}

				if (mViewTypeCount == 1)
				{
					mCurrentScrap.Add(scrap);
				}
				else
				{
					mScrapViews[viewType].Add(scrap);
				}
			}

			/// <summary>
			/// Finish the removal of any views that skipped the scrap heap.
			/// </summary>
			public void RemoveSkippedScrap()
			{
				if (mSkippedScrap == null)
				{
					return;
				}
				int count = mSkippedScrap.Count;
				for (int i = 0; i < count; i++)
				{
					listview.RemoveDetachedView(mSkippedScrap[i], false);
				}
				mSkippedScrap.Clear();
			}

			/// <summary>
			/// Move all views remaining in mActiveViews to mScrapViews.
			/// </summary>
			public void ScrapActiveViews()
			{
				View[] activeViews = mActiveViews;
				bool multipleScraps = mViewTypeCount > 1;

				List<View> scrapViews = mCurrentScrap;
				int count = activeViews.Length;
				for (int i = count - 1; i >= 0; i--)
				{
					View victim = activeViews[i];
					if (victim != null)
					{
						LayoutParams lp = (LayoutParams)victim.LayoutParameters;
						activeViews[i] = null;

						bool scrapHasTransientState = ViewCompat.HasTransientState(victim);
						int viewType = lp.viewType;

						if (!ShouldRecycleViewType(viewType) || scrapHasTransientState)
						{
							// Do not move views that should be ignored
							if (viewType != ItemViewTypeHeaderOrFooter || scrapHasTransientState)
							{
								listview.RemoveDetachedView(victim, false);
							}
							if (scrapHasTransientState)
							{
								if (mTransientStateViews == null)
								{
									mTransientStateViews = new SparseArrayCompat();
								}
								mTransientStateViews.Put(mFirstActivePosition + i, victim);
							}
							continue;
						}

						if (multipleScraps)
						{
							scrapViews = mScrapViews[viewType];
						}
						lp.position = mFirstActivePosition + i;
						scrapViews.Add(victim);
					}
				}

				PruneScrapViews();
			}

			/// <summary>
			/// Makes sure that the size of mScrapViews does not exceed the size of mActiveViews.
			/// (This can happen if an adapter does not recycle its views).
			/// </summary>
			public void PruneScrapViews()
			{
				int maxViews = mActiveViews.Length;
				int viewTypeCount = mViewTypeCount;
				List<View>[] scrapViews = mScrapViews;
				for (int i = 0; i < viewTypeCount; ++i)
				{
					List<View> scrapPile = scrapViews[i];
					int size = scrapPile.Count;
					int extras = size - maxViews;
					size--;
					for (int j = 0; j < extras; j++)
					{
                        View v = scrapPile.ElementAt(size--);
                        listview.RemoveDetachedView(v, false);
                        scrapPile.Remove(v);
					}
				}

				if (mTransientStateViews != null)
				{
					for (int i = 0; i < mTransientStateViews.Size(); i++)
					{
						View v = (View)mTransientStateViews.ValueAt(i);
						if (!ViewCompat.HasTransientState(v))
						{
							mTransientStateViews.RemoveAt(i);
							i--;
						}
					}
				}
			}

			/// <summary>
			/// Updates the cache color hint of all known views.
			/// </summary>
			/// <param name="color"> The new cache color hint. </param>
            public Color CacheColorHint
			{
				set
				{
					if (mViewTypeCount == 1)
					{
						List<View> scrap = mCurrentScrap;
						int scrapCount = scrap.Count;
						for (int i = 0; i < scrapCount; i++)
						{
							scrap[i].DrawingCacheBackgroundColor = value;
						}
					}
					else
					{
						int typeCount = mViewTypeCount;
						for (int i = 0; i < typeCount; i++)
						{
							List<View> scrap = mScrapViews[i];
							int scrapCount = scrap.Count;
							for (int j = 0; j < scrapCount; j++)
							{
								scrap[j].DrawingCacheBackgroundColor = value;
							}
						}
					}
					// Just in case this is called during a layout pass
					View[] activeViews = mActiveViews;
					int count = activeViews.Length;
					for (int i = 0; i < count; ++i)
					{
						View victim = activeViews[i];
						if (victim != null)
						{
							victim.DrawingCacheBackgroundColor = value;
						}
					}
				}
			}
		}

        public static View RetrieveFromScrap(List<View> scrapViews, int position)
		{
			int size = scrapViews.Count;
			if (size > 0)
			{
				// See if we still have a view for this position.
				for (int i = 0; i < size; i++)
				{
					View view = scrapViews[i];
					if (((LayoutParams) view.LayoutParameters).position == position)
					{
						scrapViews.RemoveAt(i);
						return view;
					}
				}

                View v = scrapViews.ElementAt(size - 1);
                scrapViews.Remove(v);

                return v;
			}
			else
			{
				return null;
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// OUR STATE
		//

		/// <summary>
		/// Position from which to start looking for mSyncRowId
		/// </summary>
		protected int mSyncPosition;

		/// <summary>
		/// The offset in pixels from the top of the AdapterView to the top
		/// of the view to select during the next layout.
		/// </summary>
		protected int mSpecificTop;

		/// <summary>
		/// Row id to look for when data has changed
		/// </summary>
        public long mSyncRowId = InvalidRowId;

		/// <summary>
		/// Height of the view when mSyncPosition and mSyncRowId where set
		/// </summary>
        public long mSyncHeight;

		/// <summary>
		/// True if we need to sync to mSyncRowId
		/// </summary>
        public bool mNeedSync = false;


		private ListSavedState mSyncState;


		/// <summary>
		/// Remember enough information to restore the screen state when the data has
		/// changed.
		/// </summary>
		public void RememberSyncState()
		{
			if (ChildCount > 0)
			{
				mNeedSync = true;
				mSyncHeight = Height;
				// Sync the based on the offset of the first view
				View v = GetChildAt(0);
				IListAdapter adapter = Adapter;
				if (mFirstPosition >= 0 && mFirstPosition < adapter.Count)
				{
					mSyncRowId = adapter.GetItemId(mFirstPosition);
				}
				else
				{
					mSyncRowId = NoId;
				}
				if (v != null)
				{
					mSpecificTop = v.Top;
				}
				mSyncPosition = mFirstPosition;
			}
		}

		private void ClearState()
		{
			// cleanup headers and footers before removing the views
			ClearRecycledState(mHeaderViewInfos);
			ClearRecycledState(mFooterViewInfos);

			RemoveAllViewsInLayout();
			mFirstPosition = 0;
			mDataChanged = false;
			mRecycleBin.Clear();
			mNeedSync = false;
			mSyncState = null;
			mLayoutMode = LAYOUT_NORMAL;
			Invalidate();
		}

		private void ClearRecycledState(List<FixedViewInfo> infos)
		{
			if (infos == null)
			{
				return;
			}
			foreach (FixedViewInfo info in infos)
			{
				View child = info.view;
				ViewGroup.LayoutParams p = child.LayoutParameters;

				if (p is LayoutParams)
				{
					((LayoutParams) p).recycledHeaderFooter = false;
				}
			}
		}

		public class ListSavedState : ClassLoaderSavedState
		{
			public long SelectedId;
            public long FirstId;
            public int ViewTop;
            public int Position;
            public int Height;

			/// <summary>
			/// Constructor called from <seealso cref="android.widget.AbsListView#onSaveInstanceState()"/>
			/// </summary>
			public ListSavedState(IParcelable superState, AbsListView listview)
                : base(superState, listview.Class.ClassLoader)
			{
			}

			/// <summary>
			/// Constructor called from <seealso cref="#CREATOR"/>
			/// </summary>
			public ListSavedState(Parcel state) 
                : base(state)
			{
				SelectedId = state.ReadLong();
				FirstId = state.ReadLong();
				ViewTop = state.ReadInt();
				Position = state.ReadInt();
				Height = state.ReadInt();
			}

			public override void WriteToParcel(Parcel state, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(state, flags);

				state.WriteLong(SelectedId);
				state.WriteLong(FirstId);
				state.WriteInt(ViewTop);
				state.WriteInt(Position);
				state.WriteInt(Height);
			}

			public override string ToString()
			{
				return "ExtendableListView.ListSavedState{" + JavaSystem.IdentityHashCode(this).ToString("x") + " selectedId=" + SelectedId + " firstId=" + FirstId + " viewTop=" + ViewTop + " position=" + Position + " height=" + Height + "}";
			}

            [ExportField("CREATOR")]
            public static ListParcelableCreator ListInitializeCreator()
            {
                Console.WriteLine("ListParcelableCreator.ListInitializeCreator");
                return new ListParcelableCreator();
            }

            public class ListParcelableCreator : Java.Lang.Object, IParcelableCreator
			{
                public Java.Lang.Object CreateFromParcel(Parcel source)
				{
                    return new ListSavedState(source);
				}

                public Java.Lang.Object[] NewArray(int size)
				{
					return new ListSavedState[size];
				}
			}
		}


		public override IParcelable OnSaveInstanceState()
		{

			IParcelable superState = base.OnSaveInstanceState();
			ListSavedState ss = new ListSavedState(superState, this);

			if (mSyncState != null)
			{
				// Just keep what we last restored.
				ss.SelectedId = mSyncState.SelectedId;
				ss.FirstId = mSyncState.FirstId;
				ss.ViewTop = mSyncState.ViewTop;
				ss.Position = mSyncState.Position;
				ss.Height = mSyncState.Height;
				return ss;
			}

			bool haveChildren = ChildCount > 0 && mItemCount > 0;
			ss.SelectedId = SelectedItemId;
			ss.Height = Height;

			// TODO : sync selection when we handle it
			if (haveChildren && mFirstPosition > 0)
			{
				// Remember the position of the first child.
				// We only do this if we are not currently at the top of
				// the list, for two reasons:
				// (1) The list may be in the process of becoming empty, in
				// which case mItemCount may not be 0, but if we try to
				// ask for any information about position 0 we will crash.
				// (2) Being "at the top" seems like a special case, anyway,
				// and the user wouldn't expect to end up somewhere else when
				// they revisit the list even if its content has changed.
				View v = GetChildAt(0);
				ss.ViewTop = v.Top;
				int firstPos = mFirstPosition;
				if (firstPos >= mItemCount)
				{
					firstPos = mItemCount - 1;
				}
				ss.Position = firstPos;
				ss.FirstId = mAdapter.GetItemId(firstPos);
			}
			else
			{
				ss.ViewTop = 0;
				ss.FirstId = InvalidPosition;
				ss.Position = 0;
			}

			return ss;
		}

		public override void OnRestoreInstanceState(IParcelable state)
		{
			ListSavedState ss = (ListSavedState)state;
			base.OnRestoreInstanceState(ss.SuperState);
			mDataChanged = true;

			mSyncHeight = ss.Height;

			if (ss.FirstId >= 0)
			{
				mNeedSync = true;
				mSyncState = ss;
				mSyncRowId = ss.FirstId;
				mSyncPosition = ss.Position;
				mSpecificTop = ss.ViewTop;
			}
			RequestLayout();
		}

		private class PerformClick : WindowRunnnable, IRunnable
		{
			private readonly ExtendableListView listview;

            public PerformClick(ExtendableListView listview)
                : base(listview)
			{
                this.listview = listview;
			}

            public int mClickMotionPosition;

			public void Run()
			{
				if (listview.mDataChanged)
				{
					return;
				}

				IListAdapter adapter = listview.mAdapter;
				int motionPosition = mClickMotionPosition;
				if (adapter != null && listview.mItemCount > 0 && motionPosition != InvalidPosition && motionPosition < adapter.Count && SameWindow())
				{
                    View view = listview.GetChildAt(motionPosition); // a fix by @pboos

					if (view != null)
					{
						int clickPosition = motionPosition + listview.mFirstPosition;
                        listview.PerformItemClick(view, clickPosition, adapter.GetItemId(clickPosition));
					}
				}
			}
		}

		private bool PerformLongPress(View child, int longPressPosition, long longPressId)
		{
			bool handled = false;

			IOnItemLongClickListener onItemLongClickListener = OnItemLongClickListener;
			if (onItemLongClickListener != null)
			{
				handled = onItemLongClickListener.OnItemLongClick(this, child, longPressPosition, longPressId);
			}
	//        if (!handled) {
	//            mContextMenuInfo = createContextMenuInfo(child, longPressPosition, longPressId);
	//            handled = super.showContextMenuForChild(AbsListView.this);
	//        }
			if (handled)
			{
				PerformHapticFeedback(FeedbackConstants.LongPress);
			}
			return handled;
		}

		/// <summary>
		/// A base class for Runnables that will check that their view is still attached to
		/// the original window as when the Runnable was created.
		/// </summary>
		private class WindowRunnnable : Java.Lang.Object
		{
			private ExtendableListView listview;

            public WindowRunnnable(ExtendableListView listview)
			{
                this.listview = listview;
			}

			public int mOriginalAttachCount;

			public void RememberWindowAttachCount()
			{
                mOriginalAttachCount = listview.WindowAttachCount;
			}

			public bool SameWindow()
			{
                return listview.HasWindowFocus && listview.WindowAttachCount == mOriginalAttachCount;
			}
		}
	}
}