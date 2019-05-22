namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react to positive, negative and neutral dialog buttons.
	/// </summary>
	public interface ISimpleDialogListener : IPositiveButtonDialogListener, INegativeButtonDialogListener, INeutralButtonDialogListener
	{

	}
}