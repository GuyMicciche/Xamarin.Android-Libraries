using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
namespace MaterialDesignLibrary
{
	public class LayoutRipple : CustomView
	{
		internal float rippleSpeed = 10f;
		internal int rippleSize = 3;

		internal IOnClickListener onClickListener;
		internal Color backgroundColor = Color.ParseColor("#FFFFFF");

		internal Color rippleColor;
		internal float xRippleOrigin;
		internal float yRippleOrigin;

		public LayoutRipple(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
				// Set background Color
				// Color by resource
				int bacgroundColor = value.GetAttributeResourceValue(ANDROIDXML, "background", -1);
				if (bacgroundColor != -1)
				{
					BackgroundColor = Resources.GetColor(bacgroundColor);
				}
				else
				{
					// Color by hexadecimal
                    string bg = value.GetAttributeValue(ANDROIDXML, "background");
                    if (bg != null)
					{
                        BackgroundColor = Color.ParseColor(bg);
					}
					else
					{
						BackgroundColor = this.backgroundColor;
					}
				}
				// Set Ripple Color
				// Color by resource
				int rippleColor = value.GetAttributeResourceValue(MATERIALDESIGNXML, "rippleColor", -1);
				if (rippleColor != -1)
				{
					RippleColor = Resources.GetColor(rippleColor);
				}
				else
				{
					// Color by hexadecimal
                    string bg = value.GetAttributeValue(MATERIALDESIGNXML, "rippleColor");
                    if (bg != null)
					{
						RippleColor = Color.ParseColor(bg);
					}
					else
					{
						RippleColor = MakePressColor();
					}
				}
    
				rippleSpeed = value.GetAttributeFloatValue(MATERIALDESIGNXML, "rippleSpeed", 20f);
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
				base.SetBackgroundColor(value);
			}
		}

		public virtual int RippleSpeed
		{
			set
			{
				this.rippleSpeed = value;
			}
		}

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
					}
					else
					{
						isLastTouch = false;
						x = -1;
						y = -1;
					}
				}
				if (e.Action == MotionEventActions.Cancel)
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
            Bitmap output = Bitmap.CreateBitmap(Width, Height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(output);
			canvas.DrawARGB(0, 0, 0, 0);
			Paint paint = new Paint();
			paint.AntiAlias = true;
			if (rippleColor == null)
			{
				rippleColor = MakePressColor();
			}
			paint.Color = rippleColor;
			x = (xRippleOrigin == null) ? x : xRippleOrigin;
			y = (yRippleOrigin == null) ? y : yRippleOrigin;
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
				if (onClickListener != null)
				{
					onClickListener.OnClick(this);
				}
			}
			return output;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

            try
            {
                if (x != -1)
                {
                    Rect src = new Rect(0, 0, Width, Height);
                    Rect dst = new Rect(0, 0, Width, Height);
                    canvas.DrawBitmap(MakeCircle(), src, dst, null);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Invalidate();
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

        public override void SetOnClickListener(View.IOnClickListener l)
        {
            onClickListener = l;
        }

		public virtual Color RippleColor
		{
			set
			{
				this.rippleColor = value;
			}
		}

		public virtual void SetxRippleOrigin(float xRippleOrigin)
		{
			this.xRippleOrigin = xRippleOrigin;
		}

		public virtual void SetyRippleOrigin(float yRippleOrigin)
		{
			this.yRippleOrigin = yRippleOrigin;
		}

	}

}