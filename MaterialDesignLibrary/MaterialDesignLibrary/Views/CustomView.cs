using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Widget;

namespace MaterialDesignLibrary
{
	public class CustomView : RelativeLayout
	{
		internal const string MATERIALDESIGNXML = "http://schemas.android.com/apk/res-auto";
		internal const string ANDROIDXML = "http://schemas.android.com/apk/res/android";

        internal readonly Color disabledBackgroundColor = Color.ParseColor("#E2E2E2");
        internal Color beforeBackground;

		// Indicate if user touched this view the last time
		public bool isLastTouch = false;

		public CustomView(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
		}

		public override bool Enabled
		{
			set
			{
				base.Enabled = value;
				if (value)
				{
					SetBackgroundColor(beforeBackground);
				}
				else
				{
                    SetBackgroundColor(disabledBackgroundColor);
				}
				Invalidate();
			}
		}

		internal bool animation = false;

		protected override void OnAnimationStart()
		{
			base.OnAnimationStart();
			animation = true;
		}

		protected override void OnAnimationEnd()
		{
			base.OnAnimationEnd();
			animation = false;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);
			if (animation)
			{
				Invalidate();
			}
		}
	}
}