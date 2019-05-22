using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Util;
using Org.Json;
using System;
using Java.Interop;
using Java.Lang;
using Java.Lang.Reflect;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Webview subclass that hijacks web content selection.
	/// </summary>
	public class BTWebView : WebView, ITextSelectionJavascriptInterfaceListener, View.IOnTouchListener, View.IOnLongClickListener, IOnDismissListener, IDragListener
	{
		/// <summary>
		/// The logging tag. </summary>
		private const string TAG = "BTWebView";

		/// <summary>
		/// Context. </summary>
		protected Context mContext;

		/// <summary>
		/// The context menu. </summary>
		protected QuickAction mContextMenu;

		/// <summary>
		/// The drag layer for selection. </summary>
		protected DragLayer mSelectionDragLayer;

		/// <summary>
		/// The drag controller for selection. </summary>
		protected DragController mDragController;

		/// <summary>
		/// The selection bounds. </summary>
		protected Rect mSelectionBounds = null;

		/// <summary>
		/// The previously selected region. </summary>
		protected Region mLastSelectedRegion = null;

		/// <summary>
		/// The selected range. </summary>
		protected string mSelectedRange = "";

		/// <summary>
		/// The selected text. </summary>
		protected string mSelectedText = "";

		/// <summary>
		/// Javascript interface for catching text selection. </summary>
		protected TextSelectionJavascriptInterface mTextSelectionJSInterface = null;

		/// <summary>
		/// Selection mode flag. </summary>
		protected bool mInSelectionMode = false;

		/// <summary>
		/// Flag for dragging. </summary>
		protected bool mDragging = false;

		/// <summary>
		/// Flag to stop from showing context menu twice. </summary>
		protected bool mContextMenuVisible = false;

		/// <summary>
		/// The current content width. </summary>
		protected int mContentWidth = 0;

		/// <summary>
		/// The current scale of the web view. </summary>
		protected float mCurrentScale = 1.0f;

		//*****************************************************
		//*
		//*			Selection Handles
		//*
		//*****************************************************

		/// <summary>
		/// The start selection handle. </summary>
		protected ImageView mStartSelectionHandle;

		/// <summary>
		/// the end selection handle. </summary>
		protected ImageView mEndSelectionHandle;

		/// <summary>
		/// Identifier for the selection start handle. </summary>
		protected readonly int SELECTION_START_HANDLE = 0;

		/// <summary>
		/// Identifier for the selection end handle. </summary>
		protected readonly int SELECTION_END_HANDLE = 1;

		/// <summary>
		/// Last touched selection handle. </summary>
		protected int mLastTouchedSelectionHandle = -1;

        /// <summary>
        /// Starts selection mode on the UI thread
        /// </summary>
        private Handler startSelectionModeHandler;
        // Ends selection mode on the UI thread
        private Handler endSelectionModeHandler;
        /// <summary>
        /// Handler for drawing the selection handles on the UI thread.
        /// </summary>
        private Handler drawSelectionHandlesHandler;

		public BTWebView(Context context)
            : base(context)
		{

			mContext = context;
			Setup(context);
		}

		public BTWebView(Context context, IAttributeSet attrs, int defStyle) 
            : base(context, attrs, defStyle)
		{
			mContext = context;
			Setup(context);
		}

		public BTWebView(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			mContext = context;
			Setup(context);
		}

		//*****************************************************
		//*
		//*		Touch Listeners
		//*
		//*****************************************************

		private bool mScrolling = false;
		private float mScrollDiffY = 0;
		private float mLastTouchY = 0;
		private float mScrollDiffX = 0;
		private float mLastTouchX = 0;

		public bool OnTouch(View v, MotionEvent ev)
		{

			float xPoint = GetDensityIndependentValue(ev.GetX(), mContext) / GetDensityIndependentValue(Scale, mContext);
			float yPoint = GetDensityIndependentValue(ev.GetY(), mContext) / GetDensityIndependentValue(Scale, mContext);

			if (ev.Action == MotionEventActions.Down)
			{

				string startTouchUrl = Java.Lang.String.Format(Locale.Us, "javascript:android.selection.startTouch(%f, %f);", xPoint, yPoint);

				mLastTouchX = xPoint;
				mLastTouchY = yPoint;

				Console.WriteLine(TAG + " scale " + Scale);

				LoadUrl(startTouchUrl);

				// Flag scrolling for first touch
				//mScrolling = !isInSelectionMode();
			}
			else if (ev.Action == MotionEventActions.Up)
			{
				// Check for scrolling flag
				if (!mScrolling)
				{
					mScrolling = false;
					EndSelectionMode();
					return false;
				}

				mScrollDiffX = 0;
				mScrollDiffY = 0;
				mScrolling = false;

				// Fixes 4.4 double selection
				return true;

			}
            else if (ev.Action == MotionEventActions.Move)
			{

				mScrollDiffX += (xPoint - mLastTouchX);
				mScrollDiffY += (yPoint - mLastTouchY);

				mLastTouchX = xPoint;
				mLastTouchY = yPoint;

				// Only account for legitimate movement.
				mScrolling = (System.Math.Abs(mScrollDiffX) > 10 || System.Math.Abs(mScrollDiffY) > 10);

			}

			// If this is in selection mode, then nothing else should handle this touch
			return false;
		}

		public bool OnLongClick(View v)
		{

			// Tell the javascript to handle this if not in selection mode
			if (!InSelectionMode)
			{
				LoadUrl("javascript:android.selection.longTouch();");
				mScrolling = true;
			}

			// Don't let the webview handle it
			return true;
		}

		//*****************************************************
		//*
		//*		Setup
		//*
		//*****************************************************

		/// <summary>
		/// Setups up the web view. </summary>
		/// <param name="context"> </param>
		protected void Setup(Context context)
		{
			// On Touch Listener
            SetOnLongClickListener(this);
			SetOnTouchListener(this);

			// Webview setup
			Settings.JavaScriptEnabled = true;
			Settings.JavaScriptCanOpenWindowsAutomatically = true;
			Settings.SetPluginState(WebSettings.PluginState.On);
			//getSettings().setBuiltInZoomControls(true);

			// Webview client.
			SetWebViewClient(new MyWebViewClient(this));

			// Zoom out fully
			//getSettings().setLoadWithOverviewMode(true);
			//getSettings().setUseWideViewPort(true);

			// Javascript interfaces
			mTextSelectionJSInterface = new TextSelectionJavascriptInterface(context, this);
			AddJavascriptInterface(mTextSelectionJSInterface, mTextSelectionJSInterface.InterfaceName);


			// Create the selection handles
			CreateSelectionLayer(context);


			// Set to the empty region
			Region region = new Region();
			region.SetEmpty();
			mLastSelectedRegion = region;

			// Load up the android asset file
			string filePath = "file:///android_asset/content.html";

			// Load the url
			this.LoadUrl(filePath);

            startSelectionModeHandler = new MyHandler(StartSelectionHandleMessage);
            endSelectionModeHandler = new MyHandler(EndSelectionHandleMessage);
            drawSelectionHandlesHandler = new MyHandler(DrawSelectionHandleMessage);
		}

		private class MyWebViewClient : WebViewClient
		{
			private readonly BTWebView outerInstance;

			public MyWebViewClient(BTWebView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

					// This is how it is supposed to work, so I'll leave it in, but this doesn't get called on pinch
					// So for now I have to use deprecated getScale method.
			public override void OnScaleChanged(WebView view, float oldScale, float newScale)
			{
				base.OnScaleChanged(view, oldScale, newScale);
				outerInstance.mCurrentScale = newScale;
			}
		}

		//*****************************************************
		//*
		//*		Selection Layer Handling
		//*
		//*****************************************************

		/// <summary>
		/// Creates the selection layer.
		/// </summary>
		/// <param name="context"> </param>
		protected void CreateSelectionLayer(Context context)
		{
			LayoutInflater inflater = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
			mSelectionDragLayer = (DragLayer)inflater.Inflate(Resource.Layout.selection_drag_layer, null);

			// Make sure it's filling parent
			mDragController = new DragController(context);
			mDragController.DragListener = this;
			mDragController.AddDropTarget(mSelectionDragLayer);
			mSelectionDragLayer.DragController = mDragController;


			mStartSelectionHandle = (ImageView) mSelectionDragLayer.FindViewById(Resource.Id.startHandle);
			mStartSelectionHandle.Tag = new int?(SELECTION_START_HANDLE);
            mEndSelectionHandle = (ImageView)mSelectionDragLayer.FindViewById(Resource.Id.endHandle);
			mEndSelectionHandle.Tag = new int?(SELECTION_END_HANDLE);

			View.IOnTouchListener handleTouchListener = new WebViewOnTouchListener(this);

			mStartSelectionHandle.SetOnTouchListener(handleTouchListener);
            mEndSelectionHandle.SetOnTouchListener(handleTouchListener);
		}

		private class WebViewOnTouchListener : Java.Lang.Object, View.IOnTouchListener
		{
			private readonly BTWebView webview;

            public WebViewOnTouchListener(BTWebView webview)
			{
                this.webview = webview;
			}

			public bool OnTouch(View v, MotionEvent ev)
			{
				bool handledHere = false;

                MotionEventActions action = ev.Action;

				// Down event starts drag for handle.
				if (action == MotionEventActions.Down)
				{
					handledHere = webview.StartDrag(v);
					webview.mLastTouchedSelectionHandle = (int)v.Tag;
				}

				return handledHere;
			}
		}

        protected void StartSelectionHandleMessage(Message m)
        {
            if (mSelectionBounds == null)
            {
                return;
            }

            AddView(mSelectionDragLayer);

            DrawSelectionHandles();

            int contentHeight = (int)System.Math.Ceiling(GetDensityDependentValue(ContentHeight, mContext));

            // Update Layout Params
            ViewGroup.LayoutParams layerParams = mSelectionDragLayer.LayoutParameters;
            layerParams.Height = contentHeight;
            layerParams.Width = mContentWidth;
            mSelectionDragLayer.LayoutParameters = layerParams;
        }

        protected void EndSelectionHandleMessage(Message m)
        {
            if (Parent != null && mContextMenu != null && mContextMenuVisible)
            {
                // This will throw an error if the webview is being redrawn.
                // No error handling needed, just need to stop the crash.
                try
                {
                    mContextMenu.Dismiss();
                }
                catch (System.Exception e)
                {

                }
            }
            mSelectionBounds = null;
            mLastTouchedSelectionHandle = -1;
            LoadUrl("javascript: android.selection.clearSelection();");
            RemoveView(mSelectionDragLayer);
        }

        protected void DrawSelectionHandleMessage(Message m)
        {

            MyAbsoluteLayout.LayoutParams startParams = (MyAbsoluteLayout.LayoutParams)mStartSelectionHandle.LayoutParameters;
            startParams.X = (int)(mSelectionBounds.Left - mStartSelectionHandle.Drawable.IntrinsicWidth);
            startParams.Y = (int)(mSelectionBounds.Top - mStartSelectionHandle.Drawable.IntrinsicHeight);

            // Stay on screen.
            startParams.X = (startParams.X < 0) ? 0 : startParams.X;
            startParams.Y = (startParams.Y < 0) ? 0 : startParams.Y;

            mStartSelectionHandle.LayoutParameters = startParams;

            MyAbsoluteLayout.LayoutParams endParams = (MyAbsoluteLayout.LayoutParams)mEndSelectionHandle.LayoutParameters;
            endParams.X = (int)mSelectionBounds.Right;
            endParams.Y = (int)mSelectionBounds.Bottom;

            // Stay on screen
            endParams.X = (endParams.X < 0) ? 0 : endParams.X;
            endParams.Y = (endParams.Y < 0) ? 0 : endParams.Y;

            mEndSelectionHandle.LayoutParameters = endParams;
        }

		/// <summary>
		/// Starts selection mode.
		/// 
		/// </summary>
		public void StartSelectionMode()
		{
			startSelectionModeHandler.SendEmptyMessage(0);

		}
        
		/// <summary>
		/// Ends selection mode.
		/// </summary>
		public void EndSelectionMode()
		{
			endSelectionModeHandler.SendEmptyMessage(0);
		}

		/// <summary>
		/// Calls the handler for drawing the selection handles.
		/// </summary>
		private void DrawSelectionHandles()
		{
			drawSelectionHandlesHandler.SendEmptyMessage(0);
		}
        		
		/// <summary>
		/// Checks to see if this view is in selection mode.
		/// @return
		/// </summary>
		public bool InSelectionMode
		{
			get
			{
				return mSelectionDragLayer.Parent != null;
			}
		}

		/// <summary>
		/// Checks to see if the view is currently dragging.
		/// @return
		/// </summary>
		public bool Dragging
		{
			get
			{
				return mDragging;
			}
		}

		//*****************************************************
		//*
		//*		DragListener Methods
		//*
		//*****************************************************

		/// <summary>
		/// Start dragging a view.
		/// 
		/// </summary>
		private bool StartDrag(View v)
		{
			// Let the DragController initiate a drag-drop sequence.
			// I use the dragInfo to pass along the object being dragged.
			// I'm not sure how the Launcher designers do this.

			mDragging = true;
			object dragInfo = v;
			mDragController.StartDrag(v, mSelectionDragLayer, dragInfo, DragController.DRAG_ACTION_MOVE);
			return true;
		}

		public void OnDragStart(IDragSource source, object info, int dragAction)
		{
			// TODO Auto-generated method stub
		}

		public void OnDrag()
		{
			// TODO Auto-generated method stub

			MyAbsoluteLayout.LayoutParams startHandleParams = (MyAbsoluteLayout.LayoutParams)mStartSelectionHandle.LayoutParameters;
			MyAbsoluteLayout.LayoutParams endHandleParams = (MyAbsoluteLayout.LayoutParams)mEndSelectionHandle.LayoutParameters;

			float scale = GetDensityIndependentValue(Scale, mContext);

			float startX = startHandleParams.X - ScrollX;
			float startY = startHandleParams.Y - ScrollY;
			float endX = endHandleParams.X - ScrollX;
			float endY = endHandleParams.Y - ScrollY;

			startX = GetDensityIndependentValue(startX, mContext) / scale;
			startY = GetDensityIndependentValue(startY, mContext) / scale;
			endX = GetDensityIndependentValue(endX, mContext) / scale;
			endY = GetDensityIndependentValue(endY, mContext) / scale;

			if (mLastTouchedSelectionHandle == SELECTION_START_HANDLE && startX > 0 && startY > 0)
			{
				string saveStartString = Java.Lang.String.Format(Locale.Us, "javascript: android.selection.setStartPos(%f, %f);", startX, startY);
				LoadUrl(saveStartString);
			}

			if (mLastTouchedSelectionHandle == SELECTION_END_HANDLE && endX > 0 && endY > 0)
			{
                string saveEndString = Java.Lang.String.Format(Locale.Us, "javascript: android.selection.setEndPos(%f, %f);", endX, endY);
				LoadUrl(saveEndString);
			}
		}

		public void OnDragEnd()
		{
			// TODO Auto-generated method stub

			MyAbsoluteLayout.LayoutParams startHandleParams = (MyAbsoluteLayout.LayoutParams) mStartSelectionHandle.LayoutParameters;
			MyAbsoluteLayout.LayoutParams endHandleParams = (MyAbsoluteLayout.LayoutParams) mEndSelectionHandle.LayoutParameters;

			float scale = GetDensityIndependentValue(Scale, mContext);

			float startX = startHandleParams.X - ScrollX;
			float startY = startHandleParams.Y - ScrollY;
			float endX = endHandleParams.X - ScrollX;
			float endY = endHandleParams.Y - ScrollY;

			startX = GetDensityIndependentValue(startX, mContext) / scale;
			startY = GetDensityIndependentValue(startY, mContext) / scale;
			endX = GetDensityIndependentValue(endX, mContext) / scale;
			endY = GetDensityIndependentValue(endY, mContext) / scale;


			if (mLastTouchedSelectionHandle == SELECTION_START_HANDLE && startX > 0 && startY > 0)
			{
                string saveStartString = Java.Lang.String.Format(Locale.Us, "javascript: android.selection.setStartPos(%f, %f);", startX, startY);
				LoadUrl(saveStartString);
			}

			if (mLastTouchedSelectionHandle == SELECTION_END_HANDLE && endX > 0 && endY > 0)
			{
                string saveEndString = Java.Lang.String.Format(Locale.Us, "javascript: android.selection.setEndPos(%f, %f);", endX, endY);
				LoadUrl(saveEndString);
			}

			mDragging = false;
		}

		//*****************************************************
		//*
		//*		Context Menu Creation
		//*
		//*****************************************************

		/// <summary>
		/// Shows the context menu using the given region as an anchor point. </summary>
		/// <param name="displayRect"> </param>
		protected void ShowContextMenu(Rect displayRect)
		{
			// Don't show this twice
			if (mContextMenuVisible)
			{
				return;
			}

			// Don't use empty rect
			//if(displayRect.isEmpty()){
			if (displayRect.Right <= displayRect.Left)
			{
				return;
			}

			//Copy action item
			ActionItem buttonOne = new ActionItem();

			buttonOne.Title = "Button 1";
			buttonOne.ActionId = 1;
            buttonOne.Icon = Resources.GetDrawable(Resource.Drawable.menu_search);


			//Highlight action item
			ActionItem buttonTwo = new ActionItem();

			buttonTwo.Title = "Button 2";
			buttonTwo.ActionId = 2;
			buttonTwo.Icon = Resources.GetDrawable(Resource.Drawable.menu_info);

			ActionItem buttonThree = new ActionItem();

			buttonThree.Title = "Button 3";
			buttonThree.ActionId = 3;
            buttonThree.Icon = Resources.GetDrawable(Resource.Drawable.menu_eraser);

			// The action menu
			mContextMenu = new QuickAction(Context);
			mContextMenu.OnDismissListener = this;

			// Add buttons
			mContextMenu.AddActionItem(buttonOne);

			mContextMenu.AddActionItem(buttonTwo);

			mContextMenu.AddActionItem(buttonThree);

			//setup the action item click listener
			mContextMenu.OnActionItemClickListener = new OnActionItemClickListenerAnonymousInnerClassHelper(this);

			mContextMenuVisible = true;
			mContextMenu.Show(this, displayRect);
		}

		private class OnActionItemClickListenerAnonymousInnerClassHelper : IOnActionItemClickListener
		{
			private readonly BTWebView outerInstance;

			public OnActionItemClickListenerAnonymousInnerClassHelper(BTWebView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public void OnItemClick(QuickAction source, int pos, int actionId)
			{
				// TODO Auto-generated method stub
				if (actionId == 1)
				{
					// Do Button 1 stuff
                    Console.WriteLine(TAG + " Hit Button 1");
				}
				else if (actionId == 2)
				{
					// Do Button 2 stuff
                    Console.WriteLine(TAG + " Hit Button 2");
				}
				else if (actionId == 3)
				{
					// Do Button 3 stuff
					Console.WriteLine(TAG + " Hit Button 3");
				}

				outerInstance.mContextMenuVisible = false;
			}
		}

		//*****************************************************
		//*
		//*		OnDismiss Listener
		//*
		//*****************************************************

		/// <summary>
		/// Clears the selection when the context menu is dismissed.
		/// </summary>
		public void OnDismiss()
		{
			//clearSelection();
			mContextMenuVisible = false;
		}

		//*****************************************************
		//*
		//*		Text Selection Javascript Interface Listener
		//*
		//*****************************************************

		/// <summary>
		/// Shows/updates the context menu based on the range
		/// </summary>
		/// <param name="error"> </param>
		public void tsjiJSError(string error)
		{
			Console.WriteLine(TAG + " JSError: " + error);
		}

		/// <summary>
		/// The user has started dragging the selection handles.
		/// </summary>
		public void tsjiStartSelectionMode()
		{
			StartSelectionMode();
		}

		/// <summary>
		/// The user has stopped dragging the selection handles.
		/// </summary>
		public void tsjiEndSelectionMode()
		{
			EndSelectionMode();
		}

		/// <summary>
		/// The selection has changed </summary>
		/// <param name="range"> </param>
		/// <param name="text"> </param>
		/// <param name="handleBounds"> </param>
		/// <param name="menuBounds"> </param>
		public void tsjiSelectionChanged(string range, string text, string handleBounds, string menuBounds)
		{
			HandleSelection(range, text, handleBounds);
			Rect displayRect = GetContextMenuBounds(menuBounds);

			if (displayRect != null)
				// This will send the menu rect
			{
				ShowContextMenu(displayRect);
			}
		}

		/// <summary>
		/// Receives the content width for the page.
		/// </summary>
		public void tsjiSetContentWidth(float contentWidth)
		{
			mContentWidth = (int) GetDensityDependentValue(contentWidth, mContext);
		}

		//*****************************************************
		//*
		//*		Convenience
		//*
		//*****************************************************

		/// <summary>
		/// Puts up the selection view. </summary>
		/// <param name="range"> </param>
		/// <param name="text"> </param>
		/// <param name="handleBounds">
		/// @return </param>
		protected void HandleSelection(string range, string text, string handleBounds)
		{
			try
			{
				JSONObject selectionBoundsObject = new JSONObject(handleBounds);

				float scale = GetDensityIndependentValue(Scale, mContext);

				Rect handleRect = new Rect();
				handleRect.Left = (int)(GetDensityDependentValue(selectionBoundsObject.GetInt("left"), Context) * scale);
                handleRect.Top = (int)(GetDensityDependentValue(selectionBoundsObject.GetInt("top"), Context) * scale);
                handleRect.Right = (int)(GetDensityDependentValue(selectionBoundsObject.GetInt("right"), Context) * scale);
                handleRect.Bottom = (int)(GetDensityDependentValue(selectionBoundsObject.GetInt("bottom"), Context) * scale);

				mSelectionBounds = handleRect;
				mSelectedRange = range;
				mSelectedText = text;

				if (!InSelectionMode)
				{
					StartSelectionMode();
				}

				DrawSelectionHandles();
			}
			catch (JSONException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		/// <summary>
		/// Calculates the context menu display rect </summary>
		/// <param name="menuBounds"> </param>
		/// <returns> The display Rect </returns>
		protected Rect GetContextMenuBounds(string menuBounds)
		{
			try
			{
				JSONObject menuBoundsObject = new JSONObject(menuBounds);

				float scale = GetDensityIndependentValue(Scale, mContext);

				Rect displayRect = new Rect();
				displayRect.Left = (int)(GetDensityDependentValue(menuBoundsObject.GetInt("left"), Context) * scale);
                displayRect.Top = (int)(GetDensityDependentValue(menuBoundsObject.GetInt("top") - 25, Context) * scale);
                displayRect.Right = (int)(GetDensityDependentValue(menuBoundsObject.GetInt("right"), Context) * scale);
                displayRect.Bottom = (int)(GetDensityDependentValue(menuBoundsObject.GetInt("bottom") + 25, Context) * scale);

				return displayRect;
			}
			catch (JSONException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			return null;
		}

		//*****************************************************
		//*
		//*		Density Conversion
		//*
		//*****************************************************

		/// <summary>
		/// Returns the density dependent value of the given float </summary>
		/// <param name="val"> </param>
		/// <param name="ctx">
		/// @return </param>
		public float GetDensityDependentValue(float val, Context ctx)
		{
			// Get display from context
            var windowService = Context.GetSystemService(Context.WindowService);
            var windowManager = windowService.JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;

			// Calculate min bound based on metrics
			DisplayMetrics metrics = new DisplayMetrics();
			display.GetMetrics(metrics);

			return val * ((float)metrics.DensityDpi / 160f);

			//return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, val, metrics);
		}

		/// <summary>
		/// Returns the density independent value of the given float </summary>
		/// <param name="val"> </param>
		/// <param name="ctx">
		/// @return </param>
		public float GetDensityIndependentValue(float val, Context ctx)
		{
			// Get display from context
            var windowService = Context.GetSystemService(Context.WindowService);
            var windowManager = windowService.JavaCast<IWindowManager>();
            Display display = windowManager.DefaultDisplay;

			// Calculate min bound based on metrics
			DisplayMetrics metrics = new DisplayMetrics();
            display.GetMetrics(metrics);

			return val / ((float)metrics.DensityDpi / 160f);

			//return TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_PX, val, metrics);
		}
	}

    public class MyHandler : Handler
    {
        Action<Message> handle_message;

        public MyHandler(Action<Message> handler)
        {
            this.handle_message = handler;
        }

        public override void HandleMessage(Message msg)
        {
            handle_message(msg);
        }
    }
}