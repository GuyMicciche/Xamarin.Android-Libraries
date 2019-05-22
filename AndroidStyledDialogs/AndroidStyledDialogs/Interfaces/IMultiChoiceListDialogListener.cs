using Java.Lang;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Interface for ListDialogFragment in modes: CHOICE_MODE_MULTIPLE
	/// Implement it in Activity or Fragment to react to item selection.
	/// </summary>
	public interface IMultiChoiceListDialogListener
	{
		void OnListItemsSelected(string[] values, int[] selectedPositions, int requestCode);
	}
}