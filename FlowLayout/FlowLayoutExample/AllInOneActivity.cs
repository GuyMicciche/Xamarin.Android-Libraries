using Android.App;
using Android.OS;

namespace FlowLayout.Example
{
    [Activity(Label = "AllInOneActivity")]
	public class AllInOneActivity : Activity
	{
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_all_in_one);
        }
	}
}