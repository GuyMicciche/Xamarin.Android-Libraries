namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Text Selection Listener Interface
	/// </summary>
	public interface ITextSelectionJavascriptInterfaceListener
	{
		/// <summary>
		/// Informs the listener that there was a javascript error. </summary>
		/// <param name="error"> </param>
		void tsjiJSError(string error);


		/// <summary>
		/// The user has started dragging the selection handles.
		/// </summary>
		void tsjiStartSelectionMode();

		/// <summary>
		/// The user has stopped dragging the selection handles.
		/// </summary>
		void tsjiEndSelectionMode();

		/// <summary>
		/// Tells the listener to show the context menu for the given range and selected text.
		/// The bounds parameter contains a json string representing the selection bounds in the form 
		/// { 'left': leftPoint, 'top': topPoint, 'right': rightPoint, 'bottom': bottomPoint } </summary>
		/// <param name="range"> </param>
		/// <param name="text"> </param>
		/// <param name="handleBounds"> </param>
		/// <param name="menuBounds"> </param>
		void tsjiSelectionChanged(string range, string text, string handleBounds, string menuBounds);

		/// <summary>
		/// Sends the content width to the listener.  
		/// Necessary because Android web views don't allow you to get the content width. </summary>
		/// <param name="contentWidth"> </param>
		void tsjiSetContentWidth(float contentWidth);
	}
}