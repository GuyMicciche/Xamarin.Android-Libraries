using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Text;
using Android.Util;
using Android.Views;
using Java.Lang;

namespace MaterialDialogs
{
	/// <summary>
	/// @author Marc Holder Kluver (marchold), Aidan Follestad (afollestad)
	/// </summary>
	public class MaterialListPreference : ListPreference
	{
		private Context context;
		private MaterialDialog mDialog;

		public MaterialListPreference(Context context) 
            : base(context)
		{
			Init(context);
		}

		public MaterialListPreference(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			Init(context);
		}

		private void Init(Context context)
		{
			this.context = context;
			if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
			{
				WidgetLayoutResource = 0;
			}
		}

		public string[] Entries
		{
			set
			{
				base.SetEntries(value);
				if (mDialog != null)
				{
					mDialog.SetItems(value);
				}
			}
		}

		protected override void ShowDialog(Bundle state)
		{
			if (GetEntries() == null || GetEntryValues() == null)
			{
				throw new IllegalStateException("ListPreference requires an entries array and an entryValues array.");
			}

			int preselect = FindIndexOfValue(Value);
			MaterialDialog.Builder builder = (new MaterialDialog.Builder(context)).SetTitle(DialogTitle).SetContent(DialogMessage).SetIcon(DialogIcon).SetNegativeText(NegativeButtonText).SetItems(GetEntries()).SetAutoDismiss(true).SetItemsCallbackSingleChoice(preselect, new ListCallbackSingleChoiceAnonymousInnerClassHelper(this)); // immediately close the dialog after selection

			View contentView = OnCreateDialogView();
			if (contentView != null)
			{
				OnBindDialogView(contentView);
				builder.SetCustomView(contentView, false);
			}
			else
			{
				builder.SetContent(DialogMessage);
			}

			mDialog = builder.Show();
		}

		private class ListCallbackSingleChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackSingleChoice
		{
			private readonly MaterialListPreference preference;

            public ListCallbackSingleChoiceAnonymousInnerClassHelper(MaterialListPreference preference)
			{
                this.preference = preference;
			}

			public virtual bool OnSelection(MaterialDialog dialog, View itemView, int which, string text)
			{
                preference.OnClick(null, (int)DialogButtonType.Positive);
                if (which >= 0 && preference.GetEntryValues() != null)
				{
                    string value = preference.GetEntryValues()[which].ToString();
                    if (preference.CallChangeListener(value) && preference.Persistent)
					{
						preference.Value = value;
					}
				}
				return true;
			}
		}

		public override string Value
		{
			set
			{
				if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
				{
					base.Value = value;
				}
				else
				{
					string oldValue = Value;
					base.Value = value;
					if (!TextUtils.Equals(value, oldValue))
					{
						NotifyChanged();
					}
				}
			}
		}
	}
}