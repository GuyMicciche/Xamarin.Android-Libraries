using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidStaggeredGrid;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidStaggeredGrid.Example
{
    [Activity(Label = "StaggeredGridEmptyViewActivity")]
    public class StaggeredGridEmptyViewActivity : Activity, AbsListView.IOnItemClickListener
    {

        public const string SAVED_DATA_KEY = "SAVED_DATA";
        private const int FETCH_DATA_TASK_DURATION = 2000;

        private StaggeredGridView mGridView;
        private SampleAdapter mAdapter;

        private List<string> mData;

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_sgv_empy_view);

			Title = "SGV";
			mGridView = (StaggeredGridView)FindViewById(Resource.Id.grid_view);

			LayoutInflater layoutInflater = LayoutInflater;

			View header = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
			View footer = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
			TextView txtHeaderTitle = (TextView)header.FindViewById(Resource.Id.txt_title);
            TextView txtFooterTitle = (TextView)footer.FindViewById(Resource.Id.txt_title);
			txtHeaderTitle.Text = "THE HEADER!";
			txtFooterTitle.Text = "THE FOOTER!";

			mGridView.AddHeaderView(header);
			mGridView.AddFooterView(footer);
			mGridView.EmptyView = FindViewById(Android.Resource.Id.Empty);
			mAdapter = new SampleAdapter(this, Resource.Id.txt_line1);

			// do we have saved data?
			if (savedInstanceState != null)
			{
				mData = savedInstanceState.GetStringArrayList(SAVED_DATA_KEY).ToList();
				FillAdapter();
			}

			if (mData == null)
			{
				mData = SampleData.GenerateSampleData();
			}


			mGridView.Adapter = mAdapter;

			mGridView.OnItemClickListener = this;

			FetchData();
		}

        private void FillAdapter()
        {
            foreach (string data in mData)
            {
                mAdapter.Add(data);
            }
        }

        private void FetchData()
        {
            ThreadPool.QueueUserWorkItem(state =>
                {
                    SystemClock.Sleep(FETCH_DATA_TASK_DURATION);

                    RunOnUiThread(() =>
                        {
                            FillAdapter();
                        });
                });
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.activity_sgv_empty_view, menu);

			return true;
		}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            mAdapter.Clear();
            FetchData();

            return true;
        }

        public void OnItemClick(AdapterView adapterView, View view, int position, long id)
        {
            Toast.MakeText(this, "Item Clicked: " + position, ToastLength.Short).Show();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutStringArrayList(SAVED_DATA_KEY, mData);
        }
    }
}