using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using Fragment = Android.Support.V4.App.Fragment;
using System.Collections;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Dialog with a list of options.
	/// <p/>
	/// Implement <seealso cref="IListDialogListener"/> to handle selection of single and no choice
	/// modes. Implement <seealso cref="IMultiChoiceListDialogListener"/> to handle selection of
	/// multi choice.
	/// </summary>
	public class ListDialogFragment : BaseDialogFragment
	{
		protected internal const string ARG_ITEMS = "items";
		protected internal const string ARG_CHECKED_ITEMS = "checkedItems";
		protected internal const string ARG_MODE = "choiceMode";
		protected internal const string ARG_TITLE = "title";
		protected internal const string ARG_POSITIVE_BUTTON = "positive_button";
		protected internal const string ARG_NEGATIVE_BUTTON = "negative_button";

		public static SimpleListDialogBuilder CreateBuilder(Context context, FragmentManager fragmentManager)
		{
			return new SimpleListDialogBuilder(context, fragmentManager);
		}

		private static int[] AsIntArray(SparseBooleanArray checkedItems)
		{
			int num = 0;
			// compute number of items
			for (int i = 0; i < checkedItems.Size(); i++)
			{
				int key = checkedItems.KeyAt(i);
				if (checkedItems.Get(key))
				{
                    ++num;
				}
			}

            int[] array = new int[num];
			//add indexes that are checked
            for (int i = 0, j = 0; i < checkedItems.Size(); i++)
			{
                int key = checkedItems.KeyAt(i);
                if (checkedItems.Get(key))
				{
					array[j++] = key;
				}
			}
			Java.Util.Arrays.Sort(array);
			return array;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			if (Arguments == null)
			{
				throw new System.ArgumentException("use SimpleListDialogBuilder to construct this dialog");
			}
		}

		private IListAdapter PrepareAdapter(int itemLayoutId)
		{
			return new CustomArrayAdapter(this, Activity, itemLayoutId, Resource.Id.sdl_text, Items);
		}

		private class CustomArrayAdapter : ArrayAdapter<object>
		{
			private readonly ListDialogFragment fragment;
            private int resource;

            public CustomArrayAdapter(ListDialogFragment fragment, Activity activity, int resource, int textViewResourceId, string[] getItems)
                : base(activity, resource, textViewResourceId, getItems)
			{
                this.fragment = fragment;
                this.resource = resource;
			}

			/// <summary>
			/// Overriding default implementation because it ignores current light/dark theme.
			/// </summary>
			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
                    convertView = LayoutInflater.From(parent.Context).Inflate(resource, parent, false);
				}
				TextView t = (TextView)convertView.FindViewById(Resource.Id.sdl_text);
				if (t != null)
				{
                    t.Text = fragment.Items[position].ToString();
				}
				return convertView;
			}
		}

		private void BuildMultiChoice(Builder builder)
		{
            builder.SetItems(PrepareAdapter(Resource.Layout.sdl_list_item_multichoice), AsIntArray(CheckedItems), Android.Widget.ChoiceMode.Multiple, (sender, e) =>
            {
                SparseBooleanArray checkedPositions = ((ListView)e.Parent).CheckedItemPositions;
                CheckedItems = new SparseBooleanArrayParcelable(checkedPositions); 
            });
		}

		private void BuildSingleChoice(Builder builder)
		{
            builder.SetItems(PrepareAdapter(Resource.Layout.sdl_list_item_singlechoice), AsIntArray(CheckedItems), Android.Widget.ChoiceMode.Single, (sender, e) =>
            {
                foreach (IListDialogListener listener in SingleDialogListeners)
                {
                    listener.OnListItemSelected(Items[e.Position], e.Position, mRequestCode);
                }
                Dismiss();
            });
		}

		private void BuildNormalChoice(Builder builder)
		{
			builder.SetItems(PrepareAdapter(Resource.Layout.sdl_list_item), -1, (sender, e) =>
            {
                foreach (IListDialogListener listener in SingleDialogListeners)
                {
                    listener.OnListItemSelected(Items[e.Position], e.Position, mRequestCode);
                }
                Dismiss();
            });
		}

		protected override Builder Build(Builder builder)
		{
            string[] items = Items;

			string title = Title;
			if (!TextUtils.IsEmpty(title))
			{
				builder.SetTitle(title);
			}

			if (!TextUtils.IsEmpty(NegativeButtonText))
			{
                builder.SetNegativeButton(NegativeButtonText, (sender, e) =>
                {
                    foreach (ISimpleDialogCancelListener listener in CancelListeners)
                    {
                        listener.OnCancelled(mRequestCode);
                    }
                    Dismiss();
                });
			}

			//confirm button makes no sense when CHOICE_MODE_NONE
			if (Mode != AbsListViewChoiceMode.None)
			{
				string positiveButton = PositiveButtonText;
				if (TextUtils.IsEmpty(PositiveButtonText))
				{
					//we always need confirm button when CHOICE_MODE_SINGLE or CHOICE_MODE_MULTIPLE
                    positiveButton = GetString(Android.Resource.String.Ok);
				}

                switch (Mode)
                {
                    case AbsListViewChoiceMode.Multiple:
                        builder.SetPositiveButton(positiveButton, (sender, e) =>
                        {
                            // prepare multiple results
                            int[] checkedPositions = AsIntArray(CheckedItems);
                            items = Items;
                            string[] checkedValues = new string[checkedPositions.Length];
                            int i = 0;
                            foreach (int checkedPosition in checkedPositions)
                            {
                                if (checkedPosition >= 0 && checkedPosition < items.Length)
                                {
                                    checkedValues[i++] = items[checkedPosition];
                                }
                            }

                            foreach (IMultiChoiceListDialogListener listener in MutlipleDialogListeners)
                            {
                                listener.OnListItemsSelected(checkedValues, checkedPositions, mRequestCode);
                            }
                            Dismiss();
                        });
                        break;
                    case AbsListViewChoiceMode.Single:
                        builder.SetPositiveButton(positiveButton, (sender, e) =>
                        {
                            // prepare single result
                            int selectedPosition = -1;
                            int[] checkedPositions = AsIntArray(CheckedItems);
                            items = Items;
                            foreach (int i in checkedPositions)
                            {
                                if (i >= 0 && i < items.Length)
                                {
                                    //1st valid value
                                    selectedPosition = i;
                                    break;
                                }
                            }

                            // either item is selected or dialog is cancelled
                            if (selectedPosition != -1)
                            {
                                foreach (IListDialogListener listener in SingleDialogListeners)
                                {
                                    listener.OnListItemSelected(items[selectedPosition], selectedPosition, mRequestCode);
                                }
                            }
                            else
                            {
                                foreach (ISimpleDialogCancelListener listener in CancelListeners)
                                {
                                    listener.OnCancelled(mRequestCode);
                                }
                            }
                            Dismiss();
                        });
                        break;
                }
			}

			// prepare list and its item click listener
			items = Items;
			if (items != null && items.Length > 0)
			{
                AbsListViewChoiceMode mode = Mode;
				switch (mode)
				{
                    case AbsListViewChoiceMode.Multiple:
						BuildMultiChoice(builder);
						break;
                    case AbsListViewChoiceMode.Single:
						BuildSingleChoice(builder);
						break;
                    case AbsListViewChoiceMode.None:
						BuildNormalChoice(builder);
						break;
				}
			}

			return builder;
		}

		/// <summary>
		/// Get dialog listeners.
		/// There might be more than one listener.
		/// </summary>
		/// <returns> Dialog listeners
		/// @since 2.1.0 </returns>
        //private IList<IListDialogListener> SingleDialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<IListDialogListener>(typeof(IListDialogListener));
        //    }
        //}
        protected internal virtual IList SingleDialogListeners
        {
            get
            {
                Type listenerInterface = typeof(IListDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<IListDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((IListDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((IListDialogListener)Activity);
                }

                return Java.Util.Collections.UnmodifiableList(listeners);
            }
        }

		/// <summary>
		/// Get dialog listeners.
		/// There might be more than one listener.
		/// </summary>
		/// <returns> Dialog listeners
		/// @since 2.1.0 </returns>
        //private IList<IMultiChoiceListDialogListener> MutlipleDialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<IMultiChoiceListDialogListener>(typeof(IMultiChoiceListDialogListener));
        //    }
        //}
        protected internal virtual System.Collections.IList MutlipleDialogListeners
        {
            get
            {
                Type listenerInterface = typeof(IMultiChoiceListDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<IMultiChoiceListDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((IMultiChoiceListDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((IMultiChoiceListDialogListener)Activity);
                }

                return Java.Util.Collections.UnmodifiableList(listeners);
            }
        }

		private string Title
		{
			get
			{
                return Arguments.GetString(ARG_TITLE);
			}
		}

		private AbsListViewChoiceMode Mode
		{
			get
			{
				return (AbsListViewChoiceMode)Arguments.GetInt(ARG_MODE);
			}
		}

		private string[] Items
		{
			get
			{
                return Arguments.GetStringArray(ARG_ITEMS);
			}
		}

		private SparseBooleanArrayParcelable CheckedItems
		{
			get
			{
				SparseBooleanArrayParcelable items = (SparseBooleanArrayParcelable)Arguments.GetParcelable(ARG_CHECKED_ITEMS);
				if (items == null)
				{
					items = new SparseBooleanArrayParcelable();
				}
				return items;
			}
			set
			{
				Arguments.PutParcelable(ARG_CHECKED_ITEMS, value);
			}
		}


		private string PositiveButtonText
		{
			get
			{
                return Arguments.GetString(ARG_POSITIVE_BUTTON);
			}
		}

		private string NegativeButtonText
		{
			get
			{
                return Arguments.GetString(ARG_NEGATIVE_BUTTON);
			}
		}

		public class SimpleListDialogBuilder : BaseDialogBuilder<SimpleListDialogBuilder>
		{
			internal string title;

			internal string[] items;

            internal AbsListViewChoiceMode mode;
			internal int[] checkedItems;

			internal string cancelButtonText;
			internal string confirmButtonText;


			public SimpleListDialogBuilder(Context context, FragmentManager fragmentManager) 
                : base(context, fragmentManager, Java.Lang.Class.FromType(typeof(ListDialogFragment)))
			{
			}

			protected override SimpleListDialogBuilder Self()
			{
				return this;
			}

			internal virtual Resources Resources
			{
				get
				{
					return mContext.Resources;
				}
			}

            public virtual SimpleListDialogBuilder SetTitle(string title)
            {
                this.title = title;
                return this;
            }


			public virtual SimpleListDialogBuilder SetTitle(int titleResID)
			{
				this.title = Resources.GetText(titleResID);
				return this;
			}

			/// <summary>
			/// Positions of item that should be pre-selected
			/// Valid for setChoiceMode(AbsListView.CHOICE_MODE_MULTIPLE)
			/// </summary>
			/// <param name="positions"> list of item positions to mark as checked </param>
			/// <returns> builder </returns>
			public virtual SimpleListDialogBuilder SetCheckedItems(int[] positions)
			{
				this.checkedItems = positions;
				return this;
			}

			/// <summary>
			/// Position of item that should be pre-selected
			/// Valid for setChoiceMode(AbsListView.CHOICE_MODE_SINGLE)
			/// </summary>
			/// <param name="position"> item position to mark as selected </param>
			/// <returns> builder </returns>
			public virtual SimpleListDialogBuilder SetSelectedItem(int position)
			{
				this.checkedItems = new int[]{ position };
				return this;
			}

            public virtual SimpleListDialogBuilder SetChoiceMode(AbsListViewChoiceMode choiceMode)
			{
				this.mode = choiceMode;
				return this;
			}

            public virtual SimpleListDialogBuilder SetItems(string[] items)
            {
                this.items = items;
                return this;
            }

			public virtual SimpleListDialogBuilder SetItems(int itemsArrayResID)
			{
				this.items = Resources.GetTextArray(itemsArrayResID);
				return this;
			}

            public virtual SimpleListDialogBuilder SetConfirmButtonText(string text)
            {
                this.confirmButtonText = text;
                return this;
            }

			public virtual SimpleListDialogBuilder SetConfirmButtonText(int confirmBttTextResID)
			{
				this.confirmButtonText = Resources.GetText(confirmBttTextResID);
				return this;
			}

            public virtual SimpleListDialogBuilder SetCancelButtonText(string text)
            {
                this.cancelButtonText = text;
                return this;
            }

			public virtual SimpleListDialogBuilder SetCancelButtonText(int cancelBttTextResID)
			{
                this.cancelButtonText = Resources.GetText(cancelBttTextResID);
				return this;
			}
            
            public override Android.Support.V4.App.DialogFragment Show()
            {
                return base.Show();
            }

			protected override Bundle PrepareArguments()
			{
				Bundle args = new Bundle();
				args.PutString(ARG_TITLE, title);
                args.PutString(ARG_POSITIVE_BUTTON, confirmButtonText);
                args.PutString(ARG_NEGATIVE_BUTTON, cancelButtonText);

                args.PutStringArray(ARG_ITEMS, items);

				SparseBooleanArrayParcelable sparseArray = new SparseBooleanArrayParcelable();
				for (int index = 0; checkedItems != null && index < checkedItems.Length; index++)
				{
					sparseArray.Put(checkedItems[index], true);
				}
				args.PutParcelable(ARG_CHECKED_ITEMS, sparseArray);
				args.PutInt(ARG_MODE, (int)mode);
				
                return args;
			}
		}
	}
}