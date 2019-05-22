using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Linq;


using Java.Lang;

using System;
using System.Collections.Generic;
using System.Collections;

using DialogFragment = Android.Support.V4.App.DialogFragment;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentManager = Android.Support.V4.App.FragmentManager;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Base dialog fragment for all your dialogs, styleable and same design on Android 2.2+.
	/// </summary>
	public abstract class BaseDialogFragment : DialogFragment, IDialogInterfaceOnShowListener
	{
		//True then use dark theme , else by default make use of light theme
		private static bool darkTheme;
		protected internal int mRequestCode;

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			Bundle args = Arguments;

			if (args != null)
			{
                if (args.GetBoolean("usedarktheme"))
				{
					//Developer is explicitly using the dark theme
					darkTheme = true;
				}
				else
				{
					//Dynamically detecting the theme declared in manifest
					ResolveTheme();
				}
			}
			else
			{

				//Dynamically detecting the theme declared in manifest
				ResolveTheme();
			}

            Dialog dialog = new Dialog(Activity, darkTheme ? Resource.Style.SDL_Dialog_Dark : Resource.Style.SDL_Dialog);

			if (args != null)
			{
                dialog.SetCanceledOnTouchOutside(args.GetBoolean("cancelable_oto"));
			}
			dialog.SetOnShowListener(this);
			return dialog;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			Builder builder = new Builder(Activity, inflater, container);
			return Build(builder).Create();
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			Fragment targetFragment = TargetFragment;
			if (targetFragment != null)
			{
				mRequestCode = TargetRequestCode;
			}
			else
			{
				Bundle args = Arguments;
				if (args != null)
				{
                    mRequestCode = args.GetInt("request_code", 0);
				}
			}
		}

		/// <summary>
		/// Key method for using <seealso cref="com.avast.android.dialogs.core.BaseDialogFragment"/>.
		/// Customized dialogs need to be set up via provided builder.
		/// </summary>
		/// <param name="initialBuilder"> Provided builder for setting up customized dialog </param>
		/// <returns> Updated builder </returns>
		protected abstract Builder Build(Builder initialBuilder);

		public override void OnDestroyView()
		{
			// bug in the compatibility library
			if (Dialog != null && RetainInstance)
			{
				Dialog.SetDismissMessage(null);
			}
			base.OnDestroyView();
		}

		public virtual void ShowAllowingStateLoss(FragmentManager manager, string tag)
		{
			FragmentTransaction ft = manager.BeginTransaction();
			ft.Add(this, tag);
			ft.CommitAllowingStateLoss();
		}

		public void OnShow(IDialogInterface dialog)
		{
			if (View != null)
			{
				ScrollView vMessageScrollView = (ScrollView) View.FindViewById(Resource.Id.sdl_message_scrollview);
				ListView vListView = (ListView) View.FindViewById(Resource.Id.sdl_list);
				FrameLayout vCustomViewNoScrollView = (FrameLayout) View.FindViewById(Resource.Id.sdl_custom);
				bool customViewNoScrollViewScrollable = false;
				if (vCustomViewNoScrollView.ChildCount > 0)
				{
					View firstChild = vCustomViewNoScrollView.GetChildAt(0);
					if (firstChild is ViewGroup)
					{
						customViewNoScrollViewScrollable = IsScrollable((ViewGroup) firstChild);
					}
				}
				bool listViewScrollable = IsScrollable(vListView);
				bool messageScrollable = IsScrollable(vMessageScrollView);
				bool scrollable = listViewScrollable || messageScrollable || customViewNoScrollViewScrollable;
				ModifyButtonsBasedOnScrollableContent(scrollable);
			}
		}

		public override void OnCancel(IDialogInterface dialog)
		{
			base.OnCancel(dialog);

			foreach (ISimpleDialogCancelListener listener in CancelListeners)
			{
				listener.OnCancelled(mRequestCode);
			}
		}

		/// <summary>
		/// Get dialog cancel listeners.
		/// There might be more than one cancel listener.
		/// </summary>
		/// <returns> Dialog cancel listeners
		/// @since 2.1.0 </returns>
		protected internal virtual IList CancelListeners
		{
			get
			{
                Type listenerInterface = typeof(ISimpleDialogCancelListener);
                Fragment targetFragment = TargetFragment;
                IList listeners = new List<ISimpleDialogCancelListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((ISimpleDialogCancelListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((ISimpleDialogCancelListener)Activity);
                }

                return Java.Util.Collections.UnmodifiableList(listeners);
			}
		}

        //protected internal virtual IList<ISimpleDialogCancelListener> CancelListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<ISimpleDialogCancelListener>(typeof(ISimpleDialogCancelListener));
        //    }
        //}

		/// <summary>
		/// Utility method for acquiring all listeners of some type for current instance of DialogFragment
		/// </summary>
		/// <param name="listenerInterface"> Interface of the desired listeners </param>
		/// <returns> Unmodifiable list of listeners
		/// @since 2.1.0 </returns>
        //protected internal virtual IList<T> GetDialogListeners<T>(Type listenerInterface)
        //{
        //    Fragment targetFragment = TargetFragment;
        //    IList<T> listeners = new List<T>(2);
        //    if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
        //    {
        //        listeners.Add((T)targetFragment);
        //    }
        //    if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
        //    {
        //        listeners.Add((T)Activity);
        //    }

        //    return Java.Util.Collections.UnmodifiableList(listeners);
        //}

		/// <summary>
		/// Button divider should be shown only if the content is scrollable.
		/// </summary>
		private void ModifyButtonsBasedOnScrollableContent(bool scrollable)
		{
			if (View == null)
			{
				return;
			}
			View vButtonDivider = View.FindViewById(Resource.Id.sdl_button_divider);
			View vButtonsBottomSpace = View.FindViewById(Resource.Id.sdl_buttons_bottom_space);
			View vDefaultButtons = View.FindViewById(Resource.Id.sdl_buttons_default);
			View vStackedButtons = View.FindViewById(Resource.Id.sdl_buttons_stacked);
            if (vDefaultButtons.Visibility == ViewStates.Gone && vStackedButtons.Visibility == ViewStates.Gone)
			{
				// no buttons
                vButtonDivider.Visibility = ViewStates.Gone;
                vButtonsBottomSpace.Visibility = ViewStates.Gone;
			}
			else if (scrollable)
			{
                vButtonDivider.Visibility = ViewStates.Visible;
                vButtonsBottomSpace.Visibility = ViewStates.Gone;
			}
			else
			{
                vButtonDivider.Visibility = ViewStates.Gone;
                vButtonsBottomSpace.Visibility = ViewStates.Visible;
			}
		}

		private bool IsScrollable(ViewGroup listView)
		{
			int totalHeight = 0;
			for (int i = 0; i < listView.ChildCount; i++)
			{
				totalHeight += listView.GetChildAt(i).MeasuredHeight;
			}
			return listView.MeasuredHeight < totalHeight;
		}

		/// <summary>
		/// This method resolves the current theme declared in the manifest
		/// </summary>
		private void ResolveTheme()
		{
			try
			{
				TypedValue val = new TypedValue();

				//Reading attr value from current theme
				Activity.Theme.ResolveAttribute(Resource.Attribute.isLightTheme, val, true);

				//Passing the resource ID to TypedArray to get the attribute value
				TypedArray arr = Activity.ObtainStyledAttributes(val.Data, new int[]{ Resource.Attribute.isLightTheme });
				darkTheme = !arr.GetBoolean(0, false);
				arr.Recycle();
			}
			catch (RuntimeException e)
			{
				//Resource not found , so sticking to light theme
				darkTheme = false;
			}
		}

		/// <summary>
		/// Custom dialog builder
		/// </summary>
		public class Builder
		{
			public Context mContext;
            public ViewGroup mContainer;
            public LayoutInflater mInflater;
            public string mTitle = null;
            public string mPositiveButtonText;
            public View.IOnClickListener mPositiveButtonListener;
            public EventHandler mPositiveButtonHandler;
            public string mNegativeButtonText;
            public View.IOnClickListener mNegativeButtonListener;
            public EventHandler mNegativeButtonHandler;
            public string mNeutralButtonText;
            public View.IOnClickListener mNeutralButtonListener;
            public EventHandler mNeutralButtonHandler;
            public string mMessage;
            public View mCustomView;
            public IListAdapter mListAdapter;
            public int mListCheckedItemIdx;
            public ChoiceMode mChoiceMode;
            public int[] mListCheckedItemMultipleIds;
            public AdapterView.IOnItemClickListener mOnItemClickListener;
            public EventHandler<AdapterView.ItemClickEventArgs> mOnItemClickHandler;

			public Builder(Context context, LayoutInflater inflater, ViewGroup container)
			{
				this.mContext = context;
				this.mContainer = container;
				this.mInflater = inflater;
			}

			public virtual LayoutInflater LayoutInflater
			{
				get
				{
					return mInflater;
				}
			}

			public virtual Builder SetTitle(int titleId)
			{
				this.mTitle = mContext.GetText(titleId);
				return this;
			}

            public virtual Builder SetTitle(string title)
            {
                this.mTitle = title;
                return this;
            }

			public virtual Builder SetPositiveButton(int textId, View.IOnClickListener listener)
			{
                mPositiveButtonText = mContext.GetText(textId);
				mPositiveButtonListener = listener;
				return this;
			}

            public virtual Builder SetPositiveButton(string text, View.IOnClickListener listener)
            {
                mPositiveButtonText = text;
                mPositiveButtonListener = listener;
                return this;
            }

            public virtual Builder SetPositiveButton(string text, EventHandler handler)
            {
                mPositiveButtonText = text;
                mPositiveButtonHandler = handler;
                return this;
            }

			public virtual Builder SetNegativeButton(int textId, View.IOnClickListener listener)
			{
                mNegativeButtonText = mContext.GetText(textId);
				mNegativeButtonListener = listener;
				return this;
			}

            public virtual Builder SetNegativeButton(string text, View.IOnClickListener listener)
            {
                mNegativeButtonText = text;
                mNegativeButtonListener = listener;
                return this;
            }

            public virtual Builder SetNegativeButton(string text, EventHandler handler)
            {
                mNegativeButtonText = text;
                mNegativeButtonHandler = handler;
                return this;
            }

			public virtual Builder SetNeutralButton(int textId, View.IOnClickListener listener)
			{
                mNeutralButtonText = mContext.GetText(textId);
				mNeutralButtonListener = listener;
				return this;
			}

            public virtual Builder SetNeutralButton(string text, View.IOnClickListener listener)
            {
                mNeutralButtonText = text;
                mNeutralButtonListener = listener;
                return this;
            }

            public virtual Builder SetNeutralButton(string text, EventHandler handler)
            {
                mNeutralButtonText = text;
                mNeutralButtonHandler = handler;
                return this;
            }

			public virtual Builder SetMessage(int messageId)
			{
                mMessage = mContext.GetText(messageId);
				return this;
			}

            public virtual Builder SetMessage(string message)
            {
                mMessage = message;
                return this;
            }

			public virtual Builder SetItems(IListAdapter listAdapter, int[] checkedItemIds, ChoiceMode choiceMode, AdapterView.IOnItemClickListener listener)
			{
				mListAdapter = listAdapter;
				mListCheckedItemMultipleIds = checkedItemIds;
				mOnItemClickListener = listener;
				mChoiceMode = choiceMode;
				mListCheckedItemIdx = -1;
				return this;
			}

            public virtual Builder SetItems(IListAdapter listAdapter, int[] checkedItemIds, ChoiceMode choiceMode, EventHandler<AdapterView.ItemClickEventArgs> handler)
            {
                mListAdapter = listAdapter;
                mListCheckedItemMultipleIds = checkedItemIds;
                mOnItemClickHandler = handler;
                mChoiceMode = choiceMode;
                mListCheckedItemIdx = -1;
                return this;
            }

			/// <summary>
			/// Set list
			/// </summary>
			/// <param name="checkedItemIdx"> Item check by default, -1 if no item should be checked </param>
			public virtual Builder SetItems(IListAdapter listAdapter, int checkedItemIdx, AdapterView.IOnItemClickListener listener)
			{
				mListAdapter = listAdapter;
				mOnItemClickListener = listener;
				mListCheckedItemIdx = checkedItemIdx;
				mChoiceMode = ChoiceMode.None;
				return this;
			}

            public virtual Builder SetItems(IListAdapter listAdapter, int checkedItemIdx, EventHandler<AdapterView.ItemClickEventArgs> handler)
            {
                mListAdapter = listAdapter;
                mOnItemClickHandler = handler;
                mListCheckedItemIdx = checkedItemIdx;
                mChoiceMode = ChoiceMode.None;
                return this;
            }

			public virtual Builder SetView(View view)
			{
				mCustomView = view;
				return this;
			}

			public virtual View Create()
			{
				LinearLayout content = (LinearLayout) mInflater.Inflate(Resource.Layout.sdl_dialog, mContainer, false);
				TextView vTitle = (TextView) content.FindViewById(Resource.Id.sdl_title);
				TextView vMessage = (TextView) content.FindViewById(Resource.Id.sdl_message);
				FrameLayout vCustomView = (FrameLayout) content.FindViewById(Resource.Id.sdl_custom);
				Button vPositiveButton = (Button) content.FindViewById(Resource.Id.sdl_button_positive);
				Button vNegativeButton = (Button) content.FindViewById(Resource.Id.sdl_button_negative);
				Button vNeutralButton = (Button) content.FindViewById(Resource.Id.sdl_button_neutral);
				Button vPositiveButtonStacked = (Button) content.FindViewById(Resource.Id.sdl_button_positive_stacked);
				Button vNegativeButtonStacked = (Button) content.FindViewById(Resource.Id.sdl_button_negative_stacked);
				Button vNeutralButtonStacked = (Button) content.FindViewById(Resource.Id.sdl_button_neutral_stacked);
				View vButtonsDefault = content.FindViewById(Resource.Id.sdl_buttons_default);
				View vButtonsStacked = content.FindViewById(Resource.Id.sdl_buttons_stacked);
				ListView vList = (ListView) content.FindViewById(Resource.Id.sdl_list);

				Typeface regularFont = TypefaceHelper.Get(mContext, "Roboto-Regular");
				Typeface mediumFont = TypefaceHelper.Get(mContext, "Roboto-Medium");

				Set(vTitle, mTitle, mediumFont);
				Set(vMessage, mMessage, regularFont);
				SetPaddingOfTitleAndMessage(vTitle, vMessage);

				if (mCustomView != null)
				{
					vCustomView.AddView(mCustomView);
				}
				if (mListAdapter != null)
				{
					vList.Adapter = mListAdapter;

                    if (mOnItemClickListener != null)
                    {
                        vList.OnItemClickListener = mOnItemClickListener;
                    }
                    if (mOnItemClickHandler != null)
                    {
                        vList.ItemClick += mOnItemClickHandler;

                    }
					if (mListCheckedItemIdx != -1)
					{
						vList.SetSelection(mListCheckedItemIdx);
					}
					if (mListCheckedItemMultipleIds != null)
					{
						vList.ChoiceMode = mChoiceMode;
						foreach (int i in mListCheckedItemMultipleIds)
						{
							vList.SetItemChecked(i, true);
						}
					}
				}

				if (ShouldStackButtons())
				{
					Set(vPositiveButtonStacked, mPositiveButtonText, mediumFont, mPositiveButtonListener);
					Set(vNegativeButtonStacked, mNegativeButtonText, mediumFont, mNegativeButtonListener);
					Set(vNeutralButtonStacked, mNeutralButtonText, mediumFont, mNeutralButtonListener);
                    Set(vPositiveButtonStacked, mPositiveButtonText, mediumFont, mPositiveButtonHandler);
                    Set(vNegativeButtonStacked, mNegativeButtonText, mediumFont, mNegativeButtonHandler);
                    Set(vNeutralButtonStacked, mNeutralButtonText, mediumFont, mNeutralButtonHandler);
					vButtonsDefault.Visibility = ViewStates.Gone;
                    vButtonsStacked.Visibility = ViewStates.Visible;
				}
				else
				{
					Set(vPositiveButton, mPositiveButtonText, mediumFont, mPositiveButtonListener);
					Set(vNegativeButton, mNegativeButtonText, mediumFont, mNegativeButtonListener);
					Set(vNeutralButton, mNeutralButtonText, mediumFont, mNeutralButtonListener);
                    Set(vPositiveButton, mPositiveButtonText, mediumFont, mPositiveButtonHandler);
                    Set(vNegativeButton, mNegativeButtonText, mediumFont, mNegativeButtonHandler);
                    Set(vNeutralButton, mNeutralButtonText, mediumFont, mNeutralButtonHandler);
                    vButtonsDefault.Visibility = ViewStates.Visible;
                    vButtonsStacked.Visibility = ViewStates.Gone;
				}
                if (TextUtils.IsEmpty(mPositiveButtonText) && TextUtils.IsEmpty(mNegativeButtonText) && TextUtils.IsEmpty(mNeutralButtonText))
				{
                    vButtonsDefault.Visibility = ViewStates.Gone;
				}

				return content;
			}

			/// <summary>
			/// Padding is different if there is only title, only message or both.
			/// </summary>
			internal virtual void SetPaddingOfTitleAndMessage(TextView vTitle, TextView vMessage)
			{
				int grid6 = mContext.Resources.GetDimensionPixelSize(Resource.Dimension.grid_6);
                int grid4 = mContext.Resources.GetDimensionPixelSize(Resource.Dimension.grid_4);
                if (!TextUtils.IsEmpty(mTitle) && !TextUtils.IsEmpty(mMessage))
				{
					vTitle.SetPadding(grid6, grid6, grid6, grid4);
                    vMessage.SetPadding(grid6, 0, grid6, grid4);
				}
                else if (TextUtils.IsEmpty(mTitle))
				{
                    vMessage.SetPadding(grid6, grid4, grid6, grid4);
				}
                else if (TextUtils.IsEmpty(mMessage))
				{
                    vTitle.SetPadding(grid6, grid6, grid6, grid4);
				}
			}

			internal virtual bool ShouldStackButtons()
			{
				return ShouldStackButton(mPositiveButtonText) || ShouldStackButton(mNegativeButtonText) || ShouldStackButton(mNeutralButtonText);
			}

            internal virtual bool ShouldStackButton(string text)
            {
                const int MAX_BUTTON_CHARS = 12; // based on observation, could be done better with measuring widths
                return text != null && text.ToCharArray().Length > MAX_BUTTON_CHARS;
            }

            internal virtual void Set(Button button, string text, Typeface font, View.IOnClickListener listener)
            {
                Set(button, text, font);
                if (listener != null)
                {
                    button.SetOnClickListener(listener);
                }
            }

            internal virtual void Set(Button button, string text, Typeface font, EventHandler handler)
            {
                Set(button, text, font);
                if (handler != null)
                {
                    button.Click += handler;
                }
            }

            internal virtual void Set(TextView textView, string text, Typeface font)
            {
                if (text != null)
                {
                    textView.Text = text;
                    textView.Typeface = font;
                }
                else
                {
                    textView.Visibility = ViewStates.Gone;
                }
            }
		}
	}
}