using Android.Graphics;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Interface defining an object that can receive a view at the end of a drag operation.
	/// 
	/// </summary>
	public interface IDropTarget
	{
		/// <summary>
		/// Handle an object being dropped on the DropTarget
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
		void OnDrop(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo);

        void OnDragEnter(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo);

        void OnDragOver(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo);

        void OnDragExit(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo);

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
        bool AcceptDrop(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo);

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
        Rect EstimateDropLocation(IDragSource source, int x, int y, int xOffset, int yOffset, DragView dragView, object dragInfo, Rect recycle);

		// These methods are implemented in Views
		void GetHitRect(Rect outRect);
		void GetLocationOnScreen(int[] loc);
		int Left {get;}
		int Top {get;}
	}
}