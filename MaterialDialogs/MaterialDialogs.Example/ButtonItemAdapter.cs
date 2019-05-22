using Android.Content;
using Android.Views;
using Android.Widget;
namespace MaterialDialogs.Example
{
	/// <summary>
	/// Simple adapter example for custom items in the dialog
	/// </summary>
	internal class ButtonItemAdapter : BaseAdapter, View.IOnClickListener
	{

		private Toast mToast;
		private readonly Context mContext;
		private readonly string[] mItems;

		public ButtonItemAdapter(Context context, int arrayResId) 
            : this(context, context.Resources.GetTextArray(arrayResId))
		{
		}

		private ButtonItemAdapter(Context context, string[] items)
		{
			this.mContext = context;
			this.mItems = items;
		}

		public override int Count
		{
			get
			{
				return mItems.Length;
			}
		}

        public override Java.Lang.Object GetItem(int position)
        {
            return mItems[position];
        }

		public override long GetItemId(int position)
		{
			return position;
		}

        public override bool HasStableIds
        {
            get
            {
                return true;
            }
        }

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			if (convertView == null)
			{
				convertView = View.Inflate(mContext, Resource.Layout.dialog_customlistitem, null);
			}
			((TextView) convertView.FindViewById(Resource.Id.title)).Text = mItems[position] + " (" + position + ")";
			Button button = (Button) convertView.FindViewById(Resource.Id.button);
			button.Tag = position;
			button.SetOnClickListener(this);
			return convertView;
		}

		public void OnClick(View v)
		{
			int? index = (int?) v.Tag;
			if (mToast != null)
			{
				mToast.Cancel();
			}
			mToast = Toast.MakeText(mContext, "Clicked button " + index, ToastLength.Short);
			mToast.Show();
		}
	}
}