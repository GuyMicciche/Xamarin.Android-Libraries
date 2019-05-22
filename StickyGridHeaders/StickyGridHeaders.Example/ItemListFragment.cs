using Android.Annotation;
using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Fragment = Android.Support.V4.App.Fragment;

namespace StickyGridHeaders.Example
{
    /// <summary>
    /// A list fragment representing a list of Items. This fragment also supports
    /// tablet devices by allowing list items to be given an 'activated' state upon
    /// selection. This helps indicate which item is currently being viewed in a
    /// <seealso cref="ItemDetailFragment"/>.
    /// <p>
    /// Activities containing this fragment MUST implement the <seealso cref="Callbacks"/>
    /// interface.
    /// 
    /// @author Tonic Artos
    /// </summary>
    public class ItemListFragment : Fragment, AdapterView.IOnItemClickListener, StickyGridHeadersGridView.IOnHeaderClickListener, StickyGridHeadersGridView.IOnHeaderLongClickListener
    {
        private const string KEY_LIST_POSITION = "key_list_position";

        /// <summary>
        /// A dummy implementation of the <seealso cref="Callbacks"/> interface that does
        /// nothing. Used only when this fragment is not attached to an activity.
        /// </summary>
        private static Callbacks sDummyCallbacks = new CallbacksAnonymousInnerClassHelper();

        private class CallbacksAnonymousInnerClassHelper : Callbacks
        {
            public CallbacksAnonymousInnerClassHelper()
            {
            }

            public virtual void OnItemSelected(int id)
            {
            }
        }

        /// <summary>
        /// The serialization (saved instance state) Bundle key representing the
        /// activated item position. Only used on tablets.
        /// </summary>
        private const string STATE_ACTIVATED_POSITION = "activated_position";
        /// <summary>
        /// The current activated item position. Only used on tablets.
        /// </summary>
        private int mActivatedPosition = ListView.InvalidPosition;
        /// <summary>
        /// The fragment's current callback object, which is notified of list item
        /// clicks.
        /// </summary>
        private Callbacks mCallbacks = sDummyCallbacks;

        private int mFirstVisible;

        private GridView mGridView;

        private IMenu mMenu;

        private Toast mToast;

        /// <summary>
        /// Mandatory empty constructor for the fragment manager to instantiate the
        /// fragment (e.g. upon screen orientation changes).
        /// </summary>
        public ItemListFragment()
        {
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            // Activities containing this fragment must implement its callbacks.
            if (!(activity is Callbacks))
            {
                throw new IllegalStateException("Activity must implement fragment's callbacks.");
            }

            mCallbacks = (Callbacks)activity;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.fragment_item_list, menu);
            mMenu = menu;
            menu.FindItem(Resource.Id.menu_toggle_sticky).SetChecked(((StickyGridHeadersGridView)mGridView).AreHeadersSticky);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.fragment_item_grid, container, false);
        }

        public override void OnDetach()
        {
            base.OnDetach();

            // Reset the active callbacks interface to the dummy implementation.
            mCallbacks = sDummyCallbacks;
        }

        public void OnHeaderClick(AdapterView parent, View view, long id)
        {
            string text = "Header " + ((TextView)view.FindViewById(Android.Resource.Id.Text1)).Text + " was tapped.";
            if (mToast == null)
            {
                mToast = Toast.MakeText(Activity, text, ToastLength.Short);
            }
            else
            {
                mToast.SetText(text);
            }
            mToast.Show();
        }

        public bool OnHeaderLongClick(AdapterView parent, View view, long id)
        {
            string text = "Header " + ((TextView)view.FindViewById(Android.Resource.Id.Text1)).Text + " was long pressed.";
            if (mToast == null)
            {
                mToast = Toast.MakeText(Activity, text, ToastLength.Short);
            }
            else
            {
                mToast.SetText(text);
            }
            mToast.Show();
            return true;
        }

        public void OnItemClick(AdapterView gridView, View view, int position, long id)
        {
            // Notify the active callbacks interface (the activity, if the
            // fragment is attached to one) that an item has been selected.
            mCallbacks.OnItemSelected(position);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_toggle_sticky:
                    item.SetChecked(!item.IsChecked);
                    ((StickyGridHeadersGridView)mGridView).AreHeadersSticky = !((StickyGridHeadersGridView)mGridView).AreHeadersSticky;
                    return true;
                case Resource.Id.menu_use_list_adapter:
                    mGridView.Adapter = new ArrayAdapter<string>(Activity, Resource.Layout.item, Resources.GetStringArray(Resource.Array.countries));
                    mMenu.FindItem(Resource.Id.menu_use_list_adapter).SetVisible(true);
                    mMenu.FindItem(Resource.Id.menu_use_sticky_adapter).SetVisible(true);
                    mMenu.FindItem(Resource.Id.menu_toggle_sticky).SetVisible(false);
                    return true;
                case Resource.Id.menu_use_sticky_adapter:
                    mGridView.Adapter = new StickyGridHeadersSimpleArrayAdapter<string>(Activity.ApplicationContext, Resources.GetStringArray(Resource.Array.countries), Resource.Layout.header, Resource.Layout.item);
                    mMenu.FindItem(Resource.Id.menu_use_list_adapter).SetVisible(true);
                    mMenu.FindItem(Resource.Id.menu_toggle_sticky).SetVisible(true);
                    mMenu.FindItem(Resource.Id.menu_use_sticky_adapter).SetVisible(false);
                    return true;

                default:
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (mActivatedPosition != ListView.InvalidPosition)
            {
                // Serialize and persist the activated item position.
                outState.PutInt(STATE_ACTIVATED_POSITION, mActivatedPosition);
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            mGridView = (GridView)view.FindViewById(Resource.Id.asset_grid);
            mGridView.OnItemClickListener = this;

            /*
             * Currently set in the XML layout, but this is how you would do it in
             * your code.
             */
            // mGridView.setColumnWidth((int) calculatePixelsFromDips(100));
            // mGridView.setNumColumns(StickyGridHeadersGridView.AUTO_FIT);
            mGridView.Adapter = new StickyGridHeadersSimpleArrayAdapter<string>(Activity.ApplicationContext, Resources.GetStringArray(Resource.Array.countries), Resource.Layout.header, Resource.Layout.item);

            if (savedInstanceState != null)
            {
                mFirstVisible = savedInstanceState.GetInt(KEY_LIST_POSITION);
            }

            mGridView.SetSelection(mFirstVisible);

            // Restore the previously serialized activated item position.
            if (savedInstanceState != null && savedInstanceState.ContainsKey(STATE_ACTIVATED_POSITION))
            {
                ActivatedPosition = savedInstanceState.GetInt(STATE_ACTIVATED_POSITION);
            }

            ((StickyGridHeadersGridView)mGridView).OnHeaderClickListener = this;
            ((StickyGridHeadersGridView)mGridView).OnHeaderLongClickListener = this;

            HasOptionsMenu = true;
        }

        /// <summary>
        /// Turns on activate-on-click mode. When this mode is on, list items will be
        /// given the 'activated' state when touched.
        /// </summary>
        [TargetApi(Value = (int)BuildVersionCodes.Honeycomb)]
        public bool ActivateOnItemClick
        {
            set
            {
                // When setting CHOICE_MODE_SINGLE, ListView will automatically
                // give items the 'activated' state when touched.

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                {
                    mGridView.ChoiceMode = value ? (ChoiceMode)ListView.ChoiceModeSingle : (ChoiceMode)ListView.ChoiceModeNone;
                }
            }
        }

        private int ActivatedPosition
        {
            set
            {
                if (value == ListView.InvalidPosition)
                {
                    mGridView.SetItemChecked(mActivatedPosition, false);
                }
                else
                {
                    mGridView.SetItemChecked(value, true);
                }

                mActivatedPosition = value;
            }
        }

        /// <summary>
        /// A callback interface that all activities containing this fragment must
        /// implement. This mechanism allows activities to be notified of item
        /// selections.
        /// </summary>
        public interface Callbacks
        {
            /// <summary>
            /// Callback for when an item has been selected.
            /// </summary>
            void OnItemSelected(int position);
        }
    }
}