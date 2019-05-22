using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class ButtonRectangle : Button
	{
		internal TextView textButton;

		internal int paddingTop, paddingBottom, paddingLeft, paddingRight;

		public ButtonRectangle(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			SetDefaultProperties();
		}
		protected internal override void SetDefaultProperties()
		{
			base.minWidth = 80;
			base.minHeight = 36;
			base.background = Resource.Drawable.background_button_rectangle;
			base.SetDefaultProperties();
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
    
				// Set Padding
				string padding = value.GetAttributeValue(ANDROIDXML,"padding");
    
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
					textButton.Text = text;
					textButton.SetTextColor(Color.White);
					textButton.SetTypeface(null, TypefaceStyle.Bold);
                    RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
					lp.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
					lp.SetMargins(Utils.DpToPx(5, Resources), Utils.DpToPx(5, Resources), Utils.DpToPx(5, Resources), Utils.DpToPx(5, Resources));
					textButton.LayoutParameters = lp;
					AddView(textButton);
				}
    
				rippleSpeed = value.GetAttributeFloatValue(MATERIALDESIGNXML, "rippleSpeed", Utils.DpToPx(6, Resources));
			}
		}

		internal int height;
		internal int width;

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (x != -1)
			{
				Rect src = new Rect(0, 0, Width - Utils.DpToPx(6, Resources), Height - Utils.DpToPx(7, Resources));
				Rect dst = new Rect(Utils.DpToPx(6, Resources), Utils.DpToPx(6, Resources), Width - Utils.DpToPx(6, Resources), Height - Utils.DpToPx(7, Resources));
				canvas.DrawBitmap(MakeCircle(), src, dst, null);
				Invalidate();
			}
		}

		public virtual string Text
		{
			set
			{
					textButton.Text = value;
			}
			get
			{
					return textButton.Text.ToString();
			}
		}

		public virtual Color TextColor
		{
			set
			{
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
	}
}