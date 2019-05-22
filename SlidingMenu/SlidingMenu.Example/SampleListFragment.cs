using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace Example
{
    public class SampleListFragment : ListFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.list, null);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            SampleAdapter adapter = new SampleAdapter(Activity);
            for (var i = 0; i < 20; i++)
            {
                adapter.Add(new SampleItem("Sample List", Android.Resource.Drawable.IcMenuSearch));
            }
            ListAdapter = adapter;
        }

        private class SampleItem
        {
            public string Tag { get; set; }
            public int IconRes { get; set; }
            public SampleItem (string tag, int iconRes)
            {
                this.Tag = tag;
                this.IconRes = iconRes;
            }
        }

        private class SampleAdapter : ArrayAdapter<SampleItem>
        {
            public SampleAdapter(Context context)
                : base(context, 0)
            {
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                if (null == convertView)
                {
                    convertView = LayoutInflater.From(Context).Inflate(Resource.Layout.row, null);
                }

                ImageView icon = convertView.FindViewById<ImageView>(Resource.Id.row_icon);
                icon.SetImageResource(GetItem(position).IconRes);
                TextView title = convertView.FindViewById<TextView>(Resource.Id.row_title);
                title.Text = GetItem(position).Tag;

                return convertView;
            }
        }
    }
}