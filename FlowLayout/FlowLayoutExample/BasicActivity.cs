using Android.App;
using Android.OS;
namespace FlowLayout.Example
{
    [Activity(Label = "BasicActivity")]
    public class BasicActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_basic);
		}
	}
}