using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System.Collections.Generic;
using System.Linq;

namespace MaterialDialogs
{
	/// <summary>
	/// Convenience class for migrating old dialogs code. Not all methods are implemented yet. Using MaterialDialog.Builder directly is recommended.
	/// </summary>
	public class AlertDialogWrapper
	{
		public class Builder
		{
			internal readonly MaterialDialog.Builder builder;

			internal IDialogInterfaceOnClickListener negativeDialogListener;
			internal IDialogInterfaceOnClickListener positiveDialogListener;
			internal IDialogInterfaceOnClickListener neutralDialogListener;
			internal IDialogInterfaceOnClickListener OnClickListener;

			public Builder(Context context)
			{
				builder = new MaterialDialog.Builder(context);
			}

			public virtual Builder AutoDismiss(bool dismiss)
			{
				builder.SetAutoDismiss(dismiss);
				return this;
			}

			public virtual Builder SetMessage(int messageId)
			{
				builder.SetContent(messageId);
				return this;
			}

			public virtual Builder SetMessage(string message)
			{
				builder.SetContent(message);
				return this;
			}

			public virtual Builder SetTitle(int titleId)
			{
				builder.SetTitle(titleId);
				return this;
			}

			public virtual Builder SetTitle(string title)
			{
				builder.SetTitle(title);
				return this;
			}

			public virtual Builder SetIcon(int iconId)
			{
				builder.SetIconRes(iconId);
				return this;
			}

			public virtual Builder SetIcon(Drawable icon)
			{
				builder.SetIcon(icon);
				return this;
			}

			public virtual Builder SetIconAttribute(int attrId)
			{
				builder.SetIconAttr(attrId);
				return this;
			}

			public virtual Builder SetNegativeButton(int textId, IDialogInterfaceOnClickListener listener)
			{
				builder.SetNegativeText(textId);
				negativeDialogListener = listener;
				return this;
			}

            public virtual Builder SetNegativeButton(string text, IDialogInterfaceOnClickListener listener)
			{
				builder.SetNegativeText(text);
				negativeDialogListener = listener;
				return this;
			}

            public virtual Builder SetPositiveButton(int textId, IDialogInterfaceOnClickListener listener)
			{
				builder.SetPositiveText(textId);
				positiveDialogListener = listener;
				return this;
			}

            public virtual Builder SetPositiveButton(string text, IDialogInterfaceOnClickListener listener)
			{
				builder.SetPositiveText(text);
				positiveDialogListener = listener;
				return this;
			}

            public virtual Builder SetNeutralButton(int textId, IDialogInterfaceOnClickListener listener)
			{
				builder.SetNeutralText(textId);
				neutralDialogListener = listener;
				return this;
			}

            public virtual Builder SetNeutralButton(string text, IDialogInterfaceOnClickListener listener)
			{
				builder.SetNeutralText(text);
				neutralDialogListener = listener;
				return this;
			}

			public virtual Builder SetCancelable(bool cancelable)
			{
				builder.SetCancelable(cancelable);
				return this;
			}

			public virtual Builder SetItems(int itemsId, IDialogInterfaceOnClickListener listener)
			{
				builder.SetItems(itemsId);
				OnClickListener = listener;
				return this;
			}

            public virtual Builder SetItems(string[] items, IDialogInterfaceOnClickListener listener)
			{
				builder.SetItems(items);
				OnClickListener = listener;
				return this;
			}

			/// <param name="adapter"> The adapter to set. </param>
			/// <returns> An instance of the Builder for chaining. </returns>
			/// @deprecated Use <seealso cref="#setAdapter(ListAdapter, IDialogInterfaceOnClickListener)"/> instead. 
			public virtual Builder SetAdapter(IListAdapter adapter)
			{
				return SetAdapter(adapter, null);
			}

			/// <param name="adapter">  The adapter to set. </param>
			/// <param name="listener"> The listener called when list items are clicked. </param>
			/// <returns> An instance of the Builder for chaining. </returns>
			public virtual Builder SetAdapter(IListAdapter adapter, IDialogInterfaceOnClickListener listener)
			{
				builder.adapter = adapter;
				builder.listCallbackCustom = new ListCallbackAnonymousInnerClassHelper(this, listener);
				return this;
			}

			private class ListCallbackAnonymousInnerClassHelper : MaterialDialog.IListCallback
			{
				private readonly Builder outerInstance;

				private IDialogInterfaceOnClickListener listener;

				public ListCallbackAnonymousInnerClassHelper(Builder outerInstance, IDialogInterfaceOnClickListener listener)
				{
					this.outerInstance = outerInstance;
					this.listener = listener;
				}

				public virtual void OnSelection(MaterialDialog dialog, View itemView, int which, string text)
				{
					listener.OnClick(dialog, which);
				}
			}

			public virtual AlertDialog Create()
			{
				AddButtonsCallback();
				AddListCallbacks();
				return builder.Build();
			}

			public virtual AlertDialog Show()
			{
				AlertDialog dialog = Create();
				dialog.Show();
				return dialog;
			}

			internal virtual void AddListCallbacks()
			{
				if (OnClickListener != null)
				{
					builder.SetItemsCallback(new ListCallbackAnonymousInnerClassHelper2(this));
				}
			}

			private class ListCallbackAnonymousInnerClassHelper2 : MaterialDialog.IListCallback
			{
				private readonly Builder outerInstance;

				public ListCallbackAnonymousInnerClassHelper2(Builder outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void OnSelection(MaterialDialog dialog, View itemView, int which, string text)
				{
					outerInstance.OnClickListener.OnClick(dialog, which);
				}
			}

			internal virtual void AddButtonsCallback()
			{
				if (positiveDialogListener != null || negativeDialogListener != null)
				{
					builder.SetCallback(new ButtonCallbackAnonymousInnerClassHelper(this));
				}
			}

			private class ButtonCallbackAnonymousInnerClassHelper : MaterialDialog.ButtonCallback
			{
				private readonly Builder outerInstance;

				public ButtonCallbackAnonymousInnerClassHelper(Builder outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void OnNeutral(MaterialDialog dialog)
				{
					if (outerInstance.neutralDialogListener != null)
					{
						outerInstance.neutralDialogListener.OnClick(dialog, (int)DialogButtonType.Neutral);
					}
				}

				public override void OnPositive(MaterialDialog dialog)
				{
					if (outerInstance.positiveDialogListener != null)
					{
                        outerInstance.positiveDialogListener.OnClick(dialog, (int)DialogButtonType.Positive);
					}
				}

				public override void OnNegative(MaterialDialog dialog)
				{
					if (outerInstance.negativeDialogListener != null)
					{
                        outerInstance.negativeDialogListener.OnClick(dialog, (int)DialogButtonType.Negative);
					}
				}
			}

			public virtual Builder SetView(View view)
			{
				builder.SetCustomView(view, false);
				return this;
			}

			/// <summary>
			/// Set a list of items to be displayed in the dialog as the content, you will be notified of the selected item via the supplied listener.
			/// </summary>
			/// <param name="itemsId">      A resource ID for the items (e.g. R.array.my_items) </param>
			/// <param name="checkedItems"> specifies which items are checked. It should be null in which case no items are checked. If non null it must be exactly the same length as the array of items. </param>
			/// <param name="listener">     notified when an item on the list is clicked. The dialog will not be dismissed when an item is clicked. It will only be dismissed if clicked on a button, if no buttons are supplied it's up to the user to dismiss the dialog.		 * @return </param>
			/// <returns> This </returns>
            public virtual Builder SetMultiChoiceItems(int itemsId, bool[] checkedItems, IDialogInterfaceOnMultiChoiceClickListener listener)
			{
				builder.SetItems(itemsId);
				SetUpMultiChoiceCallback(checkedItems, listener);
				return this;
			}

			/// <summary>
			/// Set a list of items to be displayed in the dialog as the content, you will be notified of the selected item via the supplied listener.
			/// </summary>
			/// <param name="items">        the text of the items to be displayed in the list. </param>
			/// <param name="checkedItems"> specifies which items are checked. It should be null in which case no items are checked. If non null it must be exactly the same length as the array of items. </param>
			/// <param name="listener">     notified when an item on the list is clicked. The dialog will not be dismissed when an item is clicked. It will only be dismissed if clicked on a button, if no buttons are supplied it's up to the user to dismiss the dialog.		 * @return </param>
			/// <returns> This </returns>
            public virtual Builder SetMultiChoiceItems(string[] items, bool[] checkedItems, IDialogInterfaceOnMultiChoiceClickListener listener)
			{
				builder.SetItems(items);
				SetUpMultiChoiceCallback(checkedItems, listener);
				return this;
			}

			public virtual Builder AlwaysCallSingleChoiceCallback()
			{
				builder.AlwaysCallSingleChoiceCallback();
				return this;
			}

			public virtual Builder AlwaysCallMultiChoiceCallback()
			{
				builder.AlwaysCallMultiChoiceCallback();
				return this;
			}

            internal virtual void SetUpMultiChoiceCallback(bool[] checkedItems, IDialogInterfaceOnMultiChoiceClickListener listener)
			{
				int[] selectedIndicesArr = null;
				/* Convert old style array of booleans-per-index to new list of indices */
				if (checkedItems != null)
				{
					List<int> selectedIndices = new List<int>();
					for (int i = 0; i < checkedItems.Length; i++)
					{
						if (checkedItems[i])
						{
							selectedIndices.Add(i);
						}
					}
					selectedIndicesArr = selectedIndices.ToArray();
				}

				builder.SetItemsCallbackMultiChoice(selectedIndicesArr, new ListCallbackMultiChoiceAnonymousInnerClassHelper(this, checkedItems, listener));
			}

			private class ListCallbackMultiChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackMultiChoice
			{
				private readonly Builder builder;

				private bool[] checkedItems;
				private IDialogInterfaceOnMultiChoiceClickListener listener;

                public ListCallbackMultiChoiceAnonymousInnerClassHelper(Builder builder, bool[] checkedItems, IDialogInterfaceOnMultiChoiceClickListener listener)
				{
                    this.builder = builder;
					this.checkedItems = checkedItems;
					this.listener = listener;
				}

				public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
				{
					/* which is a list of selected indices */
					IList<int> whichList = which;
					if (checkedItems != null)
					{
						for (int i = 0; i < checkedItems.Length; i++)
						{
							/* save old state */
							bool oldChecked = checkedItems[i];
							/* Record new state */
							checkedItems[i] = whichList.Contains(i);
							/* Fire the listener if it changed */
							if (oldChecked != checkedItems[i])
							{
								listener.OnClick(dialog, i, checkedItems[i]);
							}
						}
					}
					return true;
				}
			}

			/// <summary>
			/// Set a list of items to be displayed in the dialog as the content, you will be notified of the selected item via the supplied listener.
			/// </summary>
			/// <param name="items">       the items to be displayed. </param>
			/// <param name="checkedItem"> specifies which item is checked. If -1 no items are checked. </param>
			/// <param name="listener">    notified when an item on the list is clicked. The dialog will not be dismissed when an item is clicked. It will only be dismissed if clicked on a button, if no buttons are supplied it's up to the user to dismiss the dialog. </param>
			/// <returns> This </returns>
			public virtual Builder SetSingleChoiceItems(string[] items, int checkedItem, IDialogInterfaceOnClickListener listener)
			{
				builder.SetItems(items);
				builder.SetItemsCallbackSingleChoice(checkedItem, new ListCallbackSingleChoiceAnonymousInnerClassHelper(this, listener));
				return this;
			}

			private class ListCallbackSingleChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackSingleChoice
			{
				private readonly Builder outerInstance;

                private IDialogInterfaceOnClickListener listener;

                public ListCallbackSingleChoiceAnonymousInnerClassHelper(Builder outerInstance, IDialogInterfaceOnClickListener listener)
				{
					this.outerInstance = outerInstance;
					this.listener = listener;
				}

				public bool OnSelection(MaterialDialog dialog, View itemView, int which, string text)
				{
					listener.OnClick(dialog, which);
					return true;
				}
			}

			/// <summary>
			/// Set a list of items to be displayed in the dialog as the content, you will be notified of the selected item via the supplied listener.
			/// </summary>
			/// <param name="itemsId">     the resource id of an array i.e. R.array.foo </param>
			/// <param name="checkedItem"> specifies which item is checked. If -1 no items are checked. </param>
			/// <param name="listener">    notified when an item on the list is clicked. The dialog will not be dismissed when an item is clicked. It will only be dismissed if clicked on a button, if no buttons are supplied it's up to the user to dismiss the dialog. </param>
			/// <returns> This </returns>
			public virtual Builder SetSingleChoiceItems(int itemsId, int checkedItem, IDialogInterfaceOnClickListener listener)
			{
				builder.SetItems(itemsId);
				builder.SetItemsCallbackSingleChoice(checkedItem, new ListCallbackSingleChoiceAnonymousInnerClassHelper2(this, listener));
				return this;
			}

			private class ListCallbackSingleChoiceAnonymousInnerClassHelper2 : MaterialDialog.IListCallbackSingleChoice
			{
				private readonly Builder outerInstance;

				private IDialogInterfaceOnClickListener listener;

				public ListCallbackSingleChoiceAnonymousInnerClassHelper2(Builder outerInstance, IDialogInterfaceOnClickListener listener)
				{
					this.outerInstance = outerInstance;
					this.listener = listener;
				}

				public bool OnSelection(MaterialDialog dialog, View itemView, int which, string text)
				{
					listener.OnClick(dialog, which);
					return true;
				}
			}

			public virtual Builder SetOnCancelListener(IDialogInterfaceOnCancelListener listener)
			{
				builder.SetCancelListener(listener);
				return this;
			}

			public virtual Builder SetOnDismissListener(IDialogInterfaceOnDismissListener listener)
			{
				builder.SetDismissListener(listener);
				return this;
			}

			public virtual Builder SetOnShowListener(IDialogInterfaceOnShowListener listener)
			{
				builder.SetShowListener(listener);
				return this;
			}

			public virtual Builder SetOnKeyListener(IDialogInterfaceOnKeyListener listener)
			{
				builder.SetKeyListener(listener);
				return this;
			}
		}
	}
}