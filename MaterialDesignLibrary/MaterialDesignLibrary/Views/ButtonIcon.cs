using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;

namespace MaterialDesignLibrary
{
	public class ButtonIcon : ButtonFloat
	{
		public ButtonIcon(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			try
			{
				Background = new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent));
			}
			catch (System.MissingMethodException e)
			{
                SetBackgroundDrawable(new ColorDrawable(Resources.GetColor(Android.Resource.Color.Transparent)));
			}
			rippleSpeed = Utils.DpToPx(2, Resources);
			rippleSize = Utils.DpToPx(5, Resources);
		}

		public override bool OnTouchEvent(MotionEvent e)
		{
			bool returnBool = base.OnTouchEvent(e);
			if (x != -1)
			{
				x = Width / 2;
				y = Height / 2;
			}
			return returnBool;
		}

		protected override void OnDraw(Canvas canvas)
		{
			if (x != -1)
			{
				Paint paint = new Paint();
				paint.AntiAlias = true;
				paint.Color = MakePressColor();
				canvas.DrawCircle(x, y, radius, paint);
				if (radius > Height / rippleSize)
				{
					radius += rippleSpeed;
				}
				if (radius >= Width / 2 - rippleSpeed)
				{
					x = -1;
					y = -1;
					radius = Height / rippleSize;
					if (onClickListener != null && clickAfterRipple)
					{
						onClickListener.OnClick(this);
					}
				}
				Invalidate();
			}
		}

		protected internal override Color MakePressColor()
		{
			return backgroundColor;
		}
	}
}