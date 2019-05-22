using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using System;

namespace BTAndroidWebViewSelection
{
    /// <summary>
    /// Listener for item click
    /// 
    /// </summary>
    public interface IOnActionItemClickListener
    {
        void OnItemClick(QuickAction source, int pos, int actionId);
    }

    /// <summary>
    /// Listener for window dismiss
    /// 
    /// </summary>
    public interface IOnDismissListener
    {
        void OnDismiss();
    }

	/// <summary>
	/// QuickAction dialog, shows action list as icon and text like the one in Gallery3D app. Currently supports vertical 
	/// and horizontal layout.
	/// </summary>
	public class QuickAction : PopupWindows, IOnDismissListener
	{
		private new View mRootView;
		private ImageView mArrowUp;
		private ImageView mArrowDown;
		private LayoutInflater mInflater;
		private ViewGroup mTrack;
		private ScrollView mScroller;
		private IOnActionItemClickListener mItemClickListener;
		private IOnDismissListener mDismissListener;

		private IList<ActionItem> actionItems = new List<ActionItem>();

		private bool mDidAction;

		private int mChildPos;
		private int mInsertPos;
		private int mAnimStyle;
		private int mOrientation;
		private int rootWidth = 0;

		public const int HORIZONTAL = 0;
		public const int VERTICAL = 1;

		public const int ANIM_GROW_FROM_LEFT = 1;
		public const int ANIM_GROW_FROM_RIGHT = 2;
		public const int ANIM_GROW_FROM_CENTER = 3;
		public const int ANIM_REFLECT = 4;
		public const int ANIM_AUTO = 5;

		/// <summary>
		/// Constructor for default vertical layout
		/// </summary>
		/// <param name="context">  Context </param>
		public QuickAction(Context context) 
            : this(context, VERTICAL)
		{
		}

		/// <summary>
		/// Constructor allowing orientation override
		/// </summary>
		/// <param name="context">    Context </param>
		/// <param name="orientation"> Layout orientation, can be vartical or horizontal </param>
		public QuickAction(Context context, int orientation)
            : base(context)
		{

			mOrientation = orientation;

			mInflater = (LayoutInflater) context.GetSystemService(Context.LayoutInflaterService);

			if (mOrientation == HORIZONTAL)
			{
				RootViewId = Resource.Layout.popup_horizontal;
			}
			else
			{
                RootViewId = Resource.Layout.popup_vertical;
			}

			mAnimStyle = ANIM_AUTO;
			mChildPos = 0;
		}

		/// <summary>
		/// Get action item at an index
		/// </summary>
		/// <param name="index">  Index of item (position from callback)
		/// </param>
		/// <returns>  Action Item at the position </returns>
		public ActionItem GetActionItem(int index)
		{
			return actionItems[index];
		}

		/// <summary>
		/// Set root view.
		/// </summary>
		/// <param name="id"> Layout resource id </param>
		public int RootViewId
		{
			set
			{
				mRootView = (ViewGroup) mInflater.Inflate(value, null);
				mTrack = (ViewGroup) mRootView.FindViewById(Resource.Id.tracks);
    
				mArrowDown = (ImageView) mRootView.FindViewById(Resource.Id.arrow_down);
				mArrowUp = (ImageView) mRootView.FindViewById(Resource.Id.arrow_up);

                mScroller = (ScrollView)mRootView.FindViewById(Resource.Id.scroller);
    
				//This was previously defined on show() method, moved here to prevent force close that occured
				//when tapping fastly on a view to show quickaction dialog.
				//Thanx to zammbi (github.com/zammbi)
                mRootView.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
    
				ContentView = mRootView;
			}
		}

		/// <summary>
		/// Set animation style
		/// </summary>
		/// <param name="mAnimStyle"> animation style, default is set to ANIM_AUTO </param>
		public int AnimStyle
		{
			set
			{
				this.mAnimStyle = value;
			}
		}

		/// <summary>
		/// Set listener for action item clicked.
		/// </summary>
		/// <param name="listener"> Listener </param>
		public IOnActionItemClickListener OnActionItemClickListener
		{
			set
			{
				mItemClickListener = value;
			}
		}

		/// <summary>
		/// Add action item
		/// </summary>
		/// <param name="action">  <seealso cref="ActionItem"/> </param>
		public void AddActionItem(ActionItem action)
		{
			actionItems.Add(action);

			string title = action.Title;
			Drawable icon = action.Icon;

			View container;

			if (mOrientation == HORIZONTAL)
			{
				container = mInflater.Inflate(Resource.Layout.action_item_horizontal, null);
			}
			else
			{
                container = mInflater.Inflate(Resource.Layout.action_item_vertical, null);
			}

            ImageView img = (ImageView)container.FindViewById(Resource.Id.iv_icon);
            TextView text = (TextView)container.FindViewById(Resource.Id.tv_title);

			if (icon != null)
			{
				img.SetImageDrawable(icon);
			}
			else
			{
				img.Visibility = ViewStates.Gone;
			}

			if (title != null)
			{
				text.Text = title;
			}
			else
			{
				text.Visibility = ViewStates.Gone;
			}

			int pos = mChildPos;
			int actionId = action.ActionId;

            container.Click += (object sender, EventArgs e) =>
                {
                    if (mItemClickListener != null)
                    {
                        mItemClickListener.OnItemClick(this, pos, actionId);
                    }

                    if (!GetActionItem(pos).Sticky)
                    {
                        mDidAction = true;

                        Dismiss();
                    }
                };

			// Since I removed focusable from the popup window,
			// I need to control the selection drawable here
            container.Touch += (object sender, View.TouchEventArgs e) =>
                {
                    View v = (View)sender;

                    if (e.Event.Action == MotionEventActions.Down)
                    {

                        v.SetBackgroundResource(Resource.Drawable.action_item_selected);
                    }
                    else if (e.Event.Action == MotionEventActions.Up || e.Event.Action == MotionEventActions.Cancel || e.Event.Action == MotionEventActions.Outside)
                    {
                        v.SetBackgroundResource(Android.Resource.Color.Transparent);
                    }
                };

			container.Focusable = true;
			container.Clickable = true;

			if (mOrientation == HORIZONTAL && mChildPos != 0)
			{
				View separator = mInflater.Inflate(Resource.Layout.horiz_separator, null);

                RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

				separator.LayoutParameters = lp;
				separator.SetPadding(5, 0, 5, 0);

				mTrack.AddView(separator, mInsertPos);

				mInsertPos++;
			}

			mTrack.AddView(container, mInsertPos);

			mChildPos++;
			mInsertPos++;
		}

		/// <summary>
		/// Shows the quick action menu using the given Rect as the anchor. </summary>
		/// <param name="parent"> </param>
		/// <param name="rect"> </param>
		public void Show(View parent, Rect rect)
		{
			PreShow();

			int xPos, yPos, arrowPos;

			mDidAction = false;

			int[] location = new int[2];
			parent.GetLocationOnScreen(location);

			int parentXPos = location[0];
			int parentYPos = location[1];

			Rect anchorRect = new Rect(parentXPos + rect.Left, parentYPos + rect.Top, parentXPos + rect.Left + rect.Width(), parentYPos + rect.Top + rect.Height());
			int width = anchorRect.Width();
			int height = anchorRect.Height();

			//mRootView.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));

            mRootView.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

			int rootHeight = mRootView.MeasuredHeight;

			if (rootWidth == 0)
			{
				rootWidth = mRootView.MeasuredWidth;
			}

			int screenWidth = mWindowManager.DefaultDisplay.Width;
			int screenHeight = mWindowManager.DefaultDisplay.Height;

			//automatically get X coord of popup (top left)
			if ((anchorRect.Left + parentXPos + rootWidth) > screenWidth)
			{
				xPos = anchorRect.Left - (rootWidth - width);
				xPos = (xPos < 0) ? 0 : xPos;


				arrowPos = anchorRect.CenterX() - xPos;

			}
			else
			{
				if (width > rootWidth)
				{
					xPos = anchorRect.CenterX() - (rootWidth / 2);
				}
				else
				{
					xPos = anchorRect.Left;
				}


				arrowPos = anchorRect.CenterX() - xPos;
			}


			int dyTop = anchorRect.Top;
			int dyBottom = screenHeight - anchorRect.Bottom;

			bool onTop = (dyTop > dyBottom) ? true : false;

			if (onTop)
			{
				if (rootHeight > dyTop)
				{
					yPos = 15;
					ViewGroup.LayoutParams lp = mScroller.LayoutParameters;
					lp.Height = dyTop - height;
				}
				else
				{
					yPos = anchorRect.Top - rootHeight;
				}
			}
			else
			{
				yPos = anchorRect.Bottom;

				if (rootHeight > dyBottom)
				{
                    ViewGroup.LayoutParams lp = mScroller.LayoutParameters;
					lp.Height = dyBottom;
				}
			}


			//showArrow(((onTop) ? R.id.arrow_down : R.id.arrow_up), arrowPos);

			// No arrows
            mArrowUp.Visibility = ViewStates.Invisible;
            mArrowDown.Visibility = ViewStates.Invisible;

			SetAnimationStyle(screenWidth, anchorRect.CenterX(), onTop);

			mWindow.ShowAtLocation(parent, GravityFlags.NoGravity, xPos, yPos);

		}

		/// <summary>
		/// Show quickaction popup. Popup is automatically positioned, on top or bottom of anchor view.
		/// 
		/// </summary>
		public void Show(View anchor)
		{
			PreShow();

			int xPos, yPos, arrowPos;

			mDidAction = false;

			int[] location = new int[2];

			anchor.GetLocationOnScreen(location);

			Rect anchorRect = new Rect(location[0], location[1], location[0] + anchor.Width, location[1] + anchor.Height);

			//mRootView.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));

            mRootView.Measure(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);

			int rootHeight = mRootView.MeasuredHeight;

			if (rootWidth == 0)
			{
				rootWidth = mRootView.MeasuredWidth;
			}

			int screenWidth = mWindowManager.DefaultDisplay.Width;
			int screenHeight = mWindowManager.DefaultDisplay.Height;

			//automatically get X coord of popup (top left)
			if ((anchorRect.Left + rootWidth) > screenWidth)
			{
				xPos = anchorRect.Left - (rootWidth - anchor.Width);
				xPos = (xPos < 0) ? 0 : xPos;

				arrowPos = anchorRect.CenterX() - xPos;

			}
			else
			{
				if (anchor.Width > rootWidth)
				{
					xPos = anchorRect.CenterX() - (rootWidth / 2);
				}
				else
				{
					xPos = anchorRect.Left;
				}

				arrowPos = anchorRect.CenterX() - xPos;
			}

			int dyTop = anchorRect.Top;
			int dyBottom = screenHeight - anchorRect.Bottom;

			bool onTop = (dyTop > dyBottom) ? true : false;

			if (onTop)
			{
				if (rootHeight > dyTop)
				{
					yPos = 15;
                    ViewGroup.LayoutParams lp = mScroller.LayoutParameters;
					lp.Height = dyTop - anchor.Height;
				}
				else
				{
					yPos = anchorRect.Top - rootHeight;
				}
			}
			else
			{
				yPos = anchorRect.Bottom;

				if (rootHeight > dyBottom)
				{
                    ViewGroup.LayoutParams lp = mScroller.LayoutParameters;
					lp.Height = dyBottom;
				}
			}

			ShowArrow(((onTop) ? Resource.Id.arrow_down : Resource.Id.arrow_up), arrowPos);

			SetAnimationStyle(screenWidth, anchorRect.CenterX(), onTop);

			mWindow.ShowAtLocation(anchor, GravityFlags.NoGravity, xPos, yPos);
		}

		/// <summary>
		/// Set animation style
		/// </summary>
		/// <param name="screenWidth"> screen width </param>
		/// <param name="requestedX"> distance from left edge </param>
		/// <param name="onTop"> flag to indicate where the popup should be displayed. Set TRUE if displayed on top of anchor view
		/// 		  and vice versa </param>
		private void SetAnimationStyle(int screenWidth, int requestedX, bool onTop)
		{
			int arrowPos = requestedX - mArrowUp.MeasuredWidth / 2;

			switch (mAnimStyle)
			{
			case ANIM_GROW_FROM_LEFT:
                    mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Left : Resource.Style.Animations_PopDownMenu_Left;
				break;

			case ANIM_GROW_FROM_RIGHT:
                mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Right : Resource.Style.Animations_PopDownMenu_Right;
				break;

			case ANIM_GROW_FROM_CENTER:
                mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Center : Resource.Style.Animations_PopDownMenu_Center;
			break;

			case ANIM_REFLECT:
            mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Reflect : Resource.Style.Animations_PopDownMenu_Reflect;
			break;

			case ANIM_AUTO:
				if (arrowPos <= screenWidth / 4)
				{
                    mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Left : Resource.Style.Animations_PopDownMenu_Left;
				}
				else if (arrowPos > screenWidth / 4 && arrowPos < 3 * (screenWidth / 4))
				{
                    mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Center : Resource.Style.Animations_PopDownMenu_Center;
				}
				else
				{
                    mWindow.AnimationStyle = (onTop) ? Resource.Style.Animations_PopUpMenu_Right : Resource.Style.Animations_PopDownMenu_Right;
				}

				break;
			}
		}

		/// <summary>
		/// Show arrow
		/// </summary>
		/// <param name="whichArrow"> arrow type resource id </param>
		/// <param name="requestedX"> distance from left screen </param>
		private void ShowArrow(int whichArrow, int requestedX)
		{
			View showArrow = (whichArrow == Resource.Id.arrow_up) ? mArrowUp : mArrowDown;
            View hideArrow = (whichArrow == Resource.Id.arrow_up) ? mArrowDown : mArrowUp;

			int arrowWidth = mArrowUp.MeasuredWidth;

			showArrow.Visibility = ViewStates.Visible;

			ViewGroup.MarginLayoutParams param = (ViewGroup.MarginLayoutParams)showArrow.LayoutParameters;

			param.LeftMargin = requestedX - arrowWidth / 2;

            hideArrow.Visibility = ViewStates.Invisible;
		}

		/// <summary>
		/// Set listener for window dismissed. This listener will only be fired if the quicakction dialog is dismissed
		/// by clicking outside the dialog or clicking on sticky item.
		/// </summary>
		public IOnDismissListener OnDismissListener
		{
			set
			{
				OnDismissListener = this;
    
				mDismissListener = value;
			}
		}

		public void OnDismiss()
		{
			if (!mDidAction && mDismissListener != null)
			{
				mDismissListener.OnDismiss();
			}
		}
	}
}