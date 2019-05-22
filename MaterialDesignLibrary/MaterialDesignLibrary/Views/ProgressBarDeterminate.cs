using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class ProgressBarDeterminate : CustomView
	{
		internal int max = 100;
		internal int min = 0;
		internal int progress = 0;

		internal Color backgroundColor = Color.ParseColor("#1E88E5");

		internal View progressView;

		public ProgressBarDeterminate(Context context, IAttributeSet attrs) : base(context, attrs)
		{
			Attributes = attrs;
		}

		// Set atributtes of XML to View
		protected internal virtual IAttributeSet Attributes
		{
			set
			{
				progressView = new View(Context);
				RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(1,1);
				progressView.LayoutParameters = lp;
				progressView.SetBackgroundResource(Resource.Drawable.background_progress);
				AddView(progressView);
    
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
    
				min = value.GetAttributeIntValue(MATERIALDESIGNXML,"min", 0);
				max = value.GetAttributeIntValue(MATERIALDESIGNXML,"max", 100);
				progress = value.GetAttributeIntValue(MATERIALDESIGNXML,"progress", min);
    
				SetMinimumHeight(Utils.DpToPx(3, Resources));
    
				Post(() =>
                    {
                        RelativeLayout.LayoutParams layParams = (RelativeLayout.LayoutParams)progressView.LayoutParameters;
                        layParams.Height = Height;
                        progressView.LayoutParameters = layParams;
                    });
    
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
			return Color.Argb(128,r, g, b);
		}

		// SETTERS
		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (pendindProgress != -1)
			{
				Progress = pendindProgress;
			}
		}

		public virtual int Max
		{
			set
			{
				this.max = value;
			}
		}

		public virtual int Min
		{
			set
			{
				this.min = value;
			}
		}

		internal int pendindProgress = -1;
		public virtual int Progress
		{
			set
			{
				if (Width == 0)
				{
					pendindProgress = value;
				}
				else
				{
					this.progress = value;
					if (value > max)
					{
						value = max;
					}
					if (value < min)
					{
						value = min;
					}
					int totalWidth = max - min;
					double progressPercent = (double)value / (double)totalWidth;
					int progressWidth = (int)(Width * progressPercent);
					RelativeLayout.LayoutParams lp = (RelativeLayout.LayoutParams) progressView.LayoutParameters;
					lp.Width = progressWidth;
					lp.Height = Height;
					progressView.LayoutParameters = lp;
					pendindProgress = -1;
				}
			}
			get
			{
				return progress;
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
				LayerDrawable layer = (LayerDrawable) progressView.Background;
				GradientDrawable shape = (GradientDrawable) layer.FindDrawableByLayerId(Resource.Id.shape_bacground);
				shape.SetColor(value);
				base.SetBackgroundColor(MakePressColor());
			}
		}
	}
}