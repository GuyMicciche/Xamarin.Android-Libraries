using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace MaterialDesignLibrary
{
	public abstract class Button : CustomView
	{
		internal new const string ANDROIDXML = "http://schemas.android.com/apk/res/android";

		// Complete in child class
		internal int minWidth;
		internal int minHeight;
		internal int background;
		internal float rippleSpeed = 12f;
		internal int rippleSize = 3;
		internal Color rippleColor;
		internal View.IOnClickListener onClickListener;
		internal bool clickAfterRipple = true;
        internal Color backgroundColor = Color.ParseColor("#1E88E5");

		public Button(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			SetDefaultProperties();
			clickAfterRipple = attrs.GetAttributeBooleanValue(MATERIALDESIGNXML,"animate", true);
			Attributes = attrs;
			beforeBackground = backgroundColor;
			if (rippleColor == null)
			{
			    rippleColor = MakePressColor();
			}
		}

		protected internal virtual void SetDefaultProperties()
		{
			// Min size
			SetMinimumHeight(Utils.DpToPx(minHeight, Resources));
			SetMinimumWidth(Utils.DpToPx(minWidth, Resources));
			// Background shape
			SetBackgroundResource(background);
			BackgroundColor = backgroundColor;
		}


		// Set atributtes of XML to View
		protected internal abstract IAttributeSet Attributes { set; }

		// ### RIPPLE EFFECT ###

		internal float x = -1, y = -1;
		internal float radius = -1;

		public override bool OnTouchEvent(MotionEvent e)
		{
			Invalidate();
			if (Enabled)
			{
				isLastTouch = true;
				if (e.Action == MotionEventActions.Down)
				{
					radius = Height / rippleSize;
					x = e.GetX();
					y = e.GetY();
				}
                else if (e.Action == MotionEventActions.Move)
				{
					radius = Height / rippleSize;
                    x = e.GetX();
                    y = e.GetY();
                    if (!((e.GetX() <= Width && e.GetX() >= 0) && (e.GetY() <= Height && e.GetY() >= 0)))
					{
						isLastTouch = false;
						x = -1;
						y = -1;
					}
				}
                else if (e.Action == MotionEventActions.Up)
				{
                    if ((e.GetX() <= Width && e.GetX() >= 0) && (e.GetY() <= Height && e.GetY() >= 0))
					{
						radius++;
						if (!clickAfterRipple && onClickListener != null)
						{
							onClickListener.OnClick(this);
						}
					}
					else
					{
						isLastTouch = false;
						x = -1;
						y = -1;
					}
				}
                else if (e.Action == MotionEventActions.Cancel)
				{
						isLastTouch = false;
						x = -1;
						y = -1;
				}
			}
			return true;
		}

        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            if (!gainFocus)
            {
                x = -1;
                y = -1;
            }
        }

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			// super.onInterceptTouchEvent(ev);
			return true;
		}

		public virtual Bitmap MakeCircle()
		{
            Bitmap output = Bitmap.CreateBitmap(Width - Utils.DpToPx(6, Resources), Height - Utils.DpToPx(7, Resources), Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(output);
			canvas.DrawARGB(0, 0, 0, 0);
			Paint paint = new Paint();
			paint.AntiAlias = true;
			paint.Color = rippleColor;
			canvas.DrawCircle(x, y, radius, paint);
			if (radius > Height / rippleSize)
			{
				radius += rippleSpeed;
			}
			if (radius >= Width)
			{
				x = -1;
				y = -1;
				radius = Height / rippleSize;
				if (onClickListener != null && clickAfterRipple)
				{
					onClickListener.OnClick(this);
				}
			}
			return output;
		}

		/// <summary>
		/// Make a dark color to ripple effect
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

			return Color.Rgb(r, g, b);
		}

		public View.IOnClickListener OnClickListener
		{
			set
			{
				onClickListener = value;
			}
		}

		// Set color of background
		public virtual Color BackgroundColor
		{
			set
			{
				this.backgroundColor = value;
				if (Enabled)
				{
					beforeBackground = backgroundColor;
				}
				try
				{
					LayerDrawable layer = (LayerDrawable)Background;
					GradientDrawable shape = (GradientDrawable)layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
					shape.SetColor(backgroundColor);
					rippleColor = MakePressColor();
				}
				catch (Exception ex)
				{
					// Without bacground
				}
			}
		}

		public abstract TextView TextView { get; }

		public virtual float RippleSpeed
		{
			set
			{
				this.rippleSpeed = value;
			}
			get
			{
				return this.rippleSpeed;
			}
		}

	}

}