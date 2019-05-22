namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Interface to receive notifications when a drag starts or stops
	/// </summary>
	public interface IDragListener
	{
		/// <summary>
		/// A drag has begun
		/// </summary>
		/// <param name="source"> An object representing where the drag originated </param>
		/// <param name="info"> The data associated with the object that is being dragged </param>
		/// <param name="dragAction"> The drag action: either <seealso cref="DragController#DRAG_ACTION_MOVE"/>
		///        or <seealso cref="DragController#DRAG_ACTION_COPY"/> </param>
		void OnDragStart(IDragSource source, object info, int dragAction);

		/// <summary>
		/// Fired while dragging the source to the target.
		/// 
		/// </summary>
		void OnDrag();

		/// <summary>
		/// The drag has eneded
		/// </summary>
		void OnDragEnd();
	}
}