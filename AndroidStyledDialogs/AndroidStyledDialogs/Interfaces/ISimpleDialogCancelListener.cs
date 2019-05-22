namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react when the dialog is cancelled.
	/// This listener is common for all types of dialogs.
	/// </summary>
	public interface ISimpleDialogCancelListener
	{
		void OnCancelled(int requestCode);
	}
}