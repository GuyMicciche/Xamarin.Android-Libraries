using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Util;
using Android.Views;
using System.Collections.Generic;
using System.Linq;

namespace MaterialDialogs
{
	/// <summary>
	/// This class only works on Honeycomb (API 11) and above.
	/// </summary>
	public class MaterialMultiSelectListPreference : MultiSelectListPreference
	{

		private Context context;
		private MaterialDialog mDialog;

		public MaterialMultiSelectListPreference(Context context) 
            : this(context, null)
		{
		}

		public MaterialMultiSelectListPreference(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
			Init(context);
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

		private void Init(Context context)
		{
			this.context = context;
			if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
			{
				WidgetLayoutResource = 0;
			}
		}

		protected override void ShowDialog(Bundle state)
		{
			IList<int> indices = new List<int>();
			foreach (string s in Values)
			{
				int index = FindIndexOfValue(s);
				if (index >= 0)
				{
					indices.Add(FindIndexOfValue(s));
				}
			}
			MaterialDialog.Builder builder = (new MaterialDialog.Builder(context)).SetTitle(DialogTitle).SetContent(DialogMessage).SetIcon(DialogIcon).SetNegativeText(NegativeButtonText).SetPositiveText(PositiveButtonText).SetItems(GetEntries()).SetItemsCallbackMultiChoice(indices.ToArray(), new ListCallbackMultiChoiceAnonymousInnerClassHelper(this)).SetDismissListener(this);

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

		private class ListCallbackMultiChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackMultiChoice
		{
			private readonly MaterialMultiSelectListPreference preference;

            public ListCallbackMultiChoiceAnonymousInnerClassHelper(MaterialMultiSelectListPreference preference)
			{
                this.preference = preference;
			}

			public virtual bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
			{
                preference.OnClick(null, (int)DialogButtonType.Positive);
				dialog.Dismiss();
                var values = new Java.Util.HashSet();
				foreach (string s in text)
				{
					values.Add((string)s);
				}
                if (preference.CallChangeListener(values))
				{
                    preference.Values = text.ToList();
				}
				return true;
			}
		}
	}
}