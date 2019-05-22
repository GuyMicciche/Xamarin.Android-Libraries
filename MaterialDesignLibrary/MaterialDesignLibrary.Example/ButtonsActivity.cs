using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
namespace MaterialDesignLibrary.Example
{
    [Activity]
    public class ButtonsActivity : Activity
	{
		internal Color backgroundColor = Color.ParseColor("#1E88E5");

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_buttons);
            //FindViewById(Resource.Id.buttonflat).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.button).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.buttonFloatSmall).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.buttonIcon).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.buttonFloat).SetBackgroundColor(Color.Black);
		}
	}
}