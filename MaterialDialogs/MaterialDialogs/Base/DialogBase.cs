using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Java.Lang;
using System;

namespace MaterialDialogs
{
	public class DialogBase : AlertDialog, IDialogInterfaceOnShowListener
	{
		protected internal const string POSITIVE = "POSITIVE";
		protected internal const string NEGATIVE = "NEGATIVE";
		protected internal const string NEUTRAL = "NEUTRAL";
        private IDialogInterfaceOnShowListener mShowListener;

		protected internal DialogBase(Context context)
            : base(context)
		{

		}

		protected internal virtual void SetVerticalMargins(View view, int TopMargin, int BottomMargin)
		{
			ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams) view.LayoutParameters;
			bool changed = false;
			if (TopMargin > -1 && lp.TopMargin != TopMargin)
			{
				lp.TopMargin = TopMargin;
				changed = true;
			}
			if (BottomMargin > -1 && lp.BottomMargin != BottomMargin)
			{
				lp.BottomMargin = BottomMargin;
				changed = true;
			}
			if (changed)
			{
				view.LayoutParameters = lp;
			}
		}

		/// @deprecated Not supported by the Material dialog. 
		[Obsolete("Not supported by the Material dialog.")]
		public override void SetView (View view)
		{
			throw new System.Exception("This method is not supported by the MaterialDialog.");
		}

		protected internal virtual void SetViewInternal(View view)
		{
		    base.SetView(view);
		}

		/// @deprecated Not supported by the Material dialog. 
		[Obsolete("Not supported by the Material dialog.")]
		public override void SetView(View view, int viewSpacingLeft, int viewSpacingTop, int viewSpacingRight, int viewSpacingBottom)
		{
			throw new System.Exception("This method is not supported by the MaterialDialog.");
		}

        [Obsolete("Use setContent() instead.")]
        public override void SetMessage(ICharSequence message)
        {
            throw new System.Exception("This method is not supported by the MaterialDialog, use setContent() instead.");
        }

		/// @deprecated Not supported by the Material dialog. 
		[Obsolete("Not supported by the Material dialog.")]
		public override void SetCustomTitle(View view)
		{
            throw new System.Exception("This method is not supported by the MaterialDialog.");
		}

        /// @deprecated Not supported by the Material dialog. 
        [Obsolete("Not supported by the Material dialog.")]
        public override void SetButton(int whichButton, ICharSequence text, Message msg)
        {
            throw new System.Exception("Use setActionButton(MaterialDialog.Button, string) instead.");
        }

        /// @deprecated Not supported by the Material dialog. 
        [Obsolete("Not supported by the Material dialog.")]
        public override void SetButton(int whichButton, ICharSequence text, IDialogInterfaceOnClickListener listener)
        {
            throw new System.Exception("Use setActionButton(MaterialDialog.Button, string) instead.");
        }

        public override void SetOnShowListener(IDialogInterfaceOnShowListener listener)
		{
			mShowListener = listener;
		}

		public void _setViewInternal(View view)
		{
            base.SetView(view);
		}

		public void _setOnShowListenerInternal()
		{
			base.SetOnShowListener(this);
		}

		public void OnShow(IDialogInterface dialog)
		{
			if (mShowListener != null)
			{
				mShowListener.OnShow(dialog);
			}
		}

		protected internal virtual void SetBackgroundCompat(View view, Drawable d)
		{
			if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBean)
			{
				//noinspection deprecation
				view.SetBackgroundDrawable(d);
			}
			else
			{
				view.Background = d;
			}
		}
	}
}