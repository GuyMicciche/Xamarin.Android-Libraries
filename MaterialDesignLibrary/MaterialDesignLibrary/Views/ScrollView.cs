using Android.Content;
using Android.Util;
using Android.Views;

namespace MaterialDesignLibrary
{
	public class ScrollView : Android.Widget.ScrollView
	{
		/*
		 * This class avoid problems in scrollviews with elements in library
		 * Use it if you want use a ScrollView in your App
		 */
		public ScrollView(Context context, IAttributeSet attrs) : base(context, attrs)
		{

		}

		public override bool OnTouchEvent(MotionEvent ev)
		{
			for (int i = 0; i < ((ViewGroup)GetChildAt(0)).ChildCount; i++)
			{
				try
				{
					CustomView child = (CustomView)((ViewGroup)GetChildAt(0)).GetChildAt(i);
					if (child.isLastTouch)
					{
						child.OnTouchEvent(ev);
						return true;
					}
				}
				catch (System.InvalidCastException e)
				{

				}
			}
			return base.OnTouchEvent(ev);
		}
	}
}