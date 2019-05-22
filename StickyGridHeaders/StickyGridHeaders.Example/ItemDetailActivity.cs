using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;

namespace StickyGridHeaders.Example
{
    /// <summary>
    /// An activity representing a single Item detail screen. This activity is only
    /// used on handset devices. On tablet-size devices, item details are presented
    /// side-by-side with a list of items in a <seealso cref="ItemListActivity"/>.
    /// <p>
    /// This activity is mostly just a 'shell' activity containing nothing more than
    /// a <seealso cref="ItemDetailFragment"/>.
    /// 
    /// @author Tonic Artos
    /// </summary>
    [Activity(Label = "ItemDetailActivity")]
    
    public class ItemDetailActivity : ActionBarActivity
    {
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    // This ID represents the Home or Up button. In the case of this
                    // activity, the Up button is shown. Use NavUtils to allow users
                    // to navigate up one level in the application structure. For
                    // more details, see the Navigation pattern on Android Design:
                    //
                    // http://developer.android.com/design/patterns/navigation.html#up-vs-back
                    //
                    NavUtils.NavigateUpTo(this, new Intent(this, typeof(ItemListActivity)));
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_item_detail);

			// Show the Up button in the action bar.
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);

			// savedInstanceState is non-null when there is fragment state
			// saved from previous configurations of this activity
			// (e.g. when rotating the screen from portrait to landscape).
			// In this case, the fragment will automatically be re-added
			// to its container so we don't need to manually add it.
			// For more information, see the Fragments API guide at:
			//
			// http://developer.android.com/guide/components/fragments.html
			//
			if (savedInstanceState == null)
			{
				// Create the detail fragment and add it to the activity
				// using a fragment transaction.
				Bundle arguments = new Bundle();
				arguments.PutInt(ItemDetailFragment.ARG_ITEM_ID, Intent.GetIntExtra(ItemDetailFragment.ARG_ITEM_ID, 0));
				ItemDetailFragment fragment = new ItemDetailFragment();
				fragment.Arguments = arguments;
				SupportFragmentManager.BeginTransaction().Add(Resource.Id.item_detail_container, fragment).Commit();
			}
		}
    }
}