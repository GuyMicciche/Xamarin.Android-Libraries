using Android.Content;
using Android.Util;
using Android.Widget;
namespace MaterialDesignLibrary
{
	public class ButtonFloatSmall : ButtonFloat
	{
		public ButtonFloatSmall(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			sizeRadius = 20;
			sizeIcon = 20;
			SetDefaultProperties();
			RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(Utils.DpToPx(sizeIcon, Resources),Utils.DpToPx(sizeIcon, Resources));
			lp.AddRule(LayoutRules.CenterInParent, (int)LayoutRules.True);
			icon.LayoutParameters = lp;
		}

        protected internal override void SetDefaultProperties()
        {
            rippleSpeed = Utils.DpToPx(2, Resources);
            rippleSize = 10;
            // Min size
            SetMinimumHeight(Utils.DpToPx(sizeRadius * 2, Resources));
            SetMinimumWidth(Utils.DpToPx(sizeRadius * 2, Resources));
            // Background shape
            SetBackgroundResource(Resource.Drawable.background_button_float);
        }
	}
}