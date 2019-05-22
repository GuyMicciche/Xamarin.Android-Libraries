using Android.App;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

using Fragment = Android.Support.V4.App.Fragment;

namespace AndroidStaggeredGrid.Example
{
    [Activity(Label="StaggeredGridActivityFragment")]
    public class StaggeredGridActivityFragment : FragmentActivity
    {

        private const string TAG = "StaggeredGridActivityFragment";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "SGV";

            // Create the list fragment and add it as our sole content.
            if (SupportFragmentManager.FindFragmentById(Android.Resource.Id.Content) == null)
            {
                StaggeredGridFragment fragment = new StaggeredGridFragment(this);
                SupportFragmentManager.BeginTransaction().Add(Android.Resource.Id.Content, fragment).Commit();
            }
        }

        private class StaggeredGridFragment : Fragment, AbsListView.IOnScrollListener, AbsListView.IOnItemClickListener
        {
            private readonly StaggeredGridActivityFragment activityFragment;

            public StaggeredGridFragment(StaggeredGridActivityFragment activityFragment)
            {
                this.activityFragment = activityFragment;
            }

            private StaggeredGridView MGridView;
            private bool MHasRequestedMore;
            private SampleAdapter MAdapter;

            private List<string> MData;

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                RetainInstance = true;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                return inflater.Inflate(Resource.Layout.activity_sgv, container, false);
            }

            public override void OnActivityCreated(Bundle savedInstanceState)
            {
                base.OnActivityCreated(savedInstanceState);

                MGridView = (StaggeredGridView)View.FindViewById(Resource.Id.grid_view);

                if (savedInstanceState == null)
                {
                    LayoutInflater layoutInflater = Activity.LayoutInflater;

                    View header = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
                    View footer = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
                    TextView txtHeaderTitle = (TextView)header.FindViewById(Resource.Id.txt_title);
                    TextView txtFooterTitle = (TextView)footer.FindViewById(Resource.Id.txt_title);
                    txtHeaderTitle.Text = "THE HEADER!";
                    txtFooterTitle.Text = "THE FOOTER!";

                    MGridView.AddHeaderView(header);
                    MGridView.AddFooterView(footer);
                }

                if (MAdapter == null)
                {
                    MAdapter = new SampleAdapter(Activity, Resource.Id.txt_line1);
                }

                if (MData == null)
                {
                    MData = SampleData.GenerateSampleData();
                }

                foreach (string data in MData)
                {
                    MAdapter.Add(data);
                }

                MGridView.Adapter = MAdapter;
                MGridView.OnScrollListener = this;
                MGridView.OnItemClickListener = this;
            }

            public void OnScrollStateChanged(AbsListView view, ScrollState scrollState)
            {
                Console.WriteLine(TAG, "onScrollStateChanged:" + scrollState);
            }

            public void OnScroll(AbsListView view, int firstVisibleItem, int visibleItemCount, int totalItemCount)
            {
                Console.WriteLine(TAG, "onScroll firstVisibleItem:" + firstVisibleItem + " visibleItemCount:" + visibleItemCount + " totalItemCount:" + totalItemCount);
                // our handling
                if (!MHasRequestedMore)
                {
                    int lastInScreen = firstVisibleItem + visibleItemCount;
                    if (lastInScreen >= totalItemCount)
                    {
                        Console.WriteLine(TAG, "onScroll lastInScreen - so load more");
                        MHasRequestedMore = true;
                        OnLoadMoreItems();
                    }
                }
            }

            protected virtual void OnLoadMoreItems()
            {
                List<string> sampleData = SampleData.GenerateSampleData();
                foreach (string data in sampleData)
                {
                    MAdapter.Add(data);
                }
                // stash all the data in our backing store
                MData.AddRange(sampleData);
                // notify the adapter that we can update now
                MAdapter.NotifyDataSetChanged();
                MHasRequestedMore = false;
            }

            public void OnItemClick(AdapterView adapterView, View view, int position, long id)
            {
                Toast.MakeText(Activity, "Item Clicked: " + position, ToastLength.Short).Show();
            }
        }
    }
}