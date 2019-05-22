using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace StickyGridHeaders.Example
{
    /// <summary>
    /// An activity representing a list of Items. This activity has different
    /// presentations for handset and tablet-size devices. On handsets, the activity
    /// presents a list of items, which when touched, lead to a
    /// <seealso cref="ItemDetailActivity"/> representing item details. On tablets, the
    /// activity presents the list of items and item details side-by-side using two
    /// vertical panes.
    /// <p>
    /// The activity makes heavy use of fragments. The list of items is a
    /// <seealso cref="ItemListFragment"/> and the item details (if present) is a
    /// <seealso cref="ItemDetailFragment"/>.
    /// <p>
    /// This activity also implements the required <seealso cref="ItemListFragment.Callbacks"/>
    /// interface to listen for item selections.
    /// 
    /// @author Tonic Artos
    /// </summary>
    [Activity(Label = "ItemListActivity", MainLauncher = true)]
    public class ItemListActivity : ActionBarActivity, ItemListFragment.Callbacks
    {
        /// <summary>
        /// Whether or not the activity is in two-pane mode, i.e. running on a tablet
        /// device.
        /// </summary>
        private bool mTwoPane;

        /// <summary>
        /// Callback method from <seealso cref="ItemListFragment.Callbacks"/> indicating that
        /// the item with the given ID was selected.
        /// </summary>
        public void OnItemSelected(int id)
        {
            if (mTwoPane)
            {
                // In two-pane mode, show the detail view in this activity by
                // adding or replacing the detail fragment using a
                // fragment transaction.
                Bundle arguments = new Bundle();
                arguments.PutInt(ItemDetailFragment.ARG_ITEM_ID, id);
                ItemDetailFragment fragment = new ItemDetailFragment();
                fragment.Arguments = arguments;
                SupportFragmentManager.BeginTransaction().Replace(Resource.Id.item_detail_container, fragment).Commit();
            }
            else
            {
                // In single-pane mode, simply start the detail activity
                // for the selected item ID.
                Intent detailIntent = new Intent(this, typeof(ItemDetailActivity));
                detailIntent.PutExtra(ItemDetailFragment.ARG_ITEM_ID, id);
                StartActivity(detailIntent);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_item_list);

			if (FindViewById(Resource.Id.item_detail_container) != null)
			{
				// The detail container view will be present only in the
				// large-screen layouts (res/values-large and
				// res/values-sw600dp). If this view is present, then the
				// activity should be in two-pane mode.
				mTwoPane = true;

				// In two-pane mode, list items should be given the
				// 'activated' state when touched.
				((ItemListFragment) SupportFragmentManager.FindFragmentById(Resource.Id.item_list)).ActivateOnItemClick = true;
			}
			// TODO: If exposing deep links into your app, handle intents here.
		}
    }
}