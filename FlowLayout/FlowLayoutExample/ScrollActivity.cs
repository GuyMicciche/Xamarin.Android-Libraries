using Android.App;
using Android.OS;

namespace FlowLayout.Example
{
    [Activity(Label = "ScrollActivity")]
    public class ScrollActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_scroll);
		}
	}
}