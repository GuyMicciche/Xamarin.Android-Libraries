using Android.Content.Res;
using Android.Util;
using Android.Views;

namespace MaterialDesignLibrary
{
	public class Utils
	{
		/// <summary>
		/// Convert Dp to Pixel
		/// </summary>
		public static int DpToPx(float dp, Resources resources)
		{
			float px = TypedValue.ApplyDimension(ComplexUnitType.Dip, dp, resources.DisplayMetrics);
			return (int) px;
		}

		public static int GetRelativeTop(View myView)
		{
			if (myView.Id == Android.Resource.Id.Content)
			{
				return myView.Top;
			}
			else
			{
				return myView.Top + GetRelativeTop((View) myView.Parent);
			}
		}

		public static int GetRelativeLeft(View myView)
		{
			if (myView.Id == Android.Resource.Id.Content)
			{
				return myView.Left;
			}
			else
			{
				return myView.Left + GetRelativeLeft((View) myView.Parent);
			}
		}
	}
}