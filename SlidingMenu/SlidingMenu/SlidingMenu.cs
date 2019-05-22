using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;

using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;

using System;

using IOnPageChangeListener = SlidingMenuLibrary.CustomViewAbove.IOnPageChangeListener;

namespace SlidingMenuLibrary
{
	public class SlidingMenu : RelativeLayout
	{
		private static readonly string TAG = Java.Lang.Class.FromType(typeof(SlidingMenu)).Class.SimpleName;

		public const int SLIDING_WINDOW = 0;
		public const int SLIDING_CONTENT = 1;
		private bool mActionbarOverlay = false;

		/// <summary>
		/// Constant value for use with setTouchModeAbove(). Allows the SlidingMenu to be opened with a swipe
		/// gesture on the screen's margin
		/// </summary>
		public const int TOUCHMODE_MARGIN = 0;

		/// <summary>
		/// Constant value for use with setTouchModeAbove(). Allows the SlidingMenu to be opened with a swipe
		/// gesture anywhere on the screen
		/// </summary>
		public const int TOUCHMODE_FULLSCREEN = 1;

		/// <summary>
		/// Constant value for use with setTouchModeAbove(). Denies the SlidingMenu to be opened with a swipe
		/// gesture
		/// </summary>
		public const int TOUCHMODE_NONE = 2;

		/// <summary>
		/// Constant value for use with setMode(). Puts the menu to the left of the content.
		/// </summary>
		public const int LEFT = 0;

		/// <summary>
		/// Constant value for use with setMode(). Puts the menu to the right of the content.
		/// </summary>
		public const int RIGHT = 1;

		/// <summary>
		/// Constant value for use with setMode(). Puts menus to the left and right of the content.
		/// </summary>
		public const int LEFT_RIGHT = 2;

        private readonly CustomViewAbove mViewAbove;
        private readonly CustomViewBehind mViewBehind;
		private IOnOpenListener mOpenListener;
		private IOnOpenListener mSecondaryOpenListner;
		private IOnCloseListener mCloseListener;

		/// <summary>
		/// The listener interface for receiving onOpen events.
		/// The class that is interested in processing a onOpen
		/// event implements this interface, and the object created
		/// with that class is registered with a component using the
		/// component's <code>addOnOpenListener<code> method. When
		/// the onOpen event occurs, that object's appropriate
		/// method is invoked
		/// </summary>
		public interface IOnOpenListener
		{

			/// <summary>
			/// On open.
			/// </summary>
			void OnOpen();
		}

		/// <summary>
		/// The listener interface for receiving onOpened events.
		/// The class that is interested in processing a onOpened
		/// event implements this interface, and the object created
		/// with that class is registered with a component using the
		/// component's <code>addOnOpenedListener<code> method. When
		/// the onOpened event occurs, that object's appropriate
		/// method is invoked.
		/// </summary>
		/// <seealso cref= OnOpenedEvent </seealso>
		public interface IOnOpenedListener
		{

			/// <summary>
			/// On opened.
			/// </summary>
			void OnOpened();
		}

		/// <summary>
		/// The listener interface for receiving onClose events.
		/// The class that is interested in processing a onClose
		/// event implements this interface, and the object created
		/// with that class is registered with a component using the
		/// component's <code>addOnCloseListener<code> method. When
		/// the onClose event occurs, that object's appropriate
		/// method is invoked.
		/// </summary>
		/// <seealso cref= OnCloseEvent </seealso>
		public interface IOnCloseListener
		{

			/// <summary>
			/// On close.
			/// </summary>
			void OnClose();
		}

		/// <summary>
		/// The listener interface for receiving onClosed events.
		/// The class that is interested in processing a onClosed
		/// event implements this interface, and the object created
		/// with that class is registered with a component using the
		/// component's <code>addOnClosedListener<code> method. When
		/// the onClosed event occurs, that object's appropriate
		/// method is invoked.
		/// </summary>
		/// <seealso cref= OnClosedEvent </seealso>
		public interface IOnClosedListener
		{

			/// <summary>
			/// On closed.
			/// </summary>
			void OnClosed();
		}

		/// <summary>
		/// The Interface CanvasTransformer.
		/// </summary>
		public interface ICanvasTransformer
		{

			/// <summary>
			/// Transform canvas.
			/// </summary>
			/// <param name="canvas"> the canvas </param>
			/// <param name="percentOpen"> the percent open </param>
			void TransformCanvas(Canvas canvas, float percentOpen);
		}

		/// <summary>
		/// Instantiates a new SlidingMenu.
		/// </summary>
		/// <param name="context"> the associated Context </param>
		public SlidingMenu(Context context) 
            : this(context, null)
		{
		}

		/// <summary>
		/// Instantiates a new SlidingMenu and attach to Activity.
		/// </summary>
		/// <param name="activity"> the activity to attach slidingmenu </param>
		/// <param name="slideStyle"> the slidingmenu style </param>
		public SlidingMenu(Activity activity, int slideStyle) 
            : this(activity, null)
		{
			this.AttachToActivity(activity, slideStyle);
		}

		/// <summary>
		/// Instantiates a new SlidingMenu.
		/// </summary>
		/// <param name="context"> the associated Context </param>
		/// <param name="attrs"> the attrs </param>
		public SlidingMenu(Context context, IAttributeSet attrs) 
            : this(context, attrs, 0)
		{
		}

		/// <summary>
		/// Instantiates a new SlidingMenu.
		/// </summary>
		/// <param name="context"> the associated Context </param>
		/// <param name="attrs"> the attrs </param>
		/// <param name="defStyle"> the def style </param>
		public SlidingMenu(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
            LayoutParams behindParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			mViewBehind = new CustomViewBehind(context);
			AddView(mViewBehind, behindParams);
            LayoutParams aboveParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
			mViewAbove = new CustomViewAbove(context);
			AddView(mViewAbove, aboveParams);
			// register the CustomViewBehind with the CustomViewAbove
			mViewAbove.CustomViewBehind = mViewBehind;
			mViewBehind.CustomViewAbove = mViewAbove;
			mViewAbove.SetInternalPageChangeListener(new MyOnPageChangeListener(this));

			// now style everything!
			TypedArray ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.SlidingMenu);
			// set the above and behind views if defined in xml
			int mode = ta.GetInt(Resource.Styleable.SlidingMenu_mode, LEFT);
			Mode = mode;
			int viewAbove = ta.GetResourceId(Resource.Styleable.SlidingMenu_viewAbove, -1);
			if (viewAbove != -1)
			{
				SetContent(viewAbove);
			}
			else
			{
				SetContent(new FrameLayout(context));
			}
            int viewBehind = ta.GetResourceId(Resource.Styleable.SlidingMenu_viewBehind, -1);
			if (viewBehind != -1)
			{
				SetMenu(viewBehind);
			}
			else
			{
				SetMenu(new FrameLayout(context));
			}
            int touchModeAbove = ta.GetInt(Resource.Styleable.SlidingMenu_touchModeAbove, TOUCHMODE_MARGIN);
			TouchModeAbove = touchModeAbove;
            int touchModeBehind = ta.GetInt(Resource.Styleable.SlidingMenu_touchModeBehind, TOUCHMODE_MARGIN);
			TouchModeBehind = touchModeBehind;

            int offsetBehind = (int)ta.GetDimension(Resource.Styleable.SlidingMenu_behindOffset, -1);
            int widthBehind = (int)ta.GetDimension(Resource.Styleable.SlidingMenu_behindWidth, -1);
			if (offsetBehind != -1 && widthBehind != -1)
			{
				throw new IllegalStateException("Cannot set both behindOffset and behindWidth for a SlidingMenu");
			}
			else if (offsetBehind != -1)
			{
				BehindOffset = offsetBehind;
			}
			else if (widthBehind != -1)
			{
				BehindWidth = widthBehind;
			}
			else
			{
				BehindOffset = 0;
			}
            float scrollOffsetBehind = ta.GetFloat(Resource.Styleable.SlidingMenu_behindScrollScale, 0.33f);
			BehindScrollScale = scrollOffsetBehind;
			int shadowRes = ta.GetResourceId(Resource.Styleable.SlidingMenu_shadowDrawable, -1);
			if (shadowRes != -1)
			{
				ShadowDrawableResource = shadowRes;
			}
			int shadowWidth = (int)ta.GetDimension(Resource.Styleable.SlidingMenu_shadowWidth, 0);
			ShadowWidth = shadowWidth;
			bool fadeEnabled = ta.GetBoolean(Resource.Styleable.SlidingMenu_fadeEnabled, true);
			FadeEnabled = fadeEnabled;
			float fadeDeg = ta.GetFloat(Resource.Styleable.SlidingMenu_fadeDegree, 0.33f);
			FadeDegree = fadeDeg;
			bool selectorEnabled = ta.GetBoolean(Resource.Styleable.SlidingMenu_selectorEnabled, false);
			SelectorEnabled = selectorEnabled;
			int selectorRes = ta.GetResourceId(Resource.Styleable.SlidingMenu_selectorDrawable, -1);
			if (selectorRes != -1)
			{
				SelectorDrawable = selectorRes;
			}
			ta.Recycle();
		}

		private class MyOnPageChangeListener : IOnPageChangeListener
		{
			private readonly SlidingMenu menu;

            public static readonly int POSITION_OPEN = 0;
            public static readonly int POSITION_CLOSE = 1;
            public static readonly int POSITION_SECONDARY_OPEN = 2;

            public MyOnPageChangeListener(SlidingMenu menu)
			{
                this.menu = menu;
			}

			public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
			{
			}

			public void OnPageSelected(int position)
			{
				if (position == POSITION_OPEN && menu.mOpenListener != null)
				{
					menu.mOpenListener.OnOpen();
				}
				else if (position == POSITION_CLOSE && menu.mCloseListener != null)
				{
					menu.mCloseListener.OnClose();
				}
				else if (position == POSITION_SECONDARY_OPEN && menu.mSecondaryOpenListner != null)
				{
					menu.mSecondaryOpenListner.OnOpen();
				}
			}
		}

		/// <summary>
		/// Attaches the SlidingMenu to an entire Activity
		/// </summary>
		/// <param name="activity"> the Activity </param>
		/// <param name="slideStyle"> either SLIDING_CONTENT or SLIDING_WINDOW </param>
		public void AttachToActivity(Activity activity, int slideStyle)
		{
			AttachToActivity(activity, slideStyle, false);
		}

		/// <summary>
		/// Attaches the SlidingMenu to an entire Activity
		/// </summary>
		/// <param name="activity"> the Activity </param>
		/// <param name="slideStyle"> either SLIDING_CONTENT or SLIDING_WINDOW </param>
		/// <param name="actionbarOverlay"> whether or not the ActionBar is overlaid </param>
		public void AttachToActivity(Activity activity, int slideStyle, bool actionbarOverlay)
		{
			if (slideStyle != SLIDING_WINDOW && slideStyle != SLIDING_CONTENT)
			{
				throw new System.ArgumentException("slideStyle must be either SLIDING_WINDOW or SLIDING_CONTENT");
			}

			if (Parent != null)
			{
				throw new IllegalStateException("This SlidingMenu appears to already be attached");
			}

			// get the window background
			TypedArray a = activity.Theme.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.WindowBackground });
			int background = a.GetResourceId(0, 0);
			a.Recycle();

			switch (slideStyle)
			{
			    case SLIDING_WINDOW:
				    mActionbarOverlay = false;
				    ViewGroup decor = (ViewGroup) activity.Window.DecorView;
				    ViewGroup decorChild = (ViewGroup) decor.GetChildAt(0);
				    // save ActionBar themes that have transparent assets
				    decorChild.SetBackgroundResource(background);
				    decor.RemoveView(decorChild);
				    decor.AddView(this);
				    SetContent(decorChild);
				    break;
			    case SLIDING_CONTENT:
				    mActionbarOverlay = actionbarOverlay;
				    // take the above view out of
				    ViewGroup contentParent = (ViewGroup)activity.FindViewById(Android.Resource.Id.Content);
				    View content = contentParent.GetChildAt(0);
				    contentParent.RemoveView(content);
				    contentParent.AddView(this);
				    SetContent(content);
				    // save people from having transparent backgrounds
				    if (content.Background == null)
				    {
					    content.SetBackgroundResource(background);
				    }
				    break;
			}
		}

		/// <summary>
		/// Set the above view content from a layout resource. The resource will be inflated, adding all top-level views
		/// to the above view.
		/// </summary>
		/// <param name="res"> the new content </param>
		public void SetContent(int res)
		{
			SetContent(LayoutInflater.From(Context).Inflate(res, null));
		}

		/// <summary>
		/// Set the above view content to the given View.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
		public void SetContent(View view)
		{
            mViewAbove.Content = view;
            ShowContent();
		}

        /// <summary>
        /// Retrieves the current content.
        /// </summary>
        /// <returns>the current content</returns>
        public View GetContent()
        {
            return mViewAbove.Content;
        }


		/// <summary>
		/// Set the behind view (menu) content from a layout resource. The resource will be inflated, adding all top-level views
		/// to the behind view.
		/// </summary>
		/// <param name="res"> the new content </param>
		public void SetMenu(int res)
		{
			SetMenu(LayoutInflater.From(Context).Inflate(res, null));
		}

		/// <summary>
		/// Set the behind view (menu) content to the given View.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
		public void SetMenu(View v)
		{
            mViewBehind.Content = v;
		}

        /// <summary>
        /// Retrieves the current content.
        /// </summary>
        /// <returns>the current content</returns>
        public View GetMenu()
        {
            return mViewBehind.Content;
        }


		/// <summary>
		/// Set the secondary behind view (right menu) content from a layout resource. The resource will be inflated, adding all top-level views
		/// to the behind view.
		/// </summary>
		/// <param name="res"> the new content </param>
		public void SetSecondaryMenu(int res)
		{
            SetSecondaryMenu(LayoutInflater.From(Context).Inflate(res, null));
		}

		/// <summary>
		/// Set the secondary behind view (right menu) content to the given View.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
		public void SetSecondaryMenu(View v)
		{
    		mViewBehind.SecondaryContent = v;
		}

		public View GetSecondaryMenu()
        {
            return mViewBehind.SecondaryContent;
        }



		/// <summary>
		/// Sets the sliding enabled.
		/// </summary>
		/// <param name="b"> true to enable sliding, false to disable it. </param>
		public bool IsSlidingEnabled
		{
			set
			{
				mViewAbove.IsSlidingEnabled = value;
			}
			get
			{
				return mViewAbove.IsSlidingEnabled;
			}
		}


		/// <summary>
		/// Sets which side the SlidingMenu should appear on. </summary>
		/// <param name="mode"> must be either SlidingMenu.LEFT or SlidingMenu.RIGHT </param>
		public int Mode
		{
			set
			{
				if (value != LEFT && value != RIGHT && value != LEFT_RIGHT)
				{
					throw new IllegalStateException("SlidingMenu mode must be LEFT, RIGHT, or LEFT_RIGHT");
				}
				mViewBehind.Mode = value;
			}
			get
			{
				return mViewBehind.Mode;
			}
		}


		/// <summary>
		/// Sets whether or not the SlidingMenu is in static mode (i.e. nothing is moving and everything is showing)
		/// </summary>
		/// <param name="b"> true to set static mode, false to disable static mode. </param>
		public bool Static
		{
			set
			{
				if (value)
				{
					IsSlidingEnabled = false;
					mViewAbove.CustomViewBehind = null;
					mViewAbove.CurrentItem = 1;
					//			mViewBehind.setCurrentItem(0);	
				}
				else
				{
					mViewAbove.CurrentItem = 1;
					//			mViewBehind.setCurrentItem(1);
					mViewAbove.CustomViewBehind = mViewBehind;
					IsSlidingEnabled = true;
				}
			}
		}

		/// <summary>
		/// Opens the menu and shows the menu view.
		/// </summary>
		public void ShowMenu()
		{
			ShowMenu(true);
		}

		/// <summary>
		/// Opens the menu and shows the menu view.
		/// </summary>
		/// <param name="animate"> true to animate the transition, false to ignore animation </param>
		public void ShowMenu(bool animate)
		{
			mViewAbove.SetCurrentItem(0, animate);
		}

		/// <summary>
		/// Opens the menu and shows the secondary menu view. Will default to the regular menu
		/// if there is only one.
		/// </summary>
		public void ShowSecondaryMenu()
		{
			ShowSecondaryMenu(true);
		}

		/// <summary>
		/// Opens the menu and shows the secondary (right) menu view. Will default to the regular menu
		/// if there is only one.
		/// </summary>
		/// <param name="animate"> true to animate the transition, false to ignore animation </param>
		public void ShowSecondaryMenu(bool animate)
		{
			mViewAbove.SetCurrentItem(2, animate);
		}

		/// <summary>
		/// Closes the menu and shows the above view.
		/// </summary>
		public void ShowContent()
		{
			ShowContent(true);
		}

		/// <summary>
		/// Closes the menu and shows the above view.
		/// </summary>
		/// <param name="animate"> true to animate the transition, false to ignore animation </param>
		public void ShowContent(bool animate)
		{
			mViewAbove.SetCurrentItem(1, animate);
		}

		/// <summary>
		/// Toggle the SlidingMenu. If it is open, it will be closed, and vice versa.
		/// </summary>
		public void Toggle()
		{
			Toggle(true);
		}

		/// <summary>
		/// Toggle the SlidingMenu. If it is open, it will be closed, and vice versa.
		/// </summary>
		/// <param name="animate"> true to animate the transition, false to ignore animation </param>
		public void Toggle(bool animate)
		{
			if (IsMenuShowing)
			{
				ShowContent(animate);
			}
			else
			{
				ShowMenu(animate);
			}
		}

		/// <summary>
		/// Checks if is the behind view showing.
		/// </summary>
		/// <returns> Whether or not the behind view is showing </returns>
		public bool IsMenuShowing
		{
			get
			{
				return mViewAbove.CurrentItem == 0 || mViewAbove.CurrentItem == 2;
			}
		}

		/// <summary>
		/// Checks if is the behind view showing.
		/// </summary>
		/// <returns> Whether or not the behind view is showing </returns>
		public bool IsSecondaryMenuShowing
		{
			get
			{
				return mViewAbove.CurrentItem == 2;
			}
		}

		/// <summary>
		/// Gets the behind offset.
		/// </summary>
		/// <returns> The margin on the right of the screen that the behind view scrolls to </returns>
		public int BehindOffset
		{
			get
			{
				return ((RelativeLayout.LayoutParams)mViewBehind.LayoutParameters).RightMargin;
			}
			set
			{
				//		RelativeLayout.LayoutParams params = ((RelativeLayout.LayoutParams)mViewBehind.getLayoutParams());
				//		int bottom = params.bottomMargin;
				//		int top = params.topMargin;
				//		int left = params.leftMargin;
				//		params.setMargins(left, top, value, bottom);
				mViewBehind.WidthOffset = value;
			}
		}


		/// <summary>
		/// Sets the behind offset.
		/// </summary>
		/// <param name="resID"> The dimension resource id to be set as the behind offset.
		/// The menu, when open, will leave this width margin on the right of the screen. </param>
		public int BehindOffsetRes
		{
			set
			{
				int i = (int)Context.Resources.GetDimension(value);
				BehindOffset = i;
			}
		}

		/// <summary>
		/// Sets the above offset.
		/// </summary>
		/// <param name="i"> the new above offset, in pixels </param>
		public int AboveOffset
		{
			set
			{
				mViewAbove.AboveOffset = value;
			}
		}

		/// <summary>
		/// Sets the above offset.
		/// </summary>
		/// <param name="resID"> The dimension resource id to be set as the above offset. </param>
		public int AboveOffsetRes
		{
			set
			{
				int i = (int)Context.Resources.GetDimension(value);
				AboveOffset = i;
			}
		}

		/// <summary>
		/// Sets the behind width.
		/// </summary>
		/// <param name="i"> The width the Sliding Menu will open to, in pixels </param>
		public int BehindWidth
		{
			set
			{
				int width;
                var windowService = Context.GetSystemService(Context.WindowService);
                var windowManager = windowService.JavaCast<IWindowManager>();
                var display = windowManager.DefaultDisplay;
				try
				{
					Class cls = Java.Lang.Class.FromType(typeof(Display)).Class;
                    Class[] parameterTypes = new Class[] { Java.Lang.Class.FromType(typeof(Point)) };
					Point parameter = new Point();
					Method method = cls.GetMethod("getSize", parameterTypes);
					method.Invoke(display, parameter);
					width = parameter.X;
				}
				catch (System.Exception e)
				{
                    Console.WriteLine(e.Message);
					width = display.Width;
				}
				BehindOffset = width - value;
			}
		}

		/// <summary>
		/// Sets the behind width.
		/// </summary>
		/// <param name="res"> The dimension resource id to be set as the behind width offset.
		/// The menu, when open, will open this wide. </param>
		public int BehindWidthRes
		{
			set
			{
				int i = (int) Context.Resources.GetDimension(value);
				BehindWidth = i;
			}
		}

		/// <summary>
		/// Gets the behind scroll scale.
		/// </summary>
		/// <returns> The scale of the parallax scroll </returns>
		public float BehindScrollScale
		{
			get
			{
				return mViewBehind.ScrollScale;
			}
			set
			{
				if (value < 0 && value > 1)
				{
					throw new IllegalStateException("ScrollScale must be between 0 and 1");
				}
				mViewBehind.ScrollScale = value;
			}
		}

		/// <summary>
		/// Gets the touch mode margin threshold </summary>
		/// <returns> the touch mode margin threshold </returns>
		public int TouchmodeMarginThreshold
		{
			get
			{
				return mViewBehind.MarginThreshold;
			}
			set
			{
				mViewBehind.MarginThreshold = value;
			}
		}



		/// <summary>
		/// Sets the behind canvas transformer.
		/// </summary>
		/// <param name="t"> the new behind canvas transformer </param>
		public ICanvasTransformer BehindCanvasTransformer
		{
			set
			{
				mViewBehind.CanvasTransformer = value;
			}
		}

		/// <summary>
		/// Gets the touch mode above.
		/// </summary>
		/// <returns> the touch mode above </returns>
		public int TouchModeAbove
		{
			get
			{
				return mViewAbove.TouchMode;
			}
			set
			{
				if (value != TOUCHMODE_FULLSCREEN && value != TOUCHMODE_MARGIN && value != TOUCHMODE_NONE)
				{
					throw new IllegalStateException("TouchMode must be set to either" + "TOUCHMODE_FULLSCREEN or TOUCHMODE_MARGIN or TOUCHMODE_NONE.");
				}
				mViewAbove.TouchMode = value;
			}
		}


		/// <summary>
		/// Controls whether the SlidingMenu can be opened with a swipe gesture.
		/// Options are <seealso cref="#TOUCHMODE_MARGIN TOUCHMODE_MARGIN"/>, <seealso cref="#TOUCHMODE_FULLSCREEN TOUCHMODE_FULLSCREEN"/>,
		/// or <seealso cref="#TOUCHMODE_NONE TOUCHMODE_NONE"/>
		/// </summary>
		/// <param name="i"> the new touch mode </param>
		public int TouchModeBehind
		{
			set
			{
				if (value != TOUCHMODE_FULLSCREEN && value != TOUCHMODE_MARGIN && value != TOUCHMODE_NONE)
				{
					throw new IllegalStateException("TouchMode must be set to either" + "TOUCHMODE_FULLSCREEN or TOUCHMODE_MARGIN or TOUCHMODE_NONE.");
				}
				mViewBehind.TouchMode = value;
			}
		}

		/// <summary>
		/// Sets the shadow drawable.
		/// </summary>
		/// <param name="resId"> the resource ID of the new shadow drawable </param>
		public int ShadowDrawableResource
		{
            set
            {
                ShadowDrawable = Context.Resources.GetDrawable(value);
            }
		}

		/// <summary>
		/// Sets the shadow drawable.
		/// </summary>
		/// <param name="d"> the new shadow drawable </param>
        public Drawable ShadowDrawable
		{
            set
            {
                mViewBehind.ShadowDrawable = value;
            }
		}

		/// <summary>
		/// Sets the secondary (right) shadow drawable.
		/// </summary>
		/// <param name="resId"> the resource ID of the new shadow drawable </param>
        public int SecondaryShadowDrawableResource
        {
            set
            {
                SecondaryShadowDrawable = Context.Resources.GetDrawable(value);
            }
        }

		/// <summary>
		/// Sets the secondary (right) shadow drawable.
		/// </summary>
		/// <param name="d"> the new shadow drawable </param>
        public Drawable SecondaryShadowDrawable
        {
            set
            {
                mViewBehind.SecondaryShadowDrawable = value;
            }
        }

		/// <summary>
		/// Sets the shadow width.
		/// </summary>
		/// <param name="resId"> The dimension resource id to be set as the shadow width. </param>
		public int ShadowWidthRes
		{
			set
			{
				ShadowWidth = (int)Resources.GetDimension(value);
			}
		}

		/// <summary>
		/// Sets the shadow width.
		/// </summary>
		/// <param name="pixels"> the new shadow width, in pixels </param>
		public int ShadowWidth
		{
			set
			{
				mViewBehind.ShadowWidth = value;
			}
		}

		/// <summary>
		/// Enables or disables the SlidingMenu's fade in and out
		/// </summary>
		/// <param name="b"> true to enable fade, false to disable it </param>
		public bool FadeEnabled
		{
			set
			{
				mViewBehind.FadeEnabled = value;
			}
		}

		/// <summary>
		/// Sets how much the SlidingMenu fades in and out. Fade must be enabled, see
		/// <seealso cref="#setFadeEnabled(boolean) setFadeEnabled(boolean)"/>
		/// </summary>
		/// <param name="f"> the new fade degree, between 0.0f and 1.0f </param>
		public float FadeDegree
		{
			set
			{
				mViewBehind.FadeDegree = value;
			}
		}

		/// <summary>
		/// Enables or disables whether the selector is drawn
		/// </summary>
		/// <param name="b"> true to draw the selector, false to not draw the selector </param>
		public bool SelectorEnabled
		{
			set
			{
				mViewBehind.SelectorEnabled = true;
			}
		}

		/// <summary>
		/// Sets the selected view. The selector will be drawn here
		/// </summary>
		/// <param name="v"> the new selected view </param>
		public View SelectedView
		{
			set
			{
				mViewBehind.SelectedView = value;
			}
		}

		/// <summary>
		/// Sets the selector drawable.
		/// </summary>
		/// <param name="res"> a resource ID for the selector drawable </param>
		public int SelectorDrawable
		{
			set
			{
				mViewBehind.SelectorBitmap = BitmapFactory.DecodeResource(Resources, value);
			}
		}

		/// <summary>
		/// Sets the selector drawable.
		/// </summary>
		/// <param name="b"> the new selector bitmap </param>
		public Bitmap SelectorBitmap
		{
			set
			{
				mViewBehind.SelectorBitmap = value;
			}
		}

		/// <summary>
		/// Add a View ignored by the Touch Down event when mode is Fullscreen
		/// </summary>
		/// <param name="v"> a view to be ignored </param>
		public void AddIgnoredView(View v)
		{
			mViewAbove.AddIgnoredView(v);
		}

		/// <summary>
		/// Remove a View ignored by the Touch Down event when mode is Fullscreen
		/// </summary>
		/// <param name="v"> a view not wanted to be ignored anymore </param>
		public void RemoveIgnoredView(View v)
		{
			mViewAbove.RemoveIgnoredView(v);
		}

		/// <summary>
		/// Clear the list of Views ignored by the Touch Down event when mode is Fullscreen
		/// </summary>
		public void ClearIgnoredViews()
		{
			mViewAbove.ClearIgnoredViews();
		}

		/// <summary>
		/// Sets the OnOpenListener. <seealso cref="OnOpenListener#onOpen() OnOpenListener.onOpen()"/> will be called when the SlidingMenu is opened
		/// </summary>
		/// <param name="listener"> the new OnOpenListener </param>
		public IOnOpenListener OnOpenListener
		{
			set
			{
				//mViewAbove.setOnOpenListener(value);
				mOpenListener = value;
			}
		}

		/// <summary>
		/// Sets the OnOpenListner for secondary menu  <seealso cref="OnOpenListener#onOpen() OnOpenListener.onOpen()"/> will be called when the secondary SlidingMenu is opened
		/// </summary>
		/// <param name="listener"> the new OnOpenListener </param>

		public IOnOpenListener SecondaryOnOpenListner
		{
			set
			{
				mSecondaryOpenListner = value;
			}
		}

		/// <summary>
		/// Sets the OnCloseListener. <seealso cref="OnCloseListener#onClose() OnCloseListener.onClose()"/> will be called when any one of the SlidingMenu is closed
		/// </summary>
		/// <param name="listener"> the new setOnCloseListener </param>
		public IOnCloseListener OnCloseListener
		{
			set
			{
				//mViewAbove.setOnCloseListener(value);
				mCloseListener = value;
			}
		}

		/// <summary>
		/// Sets the OnOpenedListener. <seealso cref="OnOpenedListener#onOpened() OnOpenedListener.onOpened()"/> will be called after the SlidingMenu is opened
		/// </summary>
		/// <param name="listener"> the new OnOpenedListener </param>
		public IOnOpenedListener OnOpenedListener
		{
			set
			{
				mViewAbove.OnOpenedListener = value;
			}
		}

		/// <summary>
		/// Sets the OnClosedListener. <seealso cref="OnClosedListener#onClosed() OnClosedListener.onClosed()"/> will be called after the SlidingMenu is closed
		/// </summary>
		/// <param name="listener"> the new OnClosedListener </param>
		public IOnClosedListener OnClosedListener
		{
			set
			{
				mViewAbove.OnClosedListener = value;
			}
		}

		public class SavedState : BaseSavedState
		{
			private int mItem;

			public SavedState(IParcelable superState, int item) 
                : base(superState)
			{
				mItem = item;
			}

            internal SavedState(Parcel source)
                : base(source)
			{
                mItem = source.ReadInt();
			}

			public int Item
			{
				get
				{
					return mItem;
				}
			}

			/* (non-Javadoc)
			 * @see android.view.AbsSavedState#writeToParcel(android.os.Parcel, int)
			 */
            public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
            {
                base.WriteToParcel(dest, flags);
                dest.WriteInt(mItem);
            }

            [ExportField("CREATOR")]
            static SavedStateCreator InitializeCreator()
            {
                Console.WriteLine("MyParcelable.InitializeCreator");
                return new SavedStateCreator();
            }

            public class SavedStateCreator : Java.Lang.Object, IParcelableCreator
            {
                public Java.Lang.Object CreateFromParcel(Parcel source)
                {
                    Console.WriteLine("MyParcelableCreator.CreateFromParcel");
                    return new SavedState(source);
                }

                public Java.Lang.Object[] NewArray(int size)
                {
                    Console.WriteLine("MyParcelableCreator.NewArray");
                    return new SavedState[size];
                }
            }
		}

		/* (non-Javadoc)
		 * @see android.view.View#onSaveInstanceState()
		 */
		protected override IParcelable OnSaveInstanceState()
		{
			IParcelable superState = base.OnSaveInstanceState();
			SavedState ss = new SavedState(superState, mViewAbove.CurrentItem);
			return ss;
		}

		/* (non-Javadoc)
		 * @see android.view.View#onRestoreInstanceState(android.os.Parcelable)
		 */
		protected override void OnRestoreInstanceState(IParcelable state)
		{
			SavedState ss = (SavedState)state;
			base.OnRestoreInstanceState(ss.SuperState);
			mViewAbove.CurrentItem = ss.Item;
		}

		/* (non-Javadoc)
		 * @see android.view.ViewGroup#fitSystemWindows(android.graphics.Rect)
		 */
		protected override bool FitSystemWindows(Rect insets)
		{
			int leftPadding = insets.Left;
			int rightPadding = insets.Right;
			int topPadding = insets.Top;
			int bottomPadding = insets.Bottom;
			if (!mActionbarOverlay)
			{
				Console.WriteLine(TAG + " setting padding!");
				SetPadding(leftPadding, topPadding, rightPadding, bottomPadding);
			}
			return true;
		}
        
        [TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
		public void ManageLayers(float percentOpen)
		{
			if (Build.VERSION.SdkInt < (BuildVersionCodes)11)
			{
				return;
			}

			bool layer = percentOpen > 0.0f && percentOpen < 1.0f;
            LayerType layerType = layer ? LayerType.Hardware : LayerType.None;

			if (layerType != GetContent().LayerType)
			{
                Handler.Post(() =>
                    {
                        Console.WriteLine(TAG + " changing layerType. hardware? " + (layerType == LayerType.Hardware));
                        GetContent().SetLayerType(layerType, null);
                        GetMenu().SetLayerType(layerType, null);
                        if (GetSecondaryMenu() != null)
                        {
                            GetSecondaryMenu().SetLayerType(layerType, null);
                        }
                    });
			}
		}
	}
}