using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views;

namespace MaterialDesignLibrary
{
	public class Switch : CustomView
	{

		internal Color backgroundColor = Color.ParseColor("#4CAF50");

		internal Ball ball;

		internal bool check = false;
		internal bool eventCheck = false;
		internal bool press = false;

		internal IOnCheckListener onCheckListener;

		public Switch(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Attributes = attrs;
            this.Click += (sender, e) =>
            {
                if (check)
                {
                    IsChecked = false;
                }
                else
                {
                    IsChecked = true;
                }
            };
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
    
				check = value.GetAttributeBooleanValue(MATERIALDESIGNXML, "check", false);
				eventCheck = check;
				ball = new Ball(this, Context);
				RelativeLayout.LayoutParams lp = new LayoutParams(Utils.DpToPx(20, Resources), Utils.DpToPx(20, Resources));
				lp.AddRule(LayoutRules.CenterVertical, (int)LayoutRules.True);
				ball.LayoutParameters = lp;
				AddView(ball);
			}
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			if (Enabled)
			{
				isLastTouch = true;
				if (e.Action == MotionEventActions.Down)
				{
					press = true;
				}
				else if (e.Action == MotionEventActions.Move)
				{
					float x = e.GetX();
					x = (x < ball.xIni) ? ball.xIni : x;
					x = (x > ball.xFin) ? ball.xFin : x;
					if (x > ball.xCen)
					{
						check = true;
					}
					else
					{
						check = false;
					}
					ViewHelper.SetX(ball, x);
					ball.ChangeBackground();
					if ((e.GetX() <= Width && e.GetX() >= 0))
					{
						isLastTouch = false;
						press = false;
					}
				}
				else if (e.Action == MotionEventActions.Up || e.Action == MotionEventActions.Cancel)
				{
					press = false;
					isLastTouch = false;
					if (eventCheck != check)
					{
						eventCheck = check;
						if (onCheckListener != null)
						{
							onCheckListener.OnCheck(check);
						}
					}
					if ((e.GetX() <= Width && e.GetX() >= 0))
					{
						ball.AnimateCheck();
					}
				}
			}
			return true;
		}

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!placedBall)
            {
                PlaceBall();
            }

            try
            {
                // Crop line to transparent effect
                Bitmap bitmap = Bitmap.CreateBitmap(canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
                Canvas temp = new Canvas(bitmap);
                Paint paint = new Paint();
                paint.AntiAlias = true;
                paint.Color = (check) ? backgroundColor : Color.ParseColor("#B0B0B0");
                paint.StrokeWidth = Utils.DpToPx(2, Resources);
                temp.DrawLine(Height / 2, Height / 2, Width - Height / 2, Height / 2, paint);
                Paint transparentPaint = new Paint();
                transparentPaint.AntiAlias = true;
                transparentPaint.Color = Resources.GetColor(Android.Resource.Color.Transparent);
                transparentPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));
                temp.DrawCircle(ViewHelper.GetX(ball) + ball.Width / 2, ViewHelper.GetY(ball) + ball.Height / 2, ball.Width / 2, transparentPaint);

                canvas.DrawBitmap(bitmap, 0, 0, new Paint());

                if (press)
                {
                    paint.Color = (check) ? MakePressColor() : Color.ParseColor("#446D6D6D");
                    canvas.DrawCircle(ViewHelper.GetX(ball) + ball.Width / 2, Height / 2, Height / 2, paint);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Invalidate();
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

		// Move ball to first position in view
		internal bool placedBall = false;

		private void PlaceBall()
		{
			ViewHelper.SetX(ball, Height / 2 - ball.Width / 2);
			ball.xIni = ViewHelper.GetX(ball);
			ball.xFin = Width - Height / 2 - ball.Width / 2;
			ball.xCen = Width / 2 - ball.Width / 2;
			placedBall = true;
			ball.AnimateCheck();
		}

		// SETTERS
        public override void SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (Enabled)
            {
                beforeBackground = backgroundColor;
            }
        }

		public virtual bool IsChecked
		{
			set
			{
				Invalidate();
				this.check = value;
				ball.AnimateCheck();
			}
		}

		public virtual bool Check
		{
			get
			{
				return check;
			}
		}

		internal class Ball : View
		{
			private readonly Switch outerInstance;

			internal float xIni, xFin, xCen;

			public Ball(Switch outerInstance, Context context)
                : base(context)
			{
				this.outerInstance = outerInstance;
				SetBackgroundResource(Resource.Drawable.background_switch_ball_uncheck);
			}

			public virtual void ChangeBackground()
			{
				if (outerInstance.check)
				{
                    SetBackgroundResource(Resource.Drawable.background_checkbox);
					LayerDrawable layer = (LayerDrawable) Background;
					GradientDrawable shape = (GradientDrawable) layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
					shape.SetColor(outerInstance.backgroundColor);
				}
				else
				{
                    SetBackgroundResource(Resource.Drawable.background_switch_ball_uncheck);
				}
			}

			public virtual void AnimateCheck()
			{
				ChangeBackground();
				ObjectAnimator objectAnimator;
				if (outerInstance.check)
				{
					objectAnimator = ObjectAnimator.OfFloat(this, "x", outerInstance.ball.xFin);

				}
				else
				{
					objectAnimator = ObjectAnimator.OfFloat(this, "x", outerInstance.ball.xIni);
				}
				objectAnimator.SetDuration(300);
				objectAnimator.Start();
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