using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace AndroidStaggeredGrid.Example
{
    [Activity(Label = "ListViewActivity")]
    public class ListViewActivity : Activity, AdapterView.IOnItemClickListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_list_view);

            Title = "ListView";

            ListView listView = FindViewById<ListView>(Resource.Id.list_view);

            LayoutInflater layoutInflater = LayoutInflater;

            View header = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
            View footer = layoutInflater.Inflate(Resource.Layout.list_item_header_footer, null);
            TextView txtHeaderTitle = (TextView)header.FindViewById(Resource.Id.txt_title);
            TextView txtFooterTitle = (TextView)footer.FindViewById(Resource.Id.txt_title);
            txtHeaderTitle.Text = "THE HEADER!";
            txtFooterTitle.Text = "THE FOOTER!";

            listView.AddHeaderView(header);
            listView.AddFooterView(footer);

            SampleAdapter adapter = new SampleAdapter(this, Resource.Id.txt_line1);
            listView.Adapter = adapter;
            listView.OnItemClickListener = this;

            IList<string> sampleData = SampleData.GenerateSampleData();
            foreach (string data in sampleData)
            {
                adapter.Add(data);
            }
        }

        public void OnItemClick(AdapterView adapterView, View view, int position, long id)
        {
            Toast.MakeText(this, "Item Clicked: " + position, ToastLength.Short).Show();
        }
    }
}