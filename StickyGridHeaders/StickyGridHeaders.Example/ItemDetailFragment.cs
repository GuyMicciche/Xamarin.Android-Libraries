using Android.OS;
using Android.Views;
using Android.Widget;

using Fragment = Android.Support.V4.App.Fragment;

namespace StickyGridHeaders.Example
{
    /// <summary>
    /// A fragment representing a single Item detail screen. This fragment is either
    /// contained in a <seealso cref="ItemListActivity"/> in two-pane mode (on tablets) or a
    /// <seealso cref="ItemDetailActivity"/> on handsets.
    /// 
    /// @author Tonic Artos
    /// </summary>
    public class ItemDetailFragment : Fragment
    {
        /// <summary>
        /// The fragment argument representing the item ID that this fragment
        /// represents.
        /// </summary>
        public const string ARG_ITEM_ID = "item_id";

        /// <summary>
        /// The dummy content this fragment is presenting.
        /// </summary>
        private int mItem;

        /// <summary>
        /// Mandatory empty constructor for the fragment manager to instantiate the
        /// fragment (e.g. upon screen orientation changes).
        /// </summary>
        public ItemDetailFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Arguments.ContainsKey(ARG_ITEM_ID))
            {
                // Load the dummy content specified by the fragment
                // arguments. In a real-world scenario, use a Loader
                // to load content from a content provider.
                mItem = Arguments.GetInt(ARG_ITEM_ID);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View rootView = inflater.Inflate(Resource.Layout.fragment_item_detail, container, false);

            ((TextView)rootView.FindViewById(Resource.Id.item_detail)).Text = Resources.GetStringArray(Resource.Array.countries)[mItem];
            return rootView;
        }
    }
}