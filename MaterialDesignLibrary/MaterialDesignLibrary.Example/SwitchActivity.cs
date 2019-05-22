using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
namespace MaterialDesignLibrary.Example
{
    [Activity]
    public class SwitchActivity : Activity
	{
		internal Color backgroundColor = Color.ParseColor("#1E88E5");

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_switchs);
			//FindViewById(Resource.Id.checkBox).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.switchView).SetBackgroundColor(Color.Black);
		}
	}
}