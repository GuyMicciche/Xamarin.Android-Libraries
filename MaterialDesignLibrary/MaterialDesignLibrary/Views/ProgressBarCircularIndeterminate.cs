using Android.Content;
using Android.Graphics;
using Android.Util;
namespace MaterialDesignLibrary
{
	public class ProgressBarCircularIndeterminate : CustomView
	{
		internal new const string ANDROIDXML = "http://schemas.android.com/apk/res/android";

		internal Color backgroundColor = Color.ParseColor("#1E88E5");

		public ProgressBarCircularIndeterminate(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
				SetMinimumHeight(Utils.DpToPx(32, Resources));
				SetMinimumWidth(Utils.DpToPx(32, Resources));
    
				//Set background Color
				// Color by resource
				int bacgroundColor = value.GetAttributeResourceValue(ANDROIDXML,"background",-1);
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
						BackgroundColor = Color.ParseColor("#1E88E5");
					}
				}
    
				SetMinimumHeight(Utils.DpToPx(3, Resources));
			}
		}

		/// <summary>
		/// Make a dark color to ripple effect
		/// @return
		/// </summary>
		protected internal virtual Color MakePressColor()
		{
			int r = (this.backgroundColor >> 16) & 0xFF;
			int g = (this.backgroundColor >> 8) & 0xFF;
			int b = (this.backgroundColor >> 0) & 0xFF;
	//		r = (r+90 > 245) ? 245 : r+90;
	//		g = (g+90 > 245) ? 245 : g+90;
	//		b = (b+90 > 245) ? 245 : b+90;
			return Color.Argb(128,r, g, b);
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (firstAnimationOver == false)
			{
				DrawFirstAnimation(canvas);
			}
			if (cont > 0)
			{
				DrawSecondAnimation(canvas);
			}
			Invalidate();
		}

		internal float radius1 = 0;
		internal float radius2 = 0;
		internal int cont = 0;
		internal bool firstAnimationOver = false;

		/// <summary>
		/// Draw first animation of view </summary>
		/// <param name="canvas"> </param>
		private void DrawFirstAnimation(Canvas canvas)
		{
			if (radius1 < Width / 2)
			{
				Paint paint = new Paint();
				paint.AntiAlias = true;
				paint.Color = MakePressColor();
				radius1 = (radius1 >= Width / 2)? (float)Width / 2 : radius1 + 1;
				canvas.DrawCircle(Width / 2, Height / 2, radius1, paint);
			}
			else
			{
                Bitmap bitmap = Bitmap.CreateBitmap(canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
				Canvas temp = new Canvas(bitmap);
				Paint paint = new Paint();
				paint.AntiAlias = true;
				paint.Color = MakePressColor();
				temp.DrawCircle(Width / 2, Height / 2, Height / 2, paint);
				Paint transparentPaint = new Paint();
				transparentPaint.AntiAlias = true;
				transparentPaint.Color = Resources.GetColor(Android.Resource.Color.Transparent);
				transparentPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));
				if (cont >= 50)
				{
					radius2 = (radius2 >= Width / 2)? (float)Width / 2 : radius2 + 1;
				}
				else
				{
					radius2 = (radius2 >= Width / 2 - Utils.DpToPx(4, Resources))? (float)Width / 2 - Utils.DpToPx(4, Resources) : radius2 + 1;
				}
				temp.DrawCircle(Width / 2, Height / 2, radius2, transparentPaint);
				canvas.DrawBitmap(bitmap, 0, 0, new Paint());
				if (radius2 >= Width / 2 - Utils.DpToPx(4, Resources))
				{
					cont++;
				}
				if (radius2 >= Width / 2)
				{
					firstAnimationOver = true;
				}
			}
		}

		internal int arcD = 1;
		internal int arcO = 0;
		internal float rotateAngle = 0;
		internal int limite = 0;

		/// <summary>
		/// Draw second animation of view </summary>
		/// <param name="canvas"> </param>
		private void DrawSecondAnimation(Canvas canvas)
		{
			if (arcO == limite)
			{
				arcD += 6;
			}
			if (arcD >= 290 || arcO > limite)
			{
				arcO += 6;
				arcD -= 6;
			}
			if (arcO > limite + 290)
			{
				limite = arcO;
				arcO = limite;
				arcD = 1;
			}
			rotateAngle += 4;
			canvas.Rotate(rotateAngle,Width / 2, Height / 2);

            Bitmap bitmap = Bitmap.CreateBitmap(canvas.Width, canvas.Height, Bitmap.Config.Argb8888);
			Canvas temp = new Canvas(bitmap);
			Paint paint = new Paint();
			paint.AntiAlias = true;
			paint.Color = backgroundColor;
	//		temp.drawARGB(0, 0, 0, 255);
			temp.DrawArc(new RectF(0, 0, Width, Height), arcO, arcD, true, paint);
			Paint transparentPaint = new Paint();
			transparentPaint.AntiAlias = true;
			transparentPaint.Color = Resources.GetColor(Android.Resource.Color.Transparent);
			transparentPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));
			temp.DrawCircle(Width / 2, Height / 2, (Width / 2) - Utils.DpToPx(4, Resources), transparentPaint);

			canvas.DrawBitmap(bitmap, 0, 0, new Paint());
		}

		// Set color of background
		public virtual Color BackgroundColor
		{
			set
			{
				base.SetBackgroundColor(Resources.GetColor(Android.Resource.Color.Transparent));
				if (Enabled)
				{
					beforeBackground = backgroundColor;
				}
				this.backgroundColor = value;
			}
		}
	}
}