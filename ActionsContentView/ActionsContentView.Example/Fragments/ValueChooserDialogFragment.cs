using Android.App;
using Android.Content;
using Android.OS;

using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace ActionsContentView.Example
{
    public class ValueChooserDialogFragment : DialogFragment
    {
        public static readonly string TAG = Java.Lang.Class.FromType(typeof(ValueChooserDialogFragment)).SimpleName;

        public interface IOnSettingSelectedListener
        {
            void OnSettingSelected(int id, int item);
        }

        private static readonly string ARG_ID = TAG + ":id";
        private static readonly string ARG_TITLE = TAG + ":title";
        private static readonly string ARG_ITEMS_ARRAY = TAG + ":items_array";

        private IOnSettingSelectedListener mSettingSelectedListener;

        public static ValueChooserDialogFragment NewInstance(int id, int titleId, int itemsArrayId)
        {
            ValueChooserDialogFragment fragment = new ValueChooserDialogFragment();
            Bundle args = new Bundle();
            args.PutInt(ARG_ID, id);
            args.PutInt(ARG_TITLE, titleId);
            args.PutInt(ARG_ITEMS_ARRAY, itemsArrayId);
            fragment.Arguments = args;

            return fragment;
        }

        public IOnSettingSelectedListener OnSettingsSelectedListener
        {
            set
            {
                mSettingSelectedListener = value;
            }
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            int id = Arguments.GetInt(ARG_ID);
            int titleId = Arguments.GetInt(ARG_TITLE);
            int itemsArrayId = Arguments.GetInt(ARG_ITEMS_ARRAY);
            string[] items = Resources.GetStringArray(itemsArrayId);

            AlertDialog.Builder builder = (new AlertDialog.Builder(Activity)).SetTitle(titleId).SetItems(items, new OnClickListenerAnonymousInnerClassHelper(this, id));

            return builder.Create();
        }

        private class OnClickListenerAnonymousInnerClassHelper : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            private readonly ValueChooserDialogFragment OuterInstance;

            private int Id;

            public OnClickListenerAnonymousInnerClassHelper(ValueChooserDialogFragment outerInstance, int id)
            {
                this.OuterInstance = outerInstance;
                this.Id = id;
            }

            public void OnClick(IDialogInterface dialog, int item)
            {
                if (OuterInstance.mSettingSelectedListener != null)
                {
                    OuterInstance.mSettingSelectedListener.OnSettingSelected(Id, item);
                }
            }
        }
    }
}