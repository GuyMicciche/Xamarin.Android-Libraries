using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;

namespace AndroidStaggeredGrid.Example
{
    [Activity(Label="MainActivity", MainLauncher=true)]
    public class MainActivity : Activity, View.IOnClickListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Title = "SGV Sample";
            SetContentView(Resource.Layout.activity_main);

            FindViewById(Resource.Id.btn_sgv).SetOnClickListener(this);
            FindViewById(Resource.Id.btn_sgv_fragment).SetOnClickListener(this);
            FindViewById(Resource.Id.btn_sgv_empty_view).SetOnClickListener(this);
            FindViewById(Resource.Id.btn_listview).SetOnClickListener(this);
        }


        public void OnClick(View v)
        {
            if (v.Id == Resource.Id.btn_sgv)
            {
                StartActivity(new Intent(this, typeof(StaggeredGridActivity)));
            }
            else if (v.Id == Resource.Id.btn_sgv_fragment)
            {
                StartActivity(new Intent(this, typeof(StaggeredGridActivityFragment)));
            }
            else if (v.Id == Resource.Id.btn_sgv_empty_view)
            {
                StartActivity(new Intent(this, typeof(StaggeredGridEmptyViewActivity)));
            }
            else if (v.Id == Resource.Id.btn_listview)
            {
                StartActivity(new Intent(this, typeof(ListViewActivity)));
            }
        }
    }
}