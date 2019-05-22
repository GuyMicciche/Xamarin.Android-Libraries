using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;

using MaterialDesignLibrary;
using System;

namespace MaterialDesignLibrary.Example
{
    [Activity]
    public class WidgetActivity : Activity
	{
		private int backgroundColor = Color.ParseColor("#1E88E5");

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_widgets);

	        // SHOW SNACKBAR
            FindViewById<ButtonFlat>(Resource.Id.buttonSnackBar).OnClickListener = new StupidClick1(this);
	        // SHOW DiALOG
            FindViewById<ButtonFlat>(Resource.Id.buttonDialog).OnClickListener = new StupidClick2(this);
	        // SHOW COLOR SEECTOR
            FindViewById<ButtonFlat>(Resource.Id.buttonColorSelector).OnClickListener = new StupidClick3(this);
		}

        private class StupidClick1 : Java.Lang.Object, View.IOnClickListener
        {
            private readonly WidgetActivity activity;

            public StupidClick1(WidgetActivity activity)
            {
                this.activity = activity;
            }

            public void OnClick(View v)
            {
                SnackBar snack = new SnackBar(activity, "Do you want change color of this button to red?", "yes", delegate
                {
                    ButtonFlat btn = (ButtonFlat)activity.FindViewById(Resource.Id.buttonSnackBar);
                    btn.SetTextColor(Color.Red);
                });
                snack.Show();
            }
        }

        private class StupidClick2 : Java.Lang.Object, View.IOnClickListener
        {
            private readonly WidgetActivity activity;

            public StupidClick2(WidgetActivity activity)
            {
                this.activity = activity;
            }

            public void OnClick(View v)
            {
                Console.WriteLine("Hello!");
                Dialog dialog = new Dialog(activity, "Title", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam");
                dialog.OnAcceptButtonClickListener = new AcceptButtonListener(activity);
                dialog.OnCancelButtonClickListener = new CancelButtonListener(activity);
                dialog.Show();
            }
        }

        private class StupidClick3 : Java.Lang.Object, View.IOnClickListener
        {
            private readonly WidgetActivity activity;

            public StupidClick3(WidgetActivity activity)
            {
                this.activity = activity;
            }

            public void OnClick(View v)
            {
                (new ColorSelector(activity, Color.Red, null)).Show();
            }
        }

        private class AcceptButtonListener : Java.Lang.Object, View.IOnClickListener
        {
            private readonly WidgetActivity activity;

            public AcceptButtonListener(WidgetActivity activity)
            {
                this.activity = activity;
            }

            public void OnClick(View v)
            {
                Toast.MakeText(activity, "Click accept button", ToastLength.Long).Show();
            }
        }

        private class CancelButtonListener : Java.Lang.Object, View.IOnClickListener
        {
            private readonly WidgetActivity activity;

            public CancelButtonListener(WidgetActivity activity)
            {
                this.activity = activity;
            }

            public void OnClick(View v)
            {
                Toast.MakeText(activity, "Click cancel button", ToastLength.Long).Show();
            }
        }
	}
}