using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using System;
using System.Collections.Generic;
using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// This class is used to initiate a drag within a view or across multiple views.
	/// When a drag starts it creates a special view (a DragView) that moves around the screen
	/// until the user ends the drag. As feedback to the user, this object causes the device to
	/// vibrate as the drag begins.
	/// 
	/// </summary>
	public class DragController
	{
		private const string TAG = "DragController";
		/// <summary>
		/// Indicates the drag is a move. </summary>
		public static int DRAG_ACTION_MOVE = 0;

		/// <summary>
		/// Indicates the drag is a copy. </summary>
		public static int DRAG_ACTION_COPY = 1;

		private const int VIBRATE_DURATION = 35;

		private const bool PROFILE_DRAWING_DURING_DRAG = false;

		private Context mContext;
		//private Vibrator mVibrator;

		// temporaries to avoid gc thrash
		private Rect mRectTemp = new Rect();
		private readonly int[] mCoordinatesTemp = new int[2];

		/// <summary>
		/// Whether or not we're dragging. </summary>
		private bool mDragging;

		/// <summary>
		/// X coordinate of the down event. </summary>
		private float mMotionDownX;

		/// <summary>
		/// Y coordinate of the down event. </summary>
		private float mMotionDownY;

		/// <summary>
		/// Info about the screen for clamping. </summary>
		private DisplayMetrics mDisplayMetrics = new DisplayMetrics();

		/// <summary>
		/// Original view that is being dragged. </summary>
		private View mOriginator;

		/// <summary>
		/// X offset from the upper-left corner of the cell to where we touched. </summary>
		private float mTouchOffsetX;

		/// <summary>
		/// Y offset from the upper-left corner of the cell to where we touched. </summary>
		private float mTouchOffsetY;

		/// <summary>
		/// Where the drag originated </summary>
		private IDragSource mDragSource;

		/// <summary>
		/// The data associated with the object being dragged </summary>
		private object mDragInfo;

		/// <summary>
		/// The view that moves around while you drag. </summary>
		private DragView mDragView;

		/// <summary>
		/// Who can receive drop events </summary>
        private List<IDropTarget> mDropTargets = new List<IDropTarget>();

		private IDragListener mListener;

		/// <summary>
		/// The window token used as the parent for the DragView. </summary>
		private IBinder mWindowToken;

		private View mMoveTarget;

		private IDropTarget mLastDropTarget;

		private InputMethodManager mInputMethodManager;

		/// <summary>
		/// Used to create a new DragLayer from XML.
		/// </summary>
		/// <param name="context"> The application's context. </param>
		public DragController(Context context)
		{
			mContext = context;
			//mVibrator = (Vibrator) context.getSystemService(Context.VIBRATOR_SERVICE);

		}

		/// <summary>
		/// Used to notify the on drag event
		/// 
		/// </summary>
		public void OnDrag()
		{
			if (mListener != null)
			{
				mListener.OnDrag();
			}
		}

		/// <summary>
		/// Starts a drag. 
		/// It creates a bitmap of the view being dragged. That bitmap is what you see moving.
		/// The actual view can be repositioned if that is what the onDrop handle chooses to do.
		/// </summary>
		/// <param name="v"> The view that is being dragged </param>
		/// <param name="source"> An object representing where the drag originated </param>
		/// <param name="dragInfo"> The data associated with the object that is being dragged </param>
		/// <param name="dragAction"> The drag action: either <seealso cref="#DRAG_ACTION_MOVE"/> or
		///        <seealso cref="#DRAG_ACTION_COPY"/> </param>
		public void StartDrag(View v, IDragSource source, object dragInfo, int dragAction)
		{
			// Start dragging, but only if the source has something to drag.
			bool doDrag = source.AllowDrag();
			if (!doDrag)
			{
				return;
			}

			mOriginator = v;

			Bitmap b = GetViewBitmap(v);

			if (b == null)
			{
				// out of memory?
				return;
			}

			int[] loc = mCoordinatesTemp;
			v.GetLocationOnScreen(loc);
			int screenX = loc[0];
			int screenY = loc[1];

			StartDrag(b, screenX, screenY, 0, 0, b.Width, b.Height, source, dragInfo, dragAction);

			b.Recycle();

			if (dragAction == DRAG_ACTION_MOVE)
			{
				v.Visibility = ViewStates.Gone;
			}
		}

		/// <summary>
		/// Starts a drag.
		/// </summary>
		/// <param name="b"> The bitmap to display as the drag image.  It will be re-scaled to the
		///          enlarged size. </param>
		/// <param name="screenX"> The x position on screen of the left-top of the bitmap. </param>
		/// <param name="screenY"> The y position on screen of the left-top of the bitmap. </param>
		/// <param name="textureLeft"> The left edge of the region inside b to use. </param>
		/// <param name="textureTop"> The top edge of the region inside b to use. </param>
		/// <param name="textureWidth"> The width of the region inside b to use. </param>
		/// <param name="textureHeight"> The height of the region inside b to use. </param>
		/// <param name="source"> An object representing where the drag originated </param>
		/// <param name="dragInfo"> The data associated with the object that is being dragged </param>
		/// <param name="dragAction"> The drag action: either <seealso cref="#DRAG_ACTION_MOVE"/> or
		///        <seealso cref="#DRAG_ACTION_COPY"/> </param>
		public void StartDrag(Bitmap b, int screenX, int screenY, int textureLeft, int textureTop, int textureWidth, int textureHeight, IDragSource source, object dragInfo, int dragAction)
		{
			if (PROFILE_DRAWING_DURING_DRAG)
			{
				Debug.StartMethodTracing("Launcher");
			}

			// Hide soft keyboard, if visible
			if (mInputMethodManager == null)
			{
				mInputMethodManager = (InputMethodManager)mContext.GetSystemService(Context.InputMethodService);
			}
			mInputMethodManager.HideSoftInputFromWindow(mWindowToken, 0);

			if (mListener != null)
			{
				mListener.OnDragStart(source, dragInfo, dragAction);
			}

			int registrationX = ((int)mMotionDownX) - screenX;
			int registrationY = ((int)mMotionDownY) - screenY;

			mTouchOffsetX = mMotionDownX - screenX;
			mTouchOffsetY = mMotionDownY - screenY;

			mDragging = true;
			mDragSource = source;
			mDragInfo = dragInfo;

			//mVibrator.vibrate(VIBRATE_DURATION);
			DragView dragView = mDragView = new DragView(mContext, b, registrationX, registrationY, textureLeft, textureTop, textureWidth, textureHeight);
			dragView.Show(mWindowToken, (int)mMotionDownX, (int)mMotionDownY);
		}

		/// <summary>
		/// Draw the view into a bitmap.
		/// </summary>
		private Bitmap GetViewBitmap(View v)
		{
			v.ClearFocus();
			v.Pressed = false;

			bool willNotCache = v.WillNotCacheDrawing();
			v.SetWillNotCacheDrawing(false);

			// Reset the drawing cache background color to fully transparent
			// for the duration of this operation
			Color color = v.DrawingCacheBackgroundColor;
			v.DrawingCacheBackgroundColor = Color.Transparent;

			if (color != Color.Transparent)
			{
				v.DestroyDrawingCache();
			}
			v.BuildDrawingCache();
			Bitmap cacheBitmap = v.DrawingCache;
			if (cacheBitmap == null)
			{
				Console.WriteLine(TAG + " failed getViewBitmap(" + v + ")", new System.Exception());
				return null;
			}

			Bitmap bitmap = Bitmap.CreateBitmap(cacheBitmap);

			// Restore the view
			v.DestroyDrawingCache();
			v.SetWillNotCacheDrawing(willNotCache);
			v.DrawingCacheBackgroundColor = color;

			return bitmap;
		}

		/// <summary>
		/// Call this from a drag source view like this:
		/// 
		/// <pre>
		///  @Override
		///  public boolean dispatchKeyEvent(KeyEvent event) {
		///      return mDragController.dispatchKeyEvent(this, event)
		///              || super.dispatchKeyEvent(event);
		/// </pre>
		/// </summary>
		public bool DispatchKeyEvent(KeyEvent ev)
		{
			return mDragging;
		}

		/// <summary>
		/// Stop dragging without dropping.
		/// </summary>
		public void CancelDrag()
		{
			EndDrag();
		}

		private void EndDrag()
		{
			if (mDragging)
			{
				mDragging = false;
				if (mOriginator != null)
				{
					mOriginator.Visibility = ViewStates.Invisible;
				}
				if (mListener != null)
				{
					mListener.OnDragEnd();
				}
				if (mDragView != null)
				{
					mDragView.Remove();
					mDragView = null;
				}
			}
		}

		/// <summary>
		/// Call this from a drag source view.
		/// </summary>
		public bool OnInterceptTouchEvent(MotionEvent ev)
		{
			MotionEventActions action = ev.Action;

            if (action == MotionEventActions.Down)
			{
				RecordScreenSize();
			}

			float screenX = Clamp((int)ev.RawX, 0, mDisplayMetrics.WidthPixels);
			float screenY = Clamp((int)ev.RawY, 0, mDisplayMetrics.HeightPixels);

			switch (action)
			{
                case MotionEventActions.Move:
					break;

                case MotionEventActions.Down:
					// Remember location of down touch
					mMotionDownX = screenX;
					mMotionDownY = screenY;
					mLastDropTarget = null;
					break;

                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
					if (mDragging)
					{
						Drop(screenX, screenY);
					}
					EndDrag();
					break;
			}

			return mDragging;
		}

		/// <summary>
		/// Sets the view that should handle move events.
		/// </summary>
		internal View MoveTarget
		{
			set
			{
				mMoveTarget = value;
			}
		}

		public bool DispatchUnhandledMove(View focused, FocusSearchDirection direction)
		{
			return mMoveTarget != null && mMoveTarget.DispatchUnhandledMove(focused, direction);
		}

		/// <summary>
		/// Call this from a drag source view.
		/// </summary>
		public bool OnTouchEvent(MotionEvent ev)
		{
			if (!mDragging)
			{
				return false;
			}

            MotionEventActions action = ev.Action;
			float screenX = Clamp((int)ev.RawX, 0, mDisplayMetrics.WidthPixels);
			float screenY = Clamp((int)ev.RawY, 0, mDisplayMetrics.HeightPixels);

			switch (action)
			{
                case MotionEventActions.Down:
				// Remember where the motion event started
				mMotionDownX = screenX;
				mMotionDownY = screenY;
				break;
                case MotionEventActions.Move:
				// Update the drag view.  Don't use the clamped pos here so the dragging looks
				// like it goes off screen a little, intead of bumping up against the edge.
				mDragView.Move((int)ev.RawX, (int)ev.RawY);
				// Drop on someone?
				int[] coordinates = mCoordinatesTemp;
				IDropTarget dropTarget = findDropTarget(screenX, screenY, coordinates);
				if (dropTarget != null)
				{
					if (mLastDropTarget == dropTarget)
					{
						dropTarget.OnDragOver(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
					}
					else
					{
						if (mLastDropTarget != null)
						{
							mLastDropTarget.OnDragExit(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
						}
						dropTarget.OnDragEnter(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
					}
				}
				else
				{
					if (mLastDropTarget != null)
					{
						mLastDropTarget.OnDragExit(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
					}
				}
				mLastDropTarget = dropTarget;

				/* The original Launcher activity supports a delete region and scrolling.
				   It is not needed in this example.
				
				// Scroll, maybe, but not if we're in the delete region.
				boolean inDeleteRegion = false;
				if (mDeleteRegion != null) {
				    inDeleteRegion = mDeleteRegion.contains(screenX, screenY);
				}
				//Log.d(TAG, "inDeleteRegion=" + inDeleteRegion + " screenX=" + screenX
				//        + " mScrollZone=" + mScrollZone);
				if (!inDeleteRegion && screenX < mScrollZone) {
				    if (mScrollState == SCROLL_OUTSIDE_ZONE) {
				        mScrollState = SCROLL_WAITING_IN_ZONE;
				        mScrollRunnable.setDirection(SCROLL_LEFT);
				        mHandler.postDelayed(mScrollRunnable, SCROLL_DELAY);
				    }
				} else if (!inDeleteRegion && screenX > scrollView.getWidth() - mScrollZone) {
				    if (mScrollState == SCROLL_OUTSIDE_ZONE) {
				        mScrollState = SCROLL_WAITING_IN_ZONE;
				        mScrollRunnable.setDirection(SCROLL_RIGHT);
				        mHandler.postDelayed(mScrollRunnable, SCROLL_DELAY);
				    }
				} else {
				    if (mScrollState == SCROLL_WAITING_IN_ZONE) {
				        mScrollState = SCROLL_OUTSIDE_ZONE;
				        mScrollRunnable.setDirection(SCROLL_RIGHT);
				        mHandler.removeCallbacks(mScrollRunnable);
				    }
				}
				*/
				if (mDragSource != null && mDragging)
				{
					OnDrag();
				}
				break;
                case MotionEventActions.Up:
				if (mDragging)
				{
					Drop(screenX, screenY);
				}
				EndDrag();

				break;
                case MotionEventActions.Cancel:
				CancelDrag();
			break;
			}

			return true;
		}

		private bool Drop(float x, float y)
		{
			int[] coordinates = mCoordinatesTemp;
			IDropTarget dropTarget = findDropTarget((int) x, (int) y, coordinates);

			if (dropTarget != null)
			{
				dropTarget.OnDragExit(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
				if (dropTarget.AcceptDrop(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo))
				{
					dropTarget.OnDrop(mDragSource, coordinates[0], coordinates[1], (int) mTouchOffsetX, (int) mTouchOffsetY, mDragView, mDragInfo);
					mDragSource.OnDropCompleted((View) dropTarget, true);
					return true;
				}
				else
				{
					mDragSource.OnDropCompleted((View) dropTarget, false);
					return true;
				}
			}
			return false;
		}

		private IDropTarget findDropTarget(float x, float y, int[] dropCoordinates)
		{
			Rect r = mRectTemp;

			List<IDropTarget> dropTargets = mDropTargets;
			int count = dropTargets.Count;
			for (int i = count - 1; i >= 0; i--)
			{
				IDropTarget target = dropTargets[i];
				target.GetHitRect(r);
				target.GetLocationOnScreen(dropCoordinates);
				r.Offset(dropCoordinates[0] - target.Left, dropCoordinates[1] - target.Top);
				if (r.Contains((int)x, (int)y))
				{
					dropCoordinates[0] = (int) x - dropCoordinates[0];
					dropCoordinates[1] = (int) y - dropCoordinates[1];
					return target;
				}
			}
			return null;
		}

		/// <summary>
		/// Get the screen size so we can clamp events to the screen size so even if
		/// you drag off the edge of the screen, we find something.
		/// </summary>
		private void RecordScreenSize()
		{
            ((IWindowManager)mContext.GetSystemService(Context.WindowService)).JavaCast<IWindowManager>().DefaultDisplay.GetMetrics(mDisplayMetrics);
		}

		/// <summary>
		/// Clamp val to be &gt;= min and &lt; max.
		/// </summary>
		private static float Clamp(float val, float min, float max)
		{
			if (val < min)
			{
				return min;
			}
			else if (val >= max)
			{
				return max - 1;
			}
			else
			{
				return val;
			}
		}

		public IBinder WindowToken
		{
			set
			{
				mWindowToken = value;
			}
		}

		/// <summary>
		/// Sets the drag listner which will be notified when a drag starts or ends.
		/// </summary>
		public IDragListener DragListener
		{
			set
			{
				mListener = value;
			}
		}

		/// <summary>
		/// Remove a previously installed drag listener.
		/// </summary>
		public void RemoveDragListener(IDragListener l)
		{
			mListener = null;
		}

		/// <summary>
		/// Add a DropTarget to the list of potential places to receive drop events.
		/// </summary>
		public void AddDropTarget(IDropTarget target)
		{
			mDropTargets.Add(target);
		}

		/// <summary>
		/// Don't send drop events to <em>target</em> any more.
		/// </summary>
		public void RemoveDropTarget(IDropTarget target)
		{
			mDropTargets.Remove(target);
		}
	}
}