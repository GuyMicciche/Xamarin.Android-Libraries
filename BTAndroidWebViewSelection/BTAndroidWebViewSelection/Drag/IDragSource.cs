using Android.Views;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Interface defining an object where drag operations originate.
	/// 
	/// </summary>
	public interface IDragSource
	{
		/// <summary>
		/// This method is called to determine if the DragSource has something to drag.
		/// </summary>
		/// <returns> True if there is something to drag </returns>

		bool AllowDrag();

		/// <summary>
		/// This method is used to tell the DragSource which drag controller it is working with.
		/// </summary>
		/// <param name="dragger"> DragController </param>

		DragController DragController {set;}

		/// <summary>
		/// This method is called on the completion of the drag operation so the DragSource knows 
		/// whether it succeeded or failed.
		/// </summary>
		/// <param name="target"> View - the view that accepted the dragged object </param>
		/// <param name="success"> boolean - true means that the object was dropped successfully </param>

		void OnDropCompleted(View target, bool success);
	}
}