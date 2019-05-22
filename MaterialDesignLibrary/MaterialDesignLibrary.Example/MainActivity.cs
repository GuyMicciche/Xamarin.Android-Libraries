using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;

using Xamarin.NineOldAndroids.Views;

namespace MaterialDesignLibrary.Example
{
    [Activity(Label = "MaterialDesignLibrary.Example", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity, ColorSelector.IOnColorSelectedListener
    {
        internal Color backgroundColor = Color.ParseColor("#1E88E5");
        internal ButtonFloatSmall buttonSelectColor;

        protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			buttonSelectColor = (ButtonFloatSmall) FindViewById(Resource.Id.buttonColorSelector);
            buttonSelectColor.Click += (sender, e) =>
                {
                    ColorSelector colorSelector = new ColorSelector(this, backgroundColor, this);
                    colorSelector.Show();
                };

			LayoutRipple layoutRipple = (LayoutRipple)FindViewById(Resource.Id.itemButtons);


			OriginRiple = layoutRipple;

            layoutRipple.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(ButtonsActivity));
                intent.PutExtra("BACKGROUND", backgroundColor);
                StartActivity(intent);
            };
			layoutRipple = (LayoutRipple) FindViewById(Resource.Id.itemSwitches);


			OriginRiple = layoutRipple;

            layoutRipple.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(SwitchActivity));
                intent.PutExtra("BACKGROUND", backgroundColor);
                StartActivity(intent);
            };
			layoutRipple = (LayoutRipple) FindViewById(Resource.Id.itemProgress);


			OriginRiple = layoutRipple;

            layoutRipple.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(ProgressActivity));
                intent.PutExtra("BACKGROUND", backgroundColor);
                StartActivity(intent);
            };
			layoutRipple = (LayoutRipple) FindViewById(Resource.Id.itemWidgets);


			OriginRiple = layoutRipple;

            layoutRipple.Click += (sender, e) =>
            {
                Intent intent = new Intent(this, typeof(WidgetActivity));
                intent.PutExtra("BACKGROUND", backgroundColor);
                StartActivity(intent);
            };
		}

        private LayoutRipple OriginRiple
        {
            set
            {

                value.Post(() =>
                {
                    View v = value.GetChildAt(0);
                    value.SetxRippleOrigin(ViewHelper.GetX(v) + v.Width / 2);
                    value.SetyRippleOrigin(ViewHelper.GetY(v) + v.Height / 2);

                    value.RippleColor = Color.ParseColor("#1E88E5");

                    value.RippleSpeed = 30;
                });
            }
        }

        public void OnColorSelected(Color color)
        {
            backgroundColor = color;
            buttonSelectColor.BackgroundColor = color;
        }
    }
}