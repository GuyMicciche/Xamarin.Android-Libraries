using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class Dialog : Android.App.Dialog
	{
		internal Context context;
		internal View view;
		internal View backView;
		internal string message;
		internal TextView messageTextView;
		internal string title;
		internal TextView titleTextView;

		internal ButtonFlat buttonAccept;
		internal ButtonFlat buttonCancel;

		internal string buttonCancelText;

		internal View.IOnClickListener onAcceptButtonClickListener;
		internal View.IOnClickListener onCancelButtonClickListener;

		public Dialog(Context context, string title, string message) 
            : base(context, Android.Resource.Style.ThemeTranslucent)
		{
			this.context = context;
			this.message = message;
			this.title = title;
		}

		public virtual void AddCancelButton(string buttonCancelText)
		{
			this.buttonCancelText = buttonCancelText;
		}

		public virtual void AddCancelButton(string buttonCancelText, View.IOnClickListener onCancelButtonClickListener)
		{
			this.buttonCancelText = buttonCancelText;
			this.onCancelButtonClickListener = onCancelButtonClickListener;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			RequestWindowFeature((int)WindowFeatures.NoTitle);
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.dialog);

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

			this.messageTextView = (TextView) FindViewById(Resource.Id.message);
			Message = message;

			this.buttonAccept = (ButtonFlat)FindViewById(Resource.Id.button_accept);
            buttonAccept.Click += (sender, e) =>
                {
                    Dismiss();
                    if (onAcceptButtonClickListener != null)
                    {
                        onAcceptButtonClickListener.OnClick((View)sender);
                    }
                };

			if (buttonCancelText != null)
			{
				this.buttonCancel = (ButtonFlat) FindViewById(Resource.Id.button_cancel);
				this.buttonCancel.Visibility = ViewStates.Visible;
				this.buttonCancel.Text = buttonCancelText;
                buttonCancel.Click += (sender, e) =>
                    {
                        Dismiss();
                        if (onCancelButtonClickListener != null)
                        {
                            onCancelButtonClickListener.OnClick((View)sender);
                        }
                    };
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
		public virtual string Message
		{
			get
			{
				return message;
			}
			set
			{
				this.message = value;
				messageTextView.Text = value;
			}
		}

		public virtual TextView MessageTextView
		{
			get
			{
				return messageTextView;
			}
			set
			{
				this.messageTextView = value;
			}
		}

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

		public virtual ButtonFlat ButtonAccept
		{
			get
			{
				return buttonAccept;
			}
			set
			{
				this.buttonAccept = value;
			}
		}

		public virtual ButtonFlat ButtonCancel
		{
			get
			{
				return buttonCancel;
			}
			set
			{
				this.buttonCancel = value;
			}
		}

		public virtual View.IOnClickListener OnAcceptButtonClickListener
		{
			set
			{
				this.onAcceptButtonClickListener = value;
				if (buttonAccept != null)
				{
					buttonAccept.OnClickListener = value;
				}
			}
		}

		public virtual View.IOnClickListener OnCancelButtonClickListener
		{
			set
			{
				this.onCancelButtonClickListener = value;
				if (buttonCancel != null)
				{
					buttonCancel.OnClickListener = value;
				}
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