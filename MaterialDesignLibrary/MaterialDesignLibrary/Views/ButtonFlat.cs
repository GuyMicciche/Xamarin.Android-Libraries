using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class ButtonFlat : Button
	{
		internal TextView textButton;

		public ButtonFlat(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{

		}

		protected internal override void SetDefaultProperties()
		{
			minHeight = 36;
			minWidth = 88;
			rippleSize = 3;
			// Min size
			SetMinimumHeight(Utils.DpToPx(minHeight, Resources));
            SetMinimumWidth(Utils.DpToPx(minWidth, Resources));
            SetBackgroundResource(Resource.Drawable.background_transparent);
		}

		protected internal override IAttributeSet Attributes
		{
			set
			{
				// Set text button
				string text = null;
				int textResource = value.GetAttributeResourceValue(ANDROIDXML,"text",-1);
				if (textResource != -1)
				{
					text = Resources.GetString(textResource);
				}
				else
				{
					text = value.GetAttributeValue(ANDROIDXML,"text");
				}
				if (text != null)
				{
					textButton = new TextView(Context);
					textButton.Text = text.ToUpper();
					textButton.SetTextColor(backgroundColor);
					textButton.SetTypeface(null, TypefaceStyle.Bold);
                    RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
                    lp.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
                    textButton.LayoutParameters = lp;
					AddView(textButton);
				}
				int bacgroundColor = value.GetAttributeResourceValue(ANDROIDXML,"background",-1);
				if (bacgroundColor != -1)
				{
					BackgroundColor = Resources.GetColor(bacgroundColor);
				}
				else
				{
					// Color by hexadecimal
					// Color by hexadecimal
                    string bg = value.GetAttributeValue(ANDROIDXML, "background");
                    if (bg != null)
					{
                        BackgroundColor = Color.ParseColor(bg);
					}
				}
			}
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
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
				Invalidate();
			}
		}

		/// <summary>
		/// Make a dark color to ripple effect
		/// @return
		/// </summary>
		protected internal override Color MakePressColor()
		{
			return Color.ParseColor("#88DDDDDD");
		}

		public virtual string Text
		{
			set
			{
				textButton.Text = value.ToUpper();
			}
			get
			{
					return textButton.Text.ToString();
			}
		}

		// Set color of background
		public override Color BackgroundColor
		{
			set
			{
				backgroundColor = value;
				if (Enabled)
				{
					beforeBackground = backgroundColor;
				}
				textButton.SetTextColor(value);
			}
		}

		public override TextView TextView
		{
			get
			{
				return textButton;
			}
		}

        public void SetTextColor(Color color)
        {
            TextView.SetTextColor(color);
        }
	}
}