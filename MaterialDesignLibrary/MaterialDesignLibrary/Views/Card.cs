using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class Card : CustomView
	{
		internal TextView textButton;

		internal int paddingTop, paddingBottom, paddingLeft, paddingRight;
		internal Color backgroundColor = Color.ParseColor("#FFFFFF");

		public Card(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
				SetBackgroundResource(Resource.Drawable.background_button_rectangle);
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
					string background = value.GetAttributeValue(ANDROIDXML,"background");
					if (background != null)
					{
						BackgroundColor = Color.ParseColor(background);
					}
					else
					{
						BackgroundColor = this.backgroundColor;
					}
				}
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
				LayerDrawable layer = (LayerDrawable) Background;
				GradientDrawable shape = (GradientDrawable)layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
				shape.SetColor(backgroundColor);
			}
		}
	}
}