using Android.Views;

namespace SlidingMenuLibrary.App
{
	public interface ISlidingActivityBase
	{

		/// <summary>
		/// Set the behind view content to an explicit view. This view is placed directly into the behind view 's view hierarchy.
		/// It can itself be a complex view hierarchy.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
		/// <param name="layoutParams"> Layout parameters for the view. </param>
		void SetBehindContentView(View view, ViewGroup.LayoutParams layoutParams);

		/// <summary>
		/// Set the behind view content to an explicit view. This view is placed directly into the behind view 's view hierarchy.
		/// It can itself be a complex view hierarchy. When calling this method, the layout parameters of the specified
		/// view are ignored. Both the width and the height of the view are set by default to MatchParent. To use your
		/// own layout parameters, invoke setContentView(android.view.View, android.view.ViewGroup.LayoutParams) instead.
		/// </summary>
		/// <param name="view"> The desired content to display. </param>
        void SetBehindContentView(View v);

		/// <summary>
		/// Set the behind view content from a layout resource. The resource will be inflated, adding all top-level views
		/// to the behind view.
		/// </summary>
		/// <param name="layoutResID"> Resource ID to be inflated. </param>
        void SetBehindContentView(int layoutResID);

		/// <summary>
		/// Gets the SlidingMenu associated with this activity.
		/// </summary>
		/// <returns> the SlidingMenu associated with this activity. </returns>
        SlidingMenu GetSlidingMenu();

		/// <summary>
		/// Toggle the SlidingMenu. If it is open, it will be closed, and vice versa.
		/// </summary>
		void Toggle();

		/// <summary>
		/// Close the SlidingMenu and show the content view.
		/// </summary>
		void ShowContent();

		/// <summary>
		/// Open the SlidingMenu and show the menu view.
		/// </summary>
		void ShowMenu();

		/// <summary>
		/// Open the SlidingMenu and show the secondary (right) menu view. Will default to the regular menu
		/// if there is only one.
		/// </summary>
		void ShowSecondaryMenu();

		/// <summary>
		/// Controls whether the ActionBar slides along with the above view when the menu is opened,
		/// or if it stays in place.
		/// </summary>
		/// <param name="slidingActionBarEnabled"> True if you want the ActionBar to slide along with the SlidingMenu,
		/// false if you want the ActionBar to stay in place </param>
		bool SlidingActionBarEnabled { set; }
	}
}