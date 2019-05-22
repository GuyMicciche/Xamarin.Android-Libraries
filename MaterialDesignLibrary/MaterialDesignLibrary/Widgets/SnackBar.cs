using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Java.IO;
using Java.Lang;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaterialDesignLibrary
{
	public class SnackBar : Android.App.Dialog
	{
		internal string text;
		internal float textSize = 14; //Roboto Regular 14sp
		internal string buttonText;
        internal View.IOnClickListener onClickListener;
        internal EventHandler clickHandler;
		internal Activity activity;
		internal View view;
		internal ButtonFlat button;
		internal Color backgroundSnackBar = Color.ParseColor("#333333");
		internal Color backgroundButton = Color.ParseColor("#1E88E5");

		internal IOnHideListener onHideListener;
		// Timer
		private bool mIndeterminate = false;
		private static int mTimer = 3 * 1000;

		// With action button
		public SnackBar(Activity activity, string text, string buttonText, View.IOnClickListener onClickListener)
            : base(activity, Android.Resource.Style.ThemeTranslucent)
		{
			this.activity = activity;
			this.text = text;
			this.buttonText = buttonText;
			this.onClickListener = onClickListener;
		}

        public SnackBar(Activity activity, string text, string buttonText, EventHandler handler)
            : base(activity, Android.Resource.Style.ThemeTranslucent)
        {
            this.activity = activity;
            this.text = text;
            this.buttonText = buttonText;
            this.clickHandler = handler;
        }

		// Only text
		public SnackBar(Activity activity, string text)
            : base(activity, Android.Resource.Style.ThemeTranslucent)
		{
			this.activity = activity;
			this.text = text;
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.snackbar);
			SetCanceledOnTouchOutside(false);
			((TextView)FindViewById(Resource.Id.text)).Text = text;
			((TextView)FindViewById(Resource.Id.text)).TextSize = textSize; //set textSize
			button = (ButtonFlat) FindViewById(Resource.Id.buttonflat);
            if (text == null || (onClickListener == null && clickHandler == null))
			{
				button.Visibility = ViewStates.Gone;
			}
			else
			{
				button.Text = buttonText;
                button.SetBackgroundColor(backgroundButton);

                if (onClickListener != null)
                {
                    button.Click += (sender, e) =>
                    {
                        Dismiss();
                        onClickListener.OnClick((View)sender);
                    };
                }
                if(clickHandler != null)
                {
                    button.Click += clickHandler;
                    button.Click += (sender, e) =>
                    {
                        Dismiss();
                    };
                }
			}
			view = FindViewById(Resource.Id.snackbar);
			view.SetBackgroundColor(backgroundSnackBar);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			return activity.DispatchTouchEvent(e);
		}

		public override void OnBackPressed()
		{
		}

		public override async void Show()
		{
			base.Show();
			view.Visibility = ViewStates.Visible;
			view.StartAnimation(AnimationUtils.LoadAnimation(activity, Resource.Animation.snackbar_show_animation));
			if (!mIndeterminate)
			{
                //try
                //{
                //    System.Threading.Thread.Sleep(mTimer);
                //}
                //catch (InterruptedException e)
                //{
                //    System.Console.WriteLine(e.ToString());
                //    System.Console.Write(e.StackTrace);
                //}
                
                await Task.Delay(mTimer);

				activity.RunOnUiThread(() =>
                {
                    if (onHideListener != null)
                    {
                        onHideListener.OnHide();
                    }
                    Dismiss();
                });
			}
		}

		public override void Dismiss()
		{
			Animation anim = AnimationUtils.LoadAnimation(activity, Resource.Animation.snackbar_hide_animation);
            anim.AnimationEnd += (sender, e) =>
                {
                    base.Dismiss();
                };
			view.StartAnimation(anim);
		}

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
		{
			 if (keyCode == Keycode.Back)
			 {
				 Dismiss();
			 }
			return base.OnKeyDown(keyCode, e);
		}

		public virtual float MessageTextSize
		{
			set
			{
				textSize = value;
			}
		}

		public virtual bool Indeterminate
		{
			set
			{
					mIndeterminate = value;
			}
			get
			{
				return mIndeterminate;
			}
		}

		public virtual int DismissTimer
		{
			set
			{
				mTimer = value;
			}
			get
			{
				return mTimer;
			}
		}

		/// <summary>
		/// Change background color of SnackBar </summary>
		/// <param name="color"> </param>
        public virtual Color BackgroundSnackBar
		{
			set
			{
				backgroundSnackBar = value;
				if (view != null)
				{
					view.SetBackgroundColor(value);
				}
			}
		}

		/// <summary>
		/// Chage color of FlatButton in Snackbar </summary>
		/// <param name="color"> </param>
		public virtual Color ColorButton
		{
			set
			{
				backgroundButton = value;
				if (button != null)
				{
					button.BackgroundColor = value;
				}
			}
		}

		/// <summary>
		/// This event start when snackbar dismish without push the button
		/// @author Navas
		/// 
		/// </summary>
		public interface IOnHideListener
		{
			void OnHide();
		}

		public virtual IOnHideListener OnhideListener
		{
			set
			{
				this.onHideListener = value;
			}
		}
	}
}