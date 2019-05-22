namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react to positive dialog buttons.
	/// </summary>
	public interface IPositiveButtonDialogListener
	{
		void OnPositiveButtonClicked(int requestCode);
	}
}