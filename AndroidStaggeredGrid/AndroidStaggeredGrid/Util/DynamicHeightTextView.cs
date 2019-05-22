using Android.Content;
using Android.Util;
using Android.Widget;

namespace AndroidStaggeredGrid.Util
{
	/// 
	/// <summary>
	/// A <seealso cref="android.widget.TextView"/> that maintains a consistent width to height aspect ratio.
	/// In the real world this would likely extend ImageView.
	/// </summary>
	public class DynamicHeightTextView : TextView
	{

		private double mHeightRatio;

		public DynamicHeightTextView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public DynamicHeightTextView(Context context) : base(context)
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