using Android;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class ColorSelector : Android.App.Dialog, Slider.IOnValueChangedListener
	{

        internal Color color = Color.Black;
		internal Context context;
		internal View colorView;
		internal View view, backView; //background

		internal IOnColorSelectedListener onColorSelectedListener;
		internal Slider red, green, blue;

		public ColorSelector(Context context, Color color, IOnColorSelectedListener onColorSelectedListener) : base(context, Android.Resource.Style.ThemeTranslucent)
		{
			this.context = context;
			this.onColorSelectedListener = onColorSelectedListener;
			if (color != null)
			{
				this.color = color;
			}
            this.DismissEvent += (sender, e) =>
            {
                if (onColorSelectedListener != null)
                {
                    onColorSelectedListener.OnColorSelected(color);
                }
            };
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature((int)WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.color_selector);

			view = (LinearLayout)FindViewById(Resource.Id.contentSelector);
			backView = (RelativeLayout)FindViewById(Resource.Id.rootSelector);
            backView.Touch += (sender, e) =>
            {
                if (e.Event.GetX() < view.Left || e.Event.GetX() > view.Right || e.Event.GetY() > view.Bottom || e.Event.GetY() < view.Top)
                {
                    Dismiss();
                }
            };

			colorView = FindViewById(Resource.Id.viewColor);
			colorView.SetBackgroundColor(color);

			// Resize ColorView
            colorView.Post(() =>
            {
                LinearLayout.LayoutParams lp = (LinearLayout.LayoutParams)colorView.LayoutParameters;
                lp.Height = colorView.Width;
                colorView.LayoutParameters = lp;
            });

			// Configure Sliders
			red = (Slider)FindViewById(Resource.Id.red);
			green = (Slider)FindViewById(Resource.Id.green);
			blue = (Slider)FindViewById(Resource.Id.blue);

			int r = (this.color >> 16) & 0xFF;
			int g = (this.color >> 8) & 0xFF;
			int b = (this.color >> 0) & 0xFF;

			red.Value = r;
			green.Value = g;
			blue.Value = b;

			red.OnValueChangedListener = this;
			green.OnValueChangedListener = this;
			blue.OnValueChangedListener = this;
		}

		public override void Show()
		{
			base.Show();
			view.StartAnimation(AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_main_show_amination));
            backView.StartAnimation(AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_root_show_amin));
		}

        public void OnValueChanged(int value)
        {
            color = Color.Rgb(red.Value, green.Value, blue.Value);
            colorView.SetBackgroundColor(color);
        }

		// Event that execute when color selector is closed
		public interface IOnColorSelectedListener
		{
			void OnColorSelected(Color color);
		}

		public override void Dismiss()
		{
            Animation anim = AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_main_hide_amination);

            anim.AnimationEnd += (sender, e) =>
            {
                view.Post(() =>
                {
                    base.Dismiss();
                });
            };

			Animation backAnim = AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_root_hide_amin);

			view.StartAnimation(anim);
			backView.StartAnimation(backAnim);
		}
	}
}