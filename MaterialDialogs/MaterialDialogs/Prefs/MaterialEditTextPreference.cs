using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MaterialDialogs
{
	/// <summary>
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	public class MaterialEditTextPreference : EditTextPreference
	{
        /// <summary>
        /// Callback listener for the MaterialDialog. Positive button checks with
        /// OnPreferenceChangeListener before committing user entered text
        /// </summary>
        private MaterialDialog.ButtonCallback callback;

        private Color mColor = Color.Black;

		public MaterialEditTextPreference(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
			{
				mColor = DialogUtils.ResolveColor(context, Resource.Attribute.colorAccent);
			}

            callback = new ButtonCallbackAnonymousInnerClassHelper(this);
		}

		public MaterialEditTextPreference(Context context) 
            : this(context, null)
		{
            callback = new ButtonCallbackAnonymousInnerClassHelper(this);
		}

		protected override void OnAddEditTextToDialogView(View dialogView, EditText editText)
		{
			if (editText.Parent != null)
			{
				((ViewGroup) EditText.Parent).RemoveView(editText);
			}
			((ViewGroup) dialogView).AddView(editText, new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
		}

		protected override void OnBindDialogView(View view)
		{
			EditText.Text = "";
			EditText.Append(Text);
			IViewParent oldParent = EditText.Parent;
			if (oldParent != view)
			{
				if (oldParent != null)
				{
					((ViewGroup) oldParent).RemoveView(EditText);
				}
				OnAddEditTextToDialogView(view, EditText);
			}
		}

		protected override void ShowDialog(Bundle state)
		{
			MaterialDialog.Builder mBuilder = (new MaterialDialog.Builder(Context)).SetTitle(DialogTitle).SetIcon(DialogIcon).SetPositiveText(PositiveButtonText).SetNegativeText(NegativeButtonText).SetCallback(callback).SetContent(DialogMessage);

			View layout = LayoutInflater.From(Context).Inflate(Resource.Layout.md_stub_input, null);
			OnBindDialogView(layout);

			if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
			{
				EditText.Background.SetColorFilter(mColor, PorterDuff.Mode.SrcAtop);
			}

			TextView message = (TextView) layout.FindViewById(Android.Resource.Id.Message);
			if (DialogMessage != null && DialogMessage.ToString().Length > 0)
			{
				message.Visibility = ViewStates.Visible;
				message.Text = DialogMessage;
			}
			else
			{
				message.Visibility = ViewStates.Gone;
			}
			mBuilder.SetCustomView(layout, false);

			MaterialDialog mDialog = mBuilder.Build();
			if (state != null)
			{
				mDialog.OnRestoreInstanceState(state);
			}
			RequestInputMethod(mDialog);

			mDialog.SetOnDismissListener(this);
			mDialog.Show();
		}


		private class ButtonCallbackAnonymousInnerClassHelper : MaterialDialog.ButtonCallback
		{
            private MaterialEditTextPreference preference;

			public ButtonCallbackAnonymousInnerClassHelper(MaterialEditTextPreference preference)
			{
                this.preference = preference;
			}

			public override void OnPositive(MaterialDialog dialog)
			{
                string value = preference.EditText.Text.ToString();
                if (preference.CallChangeListener(value) && preference.Persistent)
				{
                    preference.Text = value;
				}
			}
		}

		/// <summary>
		/// Copied From DialogPreference.java
		/// </summary>
		private void RequestInputMethod(Dialog dialog)
		{
			Window window = dialog.Window;
			window.SetSoftInputMode(SoftInput.StateAlwaysVisible);
		}
	}
}