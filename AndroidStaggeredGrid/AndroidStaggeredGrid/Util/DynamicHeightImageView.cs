using Android.Content;
using Android.Util;
using Android.Widget;

namespace AndroidStaggeredGrid.Util
{
	/// <summary>
	/// An <seealso cref="android.widget.ImageView"/> layout that maintains a consistent width to height aspect ratio.
	/// </summary>
	public class DynamicHeightImageView : ImageView
	{
		private double mHeightRatio;

		public DynamicHeightImageView(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
		}

		public DynamicHeightImageView(Context context)
            : base(context)
		{
		}

		public double HeightRatio
		{
			set
			{
				if (value != mHeightRatio)
				{
					mHeightRatio = value;
					RequestLayout();
				}
			}
			get
			{
				return mHeightRatio;
			}
		}

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (mHeightRatio > 0.0)
			{
				// set the image views size
				int width = MeasureSpec.GetSize(widthMeasureSpec);
				int height = (int)(width * mHeightRatio);
				SetMeasuredDimension(width, height);
			}
			else
			{
				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}
		}
	}
}