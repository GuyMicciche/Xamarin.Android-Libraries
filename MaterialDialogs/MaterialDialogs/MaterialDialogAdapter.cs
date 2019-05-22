using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;

namespace MaterialDialogs
{
	internal class MaterialDialogAdapter : ArrayAdapter<string>
	{

		internal readonly Color itemColor;
		internal readonly MaterialDialog dialog;

		public MaterialDialogAdapter(MaterialDialog dialog, int resource, int textViewResourceId, string[] objects) 
            : base(dialog.mBuilder.context, resource, textViewResourceId, objects)
		{
			this.dialog = dialog;
			itemColor = DialogUtils.ResolveColor(Context, Resource.Attribute.md_item_color, dialog.defaultItemColor);
		}

		public override bool HasStableIds
		{
            get
            {
			    return true;
            }
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int index, View convertView, ViewGroup parent)
		{
			View view = base.GetView(index, convertView, parent);
			TextView tv = (TextView) view.FindViewById(Resource.Id.title);
			switch (dialog.listType)
			{
                case ListType.SINGLE:
				{
					RadioButton radio = (RadioButton) view.FindViewById(Resource.Id.control);
					radio.Checked = dialog.mBuilder.selectedIndex == index;
					break;
				}
				case ListType.MULTI:
				{
					CheckBox checkbox = (CheckBox) view.FindViewById(Resource.Id.control);
					checkbox.Checked = dialog.selectedIndicesList.Contains(index);
					break;
				}
			}
			tv.Text = dialog.mBuilder.items[index];
			tv.SetTextColor(itemColor);
			dialog.SetTypeface(tv, dialog.mBuilder.regularFont);
			view.Tag = index + ":" + dialog.mBuilder.items[index];
			return view;
		}
	}
}