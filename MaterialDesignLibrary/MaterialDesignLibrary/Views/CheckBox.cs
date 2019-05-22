using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class CheckBox : CustomView
	{
		internal Color backgroundColor = Color.ParseColor("#4CAF50");

		internal Check checkView;

		internal bool press = false;
		internal bool check = false;

		internal IOnCheckListener onCheckListener;

		public CheckBox(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
    
				SetBackgroundResource(Resource.Drawable.background_checkbox);
    
				// Set size of view
                SetMinimumHeight(Utils.DpToPx(48, Resources));
                SetMinimumWidth(Utils.DpToPx(48, Resources));
    
				// Set background Color
				// Color by resource
				int bacgroundColor = value.GetAttributeResourceValue(ANDROIDXML, "background", -1);
				if (bacgroundColor != -1)
				{
					SetBackgroundColor(Resources.GetColor(bacgroundColor));
				}
				else
				{
					// Color by hexadecimal
					// Color by hexadecimal
					string background = value.GetAttributeValue(ANDROIDXML, "background");
					if (background != null)
					{
                        SetBackgroundColor(Color.ParseColor(background));
					}
				}
    
				bool check = value.GetAttributeBooleanValue(MATERIALDESIGNXML, "check", false);
                Post(() =>
                    {
                        IsChecked = check;
                        Pressed = false;
                        ChangeBackgroundColor(Resources.GetColor(Android.Resource.Color.Transparent));
                    });
    
				checkView = new Check(this, Context);
				RelativeLayout.LayoutParams lp = new LayoutParams(Utils.DpToPx(20, Resources), Utils.DpToPx(20, Resources));
				lp.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
				checkView.LayoutParameters = lp;
				AddView(checkView);
    
			}
		}

		public override void Invalidate()
		{
			checkView.Invalidate();
			base.Invalidate();
		}


		public override bool OnTouchEvent(MotionEvent e)
		{
			Invalidate();
			if (Enabled)
			{
				isLastTouch = true;
				if (e.Action == MotionEventActions.Down)
				{
					ChangeBackgroundColor((check) ? MakePressColor() : Color.ParseColor("#446D6D6D"));
				}
				else if (e.Action == MotionEventActions.Up)
				{
					ChangeBackgroundColor(Resources.GetColor(Android.Resource.Color.Transparent));
					press = false;
                    if ((e.GetX() <= Width && e.GetX() >= 0) && (e.GetY() <= Height && e.GetY() >= 0))
					{
						isLastTouch = false;
						check = !check;
						if (onCheckListener != null)
						{
							onCheckListener.OnCheck(check);
						}
						if (check)
						{
							step = 0;
						}
						if (check)
						{
							checkView.ChangeBackground();
						}
					}
				}
			}
			return true;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (press)
			{
				Paint paint = new Paint();
				paint.AntiAlias = true;
				paint.Color = (check) ? MakePressColor() : Color.ParseColor("#446D6D6D");
				canvas.DrawCircle(Width / 2, Height / 2, Width / 2, paint);
				Invalidate();
			}
		}

		private void ChangeBackgroundColor(int color)
		{
			LayerDrawable layer = (LayerDrawable) Background;
			GradientDrawable shape = (GradientDrawable) layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
			shape.SetColor(color);
		}

		/// <summary>
		/// Make a dark color to press effect
		/// 
		/// @return
		/// </summary>
		protected internal virtual Color MakePressColor()
		{
			int r = (this.backgroundColor >> 16) & 0xFF;
			int g = (this.backgroundColor >> 8) & 0xFF;
			int b = (this.backgroundColor >> 0) & 0xFF;
			r = (r - 30 < 0) ? 0 : r - 30;
			g = (g - 30 < 0) ? 0 : g - 30;
			b = (b - 30 < 0) ? 0 : b - 30;

			return Color.Argb(70, r, g, b);
		}

        public override void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (Enabled)
            {
                beforeBackground = backgroundColor;
            }
            ChangeBackgroundColor(color);
        }

		public virtual bool IsChecked
		{
			set
			{
				Invalidate();
				this.check = value;
				Pressed = false;
				ChangeBackgroundColor(Resources.GetColor(Android.Resource.Color.Transparent));
				if (value)
				{
					step = 0;
				}
				if (value)
				{
					checkView.ChangeBackground();
				}
			}
            get
            {
                return check;
            }
		}

		// Indicate step in check animation
		internal int step = 0;

		// View that contains checkbox
		internal class Check : View
		{
			private readonly CheckBox checkbox;

			internal Bitmap sprite;

            public Check(CheckBox checkbox, Context context)
                : base(context)
			{
                this.checkbox = checkbox;
				SetBackgroundResource(Resource.Drawable.background_checkbox_uncheck);
				sprite = BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.sprite_check);
			}

			public virtual void ChangeBackground()
			{
				if (checkbox.check)
				{
                    SetBackgroundResource(Resource.Drawable.background_checkbox_check);
					LayerDrawable layer = (LayerDrawable) Background;
					GradientDrawable shape = (GradientDrawable) layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
					shape.SetColor(checkbox.backgroundColor);
				}
				else
				{
                    SetBackgroundResource(Resource.Drawable.background_checkbox_uncheck);
				}
			}

			protected override void OnDraw(Canvas canvas)
			{
				base.OnDraw(canvas);

				if (checkbox.check)
				{
					if (checkbox.step < 11)
					{
						checkbox.step++;
						checkbox.Invalidate();
					}
				}
				else
				{
					if (checkbox.step >= 0)
					{
						checkbox.step--;
						checkbox.Invalidate();
					}
					if (checkbox.step == -1)
					{
						checkbox.Invalidate();
						ChangeBackground();
					}
				}
				Rect src = new Rect(40 * checkbox.step, 0, (40 * checkbox.step) + 40, 40);
				Rect dst = new Rect(0, 0, this.Width - 2, this.Height);
				canvas.DrawBitmap(sprite, src, dst, null);

			}

		}

		public virtual IOnCheckListener OncheckListener
		{
			set
			{
				this.onCheckListener = value;
			}
		}

		public interface IOnCheckListener
		{
			void OnCheck(bool check);
		}
	}
}