using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Java.Lang;
using System;
using System.Threading.Tasks;

namespace MaterialDesignLibrary.Example
{
    [Activity]
    public class ProgressActivity : Activity
	{
        internal Color backgroundColor = Color.ParseColor("#1E88E5");

		internal ProgressBarDeterminate progreesBarDeterminate;
		internal ProgressBarIndeterminateDeterminate progressBarIndeterminateDeterminate;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_progress);

            //FindViewById(Resource.Id.progressBarCircularIndetermininate).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.progressBarIndeterminate).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.progressBarIndeterminateDeterminate).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.progressDeterminate).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.slider).SetBackgroundColor(Color.Black);
            //FindViewById(Resource.Id.sliderNumber).SetBackgroundColor(Color.Black);

			progreesBarDeterminate = (ProgressBarDeterminate)FindViewById(Resource.Id.progressDeterminate);

			progressBarIndeterminateDeterminate = (ProgressBarIndeterminateDeterminate)FindViewById(Resource.Id.progressBarIndeterminateDeterminate);
		}
	}
}