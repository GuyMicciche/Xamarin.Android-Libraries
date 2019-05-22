using Java.Util;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Implement this interface in Activity or Fragment to react to positive and negative buttons of date/time dialog.
	/// </summary>
	public interface IDateDialogListener
	{
        void OnPositiveButtonClicked(int requestCode, Date date);

        void OnNegativeButtonClicked(int requestCode, Date date);
	}
}