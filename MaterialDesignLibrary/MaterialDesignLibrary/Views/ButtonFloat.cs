using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views.Animations;
using Android.Widget;

using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views;

namespace MaterialDesignLibrary
{
	public class ButtonFloat : Button
	{
		internal int sizeIcon = 24;
		internal int sizeRadius = 28;

		internal ImageView icon; // Icon of float button
		internal Drawable drawableIcon;

		public bool isShow = false;

		internal float showPosition;
		internal float hidePosition;

		public ButtonFloat(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			SetBackgroundResource(Resource.Drawable.background_button_float);
			sizeRadius = 28;
			SetDefaultProperties();
			icon = new ImageView(context);
			icon.SetAdjustViewBounds(true);
			icon.SetScaleType(ImageView.ScaleType.CenterCrop);
			if (drawableIcon != null)
			{
				icon.SetImageDrawable(drawableIcon);
			}
			RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(Utils.DpToPx(sizeIcon, Resources),Utils.DpToPx(sizeIcon, Resources));
            lp.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
            icon.LayoutParameters = lp;
			AddView(icon);

		}

		protected internal override void SetDefaultProperties()
		{
			rippleSpeed = Utils.DpToPx(2, Resources);
			rippleSize = Utils.DpToPx(5, Resources);
			SetMinimumWidth(Utils.DpToPx(sizeRadius * 2, Resources));
			SetMinimumHeight(Utils.DpToPx(sizeRadius * 2, Resources));
			base.background = Resource.Drawable.background_button_float;
		}

		// Set atributtes of XML to View
		protected internal override IAttributeSet Attributes
		{
			set
			{
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
                    string bg = value.GetAttributeValue(ANDROIDXML, "rippleColor");
					if (bg != null)
					{
						RippleColor = Color.ParseColor(bg);
					}
					else
					{
						RippleColor = MakePressColor();
					}
				}
				// Icon of button
				int iconResource = value.GetAttributeResourceValue(MATERIALDESIGNXML,"iconDrawable",-1);
				if (iconResource != -1)
				{
					drawableIcon = Resources.GetDrawable(iconResource);
				}
				bool animate = value.GetAttributeBooleanValue(MATERIALDESIGNXML,"animate", false);
				Post(() =>
                {
                    showPosition = ViewHelper.GetY(this) - Utils.DpToPx(24, Resources);
				    hidePosition = ViewHelper.GetY(this) + Height * 3;
				    if (animate)
				    {
					    ViewHelper.SetY(this, hidePosition);
					    Show();
				    }   
                });
    
			}
		}

		internal int height;
		internal int width;

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (x != -1)
			{
				Rect src = new Rect(0, 0, Width, Height);
				Rect dst = new Rect(Utils.DpToPx(1, Resources), Utils.DpToPx(2, Resources), Width - Utils.DpToPx(1, Resources), Height - Utils.DpToPx(2, Resources));
				canvas.DrawBitmap(CropCircle(MakeCircle()), src, dst, null);
				Invalidate();
			}
		}

		public virtual ImageView Icon
		{
			get
			{
				return icon;
			}
			set
			{
				this.icon = value;
			}
		}


		public virtual Drawable DrawableIcon
		{
			get
			{
				return drawableIcon;
			}
			set
			{
				this.drawableIcon = value;
				try
				{
					icon.Background = value;
				}
				catch (System.MissingMethodException e)
				{
					icon.SetBackgroundDrawable(value);
				}
			}
		}


		public virtual Bitmap CropCircle(Bitmap bitmap)
		{
            Bitmap output = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(output);

			const string color = "#ff424242";
			Paint paint = new Paint();
			Rect rect = new Rect(0, 0, bitmap.Width, bitmap.Height);

			paint.AntiAlias = true;
			canvas.DrawARGB(0, 0, 0, 0);
			paint.Color = Color.ParseColor(color);
			canvas.DrawCircle(bitmap.Width / 2, bitmap.Height / 2, bitmap.Width / 2, paint);
			paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
			canvas.DrawBitmap(bitmap, rect, rect, paint);
			return output;
		}

		public override TextView TextView
		{
			get
			{
				return null;
			}
		}

		public virtual Color RippleColor
		{
			set
			{
				this.rippleColor = value;
			}
		}

		public virtual void Show()
		{
			ObjectAnimator animator = ObjectAnimator.OfFloat(this, "y", showPosition);
			animator.SetInterpolator(new BounceInterpolator());
			animator.SetDuration(1500);
			animator.Start();
			isShow = true;
		}

		public virtual void Hide()
		{
			ObjectAnimator animator = ObjectAnimator.OfFloat(this, "y", hidePosition);
            animator.SetInterpolator(new BounceInterpolator());
            animator.SetDuration(1500);
			animator.Start();

			isShow = false;
		}

		public virtual bool IsShown
		{
			get
			{
				return isShow;
			}
		}
	}
}