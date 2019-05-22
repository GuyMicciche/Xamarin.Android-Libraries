using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class ProgressDialog : Android.App.Dialog
	{
		internal Context context;
		internal View view;
		internal View backView;
		internal string title;
		internal TextView titleTextView;

        internal Color progressColor;

		public ProgressDialog(Context context, string title)
            : base(context, Android.Resource.Style.ThemeTranslucent)
		{
			this.title = title;
			this.context = context;
		}

		public ProgressDialog(Context context, string title, Color progressColor)
            : base(context, Android.Resource.Style.ThemeTranslucent)
		{
			this.title = title;
			this.progressColor = progressColor;
			this.context = context;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature((int)WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.progress_dialog);

			view = (RelativeLayout)FindViewById(Resource.Id.contentDialog);
			backView = (RelativeLayout)FindViewById(Resource.Id.dialog_rootView);
            backView.Touch += (sender, e) =>
            {
                if (e.Event.GetX() < view.Left || e.Event.GetX() > view.Right || e.Event.GetY() > view.Bottom || e.Event.GetY() < view.Top)
                {
                    Dismiss();
                }
            };

			this.titleTextView = (TextView) FindViewById(Resource.Id.title);
			Title = title;
			if (progressColor != null)
			{
				ProgressBarCircularIndeterminate progressBarCircularIndeterminate = (ProgressBarCircularIndeterminate) FindViewById(Resource.Id.progressBarCircularIndetermininate);
				progressBarCircularIndeterminate.BackgroundColor = progressColor;
			}
		}

        public override void Show()
		{
			base.Show();
			// set dialog enter animations
			view.StartAnimation(AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_main_show_amination));
            backView.StartAnimation(AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_root_show_amin));
		}

		// GETERS & SETTERS
		public virtual string Title
		{
			get
			{
				return title;
			}
			set
			{
				this.title = value;
				if (value == null)
				{
					titleTextView.Visibility = ViewStates.Gone;
				}
				else
				{
					titleTextView.Visibility = ViewStates.Visible;
					titleTextView.Text = value;
				}
			}
		}

		public virtual TextView TitleTextView
		{
			get
			{
				return titleTextView;
			}
			set
			{
				this.titleTextView = value;
			}
		}

		public override void Dismiss()
		{
			Animation anim = AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_main_hide_amination);
            anim.AnimationEnd += (sender, e) =>
            {
                view.Post(() => base.Dismiss());
            };

			Animation backAnim = AnimationUtils.LoadAnimation(context, Resource.Animation.dialog_root_hide_amin);

			view.StartAnimation(anim);
			backView.StartAnimation(backAnim);
		}
	}
}