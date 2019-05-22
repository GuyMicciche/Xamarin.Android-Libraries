using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using Xamarin.NineOldAndroids.Views;

namespace MaterialDesignLibrary
{
	public class Slider : CustomView
	{
		// Event when slider change value
		public interface IOnValueChangedListener
		{
			void OnValueChanged(int value);
		}

		internal Color backgroundColor = Color.ParseColor("#4CAF50");

		internal Ball ball;
		internal NumberIndicator numberIndicator;

		internal bool showNumberIndicator = false;
		internal bool press = false;

		internal int val = 0;
		internal int max = 100;
		internal int min = 0;

		internal IOnValueChangedListener onValueChangedListener;

		public Slider(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
				SetBackgroundResource(Resource.Drawable.background_transparent);    
				// Set size of view
				SetMinimumHeight(Utils.DpToPx(48, Resources));
				SetMinimumWidth(Utils.DpToPx(80, Resources));
    
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
                    string bg = value.GetAttributeValue(ANDROIDXML, "background");
                    if (bg != null)
					{
						SetBackgroundColor(Color.ParseColor(bg));
					}
				}
    
				showNumberIndicator = value.GetAttributeBooleanValue(MATERIALDESIGNXML, "showNumberIndicator", false);
				min = value.GetAttributeIntValue(MATERIALDESIGNXML, "min", 0);
				max = value.GetAttributeIntValue(MATERIALDESIGNXML, "max", 0);
                val = value.GetAttributeIntValue(MATERIALDESIGNXML, "value", min);
    
				ball = new Ball(this, Context);
				RelativeLayout.LayoutParams lp = new LayoutParams(Utils.DpToPx(20, Resources), Utils.DpToPx(20, Resources));
				lp.AddRule(LayoutRules.CenterVertical, (int)LayoutRules.True);
				ball.LayoutParameters = lp;
				AddView(ball);
    
				// Set if slider content number indicator
				// TODO
				if (showNumberIndicator)
				{
					numberIndicator = new NumberIndicator(this, Context);
				}
    
			}
		}

		public override void Invalidate()
		{
			ball.Invalidate();
			base.Invalidate();
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (!placedBall)
			{
				PlaceBall();
			}

            if (val == min)
			{
                try
                {
				    // Crop line to transparent effect
				    Bitmap bitmap = Bitmap.CreateBitmap(canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
				    Canvas temp = new Canvas(bitmap);
				    Paint paint = new Paint();
				    paint.Color = Color.ParseColor("#B0B0B0");
				    paint.StrokeWidth = Utils.DpToPx(2, Resources);
				    temp.DrawLine(Height / 2, Height / 2, Width - Height / 2, Height / 2, paint);
				    Paint transparentPaint = new Paint();
				    transparentPaint.Color = Resources.GetColor(Android.Resource.Color.Transparent);
				    transparentPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));
				    temp.DrawCircle(ViewHelper.GetX(ball) + ball.Width / 2, ViewHelper.GetY(ball) + ball.Height / 2, ball.Width / 2, transparentPaint);

				    canvas.DrawBitmap(bitmap, 0, 0, new Paint());
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

			}
			else
			{
				Paint paint = new Paint();
				paint.Color = Color.ParseColor("#B0B0B0");
				paint.StrokeWidth = Utils.DpToPx(2, Resources);
				canvas.DrawLine(Height / 2, Height / 2, Width - Height / 2, Height / 2, paint);
				paint.Color = backgroundColor;
				float division = (ball.xFin - ball.xIni) / (max - min);
                int newVal = this.val - min;
                canvas.DrawLine(Height / 2, Height / 2, newVal * division + Height / 2, Height / 2, paint);
			}

			if (press && !showNumberIndicator)
			{
				Paint paint = new Paint();
				paint.Color = backgroundColor;
				paint.AntiAlias = true;
				canvas.DrawCircle(ViewHelper.GetX(ball) + ball.Width / 2, Height / 2, Height / 3, paint);
			}
			Invalidate();
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			isLastTouch = true;
			if (Enabled)
			{
				if (e.Action == MotionEventActions.Down || e.Action == MotionEventActions.Move)
				{
					if (numberIndicator != null && numberIndicator.IsShowing == false)
					{
						numberIndicator.Show();
					}
					if ((e.GetX() <= Width && e.GetX() >= 0))
					{
						press = true;
						// calculate value
						int newValue = 0;
						float division = (ball.xFin - ball.xIni) / (max - min);
						if (e.GetX() > ball.xFin)
						{
							newValue = max;
						}
						else if (e.GetX() < ball.xIni)
						{
							newValue = min;
						}
						else
						{
							newValue = min + (int)((e.GetX() - ball.xIni) / division);
						}
                        if (val != newValue)
						{
                            val = newValue;
							if (onValueChangedListener != null)
							{
								onValueChangedListener.OnValueChanged(newValue);
							}
						}
						// move ball indicator
						float x = e.GetX();
						x = (x < ball.xIni) ? ball.xIni : x;
						x = (x > ball.xFin) ? ball.xFin : x;
						ViewHelper.SetX(ball, x);
						ball.ChangeBackground();

						// If slider has number indicator
						if (numberIndicator != null)
						{
							// move number indicator
							numberIndicator.indicator.x = x;
							numberIndicator.indicator.finalY = Utils.GetRelativeTop(this) - Height / 2;
							numberIndicator.indicator.finalSize = Height / 2;
							numberIndicator.numberIndicator.Text = "";
						}
					}
					else
					{
						press = false;
						isLastTouch = false;
						if (numberIndicator != null)
						{
							numberIndicator.Dismiss();
						}
					}
				}
				else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
				{
					if (numberIndicator != null)
					{
						numberIndicator.Dismiss();
					}
					isLastTouch = false;
					press = false;
				}
			}
			return true;
		}

		/// <summary>
		/// Make a dark color to press effect
		/// 
		/// @return
		/// </summary>
		protected internal virtual int MakePressColor()
		{
			int r = (this.backgroundColor >> 16) & 0xFF;
			int g = (this.backgroundColor >> 8) & 0xFF;
			int b = (this.backgroundColor >> 0) & 0xFF;
			r = (r - 30 < 0) ? 0 : r - 30;
			g = (g - 30 < 0) ? 0 : g - 30;
			b = (b - 30 < 0) ? 0 : b - 30;
			return Color.Argb(70, r, g, b);
		}

		private void PlaceBall()
		{
			ViewHelper.SetX(ball, Height / 2 - ball.Width / 2);
			ball.xIni = ViewHelper.GetX(ball);
			ball.xFin = Width - Height / 2 - ball.Width / 2;
			ball.xCen = Width / 2 - ball.Width / 2;
			placedBall = true;
		}

		// GETERS & SETTERS
		public virtual IOnValueChangedListener OnValueChangedListener
		{
			get
			{
				return onValueChangedListener;
			}
			set
			{
				this.onValueChangedListener = value;
			}
		}

		public virtual int Value
		{
			get
			{
                return val;
			}
			set
			{
				if (placedBall == false)
				{
                    Post(() =>
                        {
                            Value = value;
                        });
				}
				else
				{
                    this.val = value;
					float division = (ball.xFin - ball.xIni) / max;
					ViewHelper.SetX(ball, value * division + Height / 2 - ball.Width / 2);
					ball.ChangeBackground();
				}
    
			}
		}

		public virtual int Max
		{
			get
			{
				return max;
			}
			set
			{
				this.max = value;
			}
		}

		public virtual int Min
		{
			get
			{
				return min;
			}
			set
			{
				this.min = value;
			}
		}

		public virtual bool ShowNumberIndicator
		{
			get
			{
				return showNumberIndicator;
			}
			set
			{
				this.showNumberIndicator = value;
				numberIndicator = (value) ? new NumberIndicator(this, Context) : null;
			}
		}

        public override void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (Enabled)
            {
                beforeBackground = backgroundColor;
            }

        }

		internal bool placedBall = false;

		internal class Ball : View
		{
			private readonly Slider slider;

			internal float xIni, xFin, xCen;

            public Ball(Slider slider, Context context) 
                : base(context)
			{
                this.slider = slider;
				SetBackgroundResource(Resource.Drawable.background_switch_ball_uncheck);
			}

			public virtual void ChangeBackground()
			{
                if (slider.val != slider.min)
				{
                    SetBackgroundResource(Resource.Drawable.background_checkbox);
					LayerDrawable layer = (LayerDrawable) Background;
					GradientDrawable shape = (GradientDrawable) layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
					shape.SetColor(slider.backgroundColor);
				}
				else
				{
                    SetBackgroundResource(Resource.Drawable.background_switch_ball_uncheck);
				}
			}
		}

		// Slider Number Indicator
		internal class NumberIndicator : Android.App.Dialog
		{
			private readonly Slider slider;

			internal Indicator indicator;
			internal TextView numberIndicator;

            public NumberIndicator(Slider slider, Context context)
                : base(context, Android.Resource.Style.ThemeTranslucent)
			{
                this.slider = slider;
			}

			protected override void OnCreate(Bundle savedInstanceState)
			{
				RequestWindowFeature((int)WindowFeatures.NoTitle);
				base.OnCreate(savedInstanceState);
				SetContentView(Resource.Layout.number_indicator_spinner);
				SetCanceledOnTouchOutside(false);

				RelativeLayout content = (RelativeLayout) this.FindViewById(Resource.Id.number_indicator_spinner_content);
				indicator = new Indicator(slider, this.Context);
				content.AddView(indicator);

				numberIndicator = new TextView(Context);
				numberIndicator.SetTextColor(Color.White);
				numberIndicator.Gravity = GravityFlags.Center;
				content.AddView(numberIndicator);

				indicator.LayoutParameters = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.FillParent, RelativeLayout.LayoutParams.FillParent);
			}

			public override void Dismiss()
			{
				base.Dismiss();
				indicator.y = 0;
				indicator.size = 0;
				indicator.animate = true;
			}

			public override void OnBackPressed()
			{
			}
		}

		internal class Indicator : RelativeLayout
		{
			private readonly Slider slider;

			// Position of number indicator
			internal float x = 0;
			internal float y = 0;
			// Size of number indicator
			internal float size = 0;

			// Final y position after animation
			internal float finalY = 0;
			// Final size after animation
			internal float finalSize = 0;

			internal bool animate = true;

			internal bool numberIndicatorResize = false;

            public Indicator(Slider slider, Context context)
                : base(context)
			{
                this.slider = slider;
                slider.SetBackgroundColor(Resources.GetColor(Android.Resource.Color.Transparent));
			}

			protected override void OnDraw(Canvas canvas)
			{
				base.OnDraw(canvas);

				if (numberIndicatorResize == false)
				{
					RelativeLayout.LayoutParams lp = (LayoutParams) slider.numberIndicator.numberIndicator.LayoutParameters;
					lp.Height = (int) finalSize * 2;
					lp.Width = (int) finalSize * 2;
					slider.numberIndicator.numberIndicator.LayoutParameters = lp;
				}

				Paint paint = new Paint();
				paint.AntiAlias = true;
				paint.Color = slider.backgroundColor;
				if (animate)
				{
					if (y == 0)
					{
						y = finalY + finalSize * 2;
					}
					y -= Utils.DpToPx(6, Resources);
					size += Utils.DpToPx(2, Resources);
				}
				canvas.DrawCircle(ViewHelper.GetX(slider.ball) + Utils.GetRelativeLeft((View) slider.ball.Parent) + slider.ball.Width / 2, y, size, paint);
				if (animate && size >= finalSize)
				{
					animate = false;
				}
				if (animate == false)
				{
					ViewHelper.SetX(slider.numberIndicator.numberIndicator, (ViewHelper.GetX(slider.ball) + Utils.GetRelativeLeft((View) slider.ball.Parent) + slider.ball.Width / 2) - size);
					ViewHelper.SetY(slider.numberIndicator.numberIndicator, y - size);
                    slider.numberIndicator.numberIndicator.Text = slider.val.ToString() + "";
				}

				slider.Invalidate();
			}
		}
	}
}