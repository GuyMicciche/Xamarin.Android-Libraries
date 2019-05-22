namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react to neutral dialog buttons.
	/// </summary>
	public interface INeutralButtonDialogListener
	{
		void OnNeutralButtonClicked(int requestCode);
	}
}