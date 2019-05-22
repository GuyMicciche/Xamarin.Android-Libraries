using Java.Lang;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Interface for ListDialogFragment in modes: CHOICE_MODE_NONE, CHOICE_MODE_SINGLE
	/// Implement it in Activity or Fragment to react to item selection.
	/// </summary>
	public interface IListDialogListener
	{
		void OnListItemSelected(string value, int number, int requestCode);
	}
}