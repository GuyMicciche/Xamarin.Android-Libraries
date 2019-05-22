using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// A ViewGroup that coordinates dragging across its dscendants.
	/// 
	/// <p> This class used DragLayer in the Android Launcher activity as a model.
	/// It is a bit different in several respects:
	/// (1) It extends MyAbsoluteLayout rather than FrameLayout; (2) it implements DragSource and DropTarget methods
	/// that were done in a separate Workspace class in the Launcher.
	/// </summary>
	public class DragLayer : MyAbsoluteLayout, IDragSource, IDropTarget
	{
		private DragController mDragController;

		/// <summary>
		/// Used to create a new DragLayer from XML.
		/// </summary>
		/// <param name="context"> The application's context. </param>
		/// <param name="attrs"> The attribtues set containing the Workspace's customization values. </param>
		public DragLayer(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{

		}

		public DragController DragController
		{
			set
			{
				mDragController = value;
			}
		}

		public override bool DispatchKeyEvent(KeyEvent ev)
		{
			return mDragController.DispatchKeyEvent(ev) || base.DispatchKeyEvent(ev);
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			return mDragController.OnInterceptTouchEvent(ev);
		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			return mDragController.OnTouchEvent(ev);
		}

		public override bool DispatchUnhandledMove(View focused, FocusSearchDirection direction)
		{
			return mDragController.DispatchUnhandledMove(focused, direction);
		}

	    // DragSource interface methods

	    /// <summary>
	    /// This method is called to determine if the DragSource has something to drag.
	    /// </summary>
	    /// <returns> True if there is something to drag </returns>

	    public bool AllowDrag()
	    {
		    // In this simple demo, any view that you touch can be dragged.
		    return true;
	    }

	    /// <summary>
	    /// setDragController
	    /// 
	    /// </summary>

	     /* setDragController is already defined. See above. */

	    /// <summary>
	    /// onDropCompleted
	    /// 
	    /// </summary>

	    public void OnDropCompleted(View target, bool success)
	    {

	    }

	    // DropTarget interface implementation

	    /// <summary>
	    /// Handle an object being dropped on the DropTarget.
	    /// This is the where a dragged view gets repositioned at the end of a drag.
	    /// </summary>
	    /// <param name="source"> DragSource where the drag started </param>
	    /// <param name="x"> X coordinate of the drop location </param>
	    /// <param name="y"> Y coordinate of the drop location </param>
	    /// <param name="xOffset"> Horizontal offset with the object being dragged where the original
	    ///          touch happened </param>
	    /// <param name="yOffset"> Vertical offset with the object being dragged where the original
	    ///          touch happened </param>
	    /// <param name="dragView"> The DragView that's being dragged around on screen. </param>
	    /// <param name="dragInfo"> Data associated with the object being dragged
	    ///  </param>
	    public void OnDrop(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo)
	    {
		    View v = (View) dragInfo;

		    int w = v.Width;
		    int h = v.Height;
		    int left = x - xOffset;
		    int top = y - yOffset;
		    DragLayer.LayoutParams lp = new DragLayer.LayoutParams(w, h, left, top);
		    this.UpdateViewLayout(v, lp);
	    }

	    public void OnDragEnter(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo)
	    {
	    }

	    public void OnDragOver(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo)
	    {
		     View v = (View) dragInfo;

			    int w = v.Width;
			    int h = v.Height;
			    int left = x - xOffset;
			    int top = y - yOffset;
			    DragLayer.LayoutParams lp = new DragLayer.LayoutParams(w, h, left, top);
			    this.UpdateViewLayout(v, lp);
	    }

	    public void OnDragExit(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo)
	    {

	    }

	    /// <summary>
	    /// Check if a drop action can occur at, or near, the requested location.
	    /// This may be called repeatedly during a drag, so any calls should return
	    /// quickly.
	    /// </summary>
	    /// <param name="source"> DragSource where the drag started </param>
	    /// <param name="x"> X coordinate of the drop location </param>
	    /// <param name="y"> Y coordinate of the drop location </param>
	    /// <param name="xOffset"> Horizontal offset with the object being dragged where the
	    ///            original touch happened </param>
	    /// <param name="yOffset"> Vertical offset with the object being dragged where the
	    ///            original touch happened </param>
	    /// <param name="dragView"> The DragView that's being dragged around on screen. </param>
	    /// <param name="dragInfo"> Data associated with the object being dragged </param>
	    /// <returns> True if the drop will be accepted, false otherwise. </returns>
	    public bool AcceptDrop(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo)
	    {
		    return true;
	    }

	    /// <summary>
	    /// Estimate the surface area where this object would land if dropped at the
	    /// given location.
	    /// </summary>
	    /// <param name="source"> DragSource where the drag started </param>
	    /// <param name="x"> X coordinate of the drop location </param>
	    /// <param name="y"> Y coordinate of the drop location </param>
	    /// <param name="xOffset"> Horizontal offset with the object being dragged where the
	    ///            original touch happened </param>
	    /// <param name="yOffset"> Vertical offset with the object being dragged where the
	    ///            original touch happened </param>
	    /// <param name="dragView"> The DragView that's being dragged around on screen. </param>
	    /// <param name="dragInfo"> Data associated with the object being dragged </param>
	    /// <param name="recycle"> <seealso cref="Rect"/> object to be possibly recycled. </param>
	    /// <returns> Estimated area that would be occupied if object was dropped at
	    ///         the given location. Should return null if no estimate is found,
	    ///         or if this target doesn't provide estimations. </returns>
        public Rect EstimateDropLocation(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo, Rect recycle)
	    {
		    return null;
	    }
	} // end class
}