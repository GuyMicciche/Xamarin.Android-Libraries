namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react to negative dialog buttons.
	/// </summary>
	public interface INegativeButtonDialogListener
	{
		void OnNegativeButtonClicked(int requestCode);
	}
}