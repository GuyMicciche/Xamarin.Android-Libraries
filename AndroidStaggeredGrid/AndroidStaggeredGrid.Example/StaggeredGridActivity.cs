using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidStaggeredGrid.Example
{
    [Activity(Label = "StaggeredGridActivity")]
    public class StaggeredGridActivity : Activity, AbsListView.IOnScrollListener, AbsListView.IOnItemClickListener, AdapterView.IOnItemLongClickListener
    {
        private const string TAG = "StaggeredGridActivity";
        public const string SAVED_DATA_KEY = "SAVED_DATA";

        private StaggeredGridView mGridView;
        private bool mHasRequestedMore;
        private SampleAdapter mAdapter;

        private List<string> mData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_sgv);

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
            mAdapter = new SampleAdapter(this, Resource.Id.txt_line1);

            // do we have saved data?
            if (savedInstanceState != null)
            {
                mData = savedInstanceState.GetStringArrayList(SAVED_DATA_KEY).ToList();
            }

            if (mData == null)
            {
                mData = SampleData.GenerateSampleData();
            }

            foreach (string data in mData)
            {
                mAdapter.Add(data);
            }

            mGridView.Adapter = mAdapter;
            mGridView.OnScrollListener = this;
            mGridView.OnItemClickListener = this;
            mGridView.OnItemLongClickListener = this;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_sgv_dynamic, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.col1:
                    mGridView.ColumnCount = 1;
                    break;
                case Resource.Id.col2:
                    mGridView.ColumnCount = 2;
                    break;
                case Resource.Id.col3:
                    mGridView.ColumnCount = 3;
                    break;
            }

            return true;
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutStringArrayList(SAVED_DATA_KEY, mData);
        }

        public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
        {
            Console.WriteLine(TAG, "onScrollStateChanged:" + scrollState);
        }

        public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
        {
            Console.WriteLine(TAG, "onScroll firstVisibleItem:" + firstVisibleItem + " visibleItemCount:" + visibleItemCount + " totalItemCount:" + totalItemCount);
            // our handling
            if (!mHasRequestedMore)
            {
                int lastInScreen = firstVisibleItem + visibleItemCount;
                if (lastInScreen >= totalItemCount)
                {
                    Console.WriteLine(TAG, "onScroll lastInScreen - so load more");
                    mHasRequestedMore = true;
                    OnLoadMoreItems();
                }
            }
        }

        private void OnLoadMoreItems()
        {
            List<string> sampleData = SampleData.GenerateSampleData();
            foreach (string data in sampleData)
            {
                mAdapter.Add(data);
            }
            // stash all the data in our backing store
            mData.AddRange(sampleData);
            // notify the adapter that we can update now
            mAdapter.NotifyDataSetChanged();
            mHasRequestedMore = false;
        }

        public void OnItemClick(AdapterView adapterView, View view, int position, long id)
        {
            Toast.MakeText(this, "Item Clicked: " + position, ToastLength.Short).Show();
        }

        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            Toast.MakeText(this, "Item Long Clicked: " + position, ToastLength.Short).Show();
            return true;
        }
    }
}