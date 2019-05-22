using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Java.Lang;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaterialDialogs
{
	/// <summary>
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	public class MaterialDialog : DialogBase, View.IOnClickListener, AdapterView.IOnItemClickListener
	{
        public event EventHandler DialogClickEvent;

		protected internal readonly View view;
		protected internal readonly Builder mBuilder;
		protected internal ListView listView;
		protected internal ImageView icon;
		protected internal TextView title;
		protected internal View titleFrame;
		protected internal FrameLayout customViewFrame;
		protected internal ProgressBar mProgress;
		protected internal TextView mProgressLabel;
		protected internal TextView mProgressMinMax;
		protected internal TextView content;

		protected internal View positiveButton;
		protected internal View neutralButton;
		protected internal View negativeButton;
		protected internal bool isStacked;
		protected internal int defaultItemColor;
		protected internal ListType listType;
		protected internal List<int> selectedIndicesList;

		public MaterialDialog(Builder builder) 
            : base(DialogInit.GetTheme(builder))
		{
			mBuilder = builder;
			LayoutInflater inflater = LayoutInflater.From(mBuilder.context);
			this.view = inflater.Inflate(DialogInit.GetInflateLayout(builder), null);
			DialogInit.Init(this);
		}

		protected internal static GravityFlags GravityIntToGravity(GravityEnum gravity)
		{
			switch (gravity)
			{
				case MaterialDialogs.GravityEnum.CENTER:
					return GravityFlags.CenterHorizontal;
				case MaterialDialogs.GravityEnum.END:
					if (Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
					{
						return GravityFlags.Right;
					}
					return GravityFlags.End;
				default:
					if (Android.OS.Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
					{
						return GravityFlags.Left;
					}
					return GravityFlags.Start;
			}
		}

		public void OnShow(IDialogInterface dialog)
		{
			base.OnShow(dialog); // calls any external show listeners
			CheckIfStackingNeeded();
			InvalidateCustomViewAssociations();
		}

		protected internal void SetTypeface(TextView text, Typeface t)
		{
			if (t == null)
			{
				return;
			}

			PaintFlags flags = text.PaintFlags | PaintFlags.SubpixelText;
			text.PaintFlags = flags;
			text.Typeface = t;
		}

		/// <summary>
		/// To account for scrolling content and overscroll glows, the frame padding/margins sometimes
		/// must be set on inner views. This is dependent on the visibility of the title bar and action
		/// buttons. This method determines where the padding or margins are needed and applies them.
		/// </summary>
		protected internal void UpdateFramePadding()
		{
			Resources r = Context.Resources;
			int framePadding = r.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);

			View contentScrollView = view.FindViewById(Resource.Id.contentScrollView);
			if (contentScrollView != null)
			{
				int paddingTop = contentScrollView.PaddingTop;
				int paddingBottom = contentScrollView.PaddingBottom;
				if (!HasActionButtons())
				{
					paddingBottom = framePadding;
				}
				if (titleFrame.Visibility == ViewStates.Gone)
				{
					paddingTop = framePadding;
				}
				contentScrollView.SetPadding(contentScrollView.PaddingLeft, paddingTop, contentScrollView.PaddingRight, paddingBottom);
			}

			if (listView != null)
			{
				// Padding below title is reduced for divider.
				int titlePaddingBottom = (int) mBuilder.context.Resources.GetDimension(Resource.Dimension.md_title_frame_margin_bottom_list);
				titleFrame.SetPadding(titleFrame.PaddingLeft, titleFrame.PaddingTop, titleFrame.PaddingRight, titlePaddingBottom);
			}
		}

		/// <summary>
		/// Invalidates visibility of views for the presence of a custom view or list content
		/// </summary>
		protected internal void InvalidateCustomViewAssociations()
		{
			if (view.MeasuredWidth == 0)
			{
				return;
			}
			View contentScrollView = view.FindViewById(Resource.Id.contentScrollView);
			int contentHorizontalPadding = (int) mBuilder.context.Resources.GetDimension(Resource.Dimension.md_dialog_frame_margin);
			if (content != null)
			{
				content.SetPadding(contentHorizontalPadding, 0, contentHorizontalPadding, 0);
			}

			if (mBuilder.customView != null)
			{
				bool topScroll = CanViewOrChildScroll(customViewFrame.GetChildAt(0), false);
				bool bottomScroll = CanViewOrChildScroll(customViewFrame.GetChildAt(0), true);
				SetDividerVisibility(topScroll, bottomScroll);
			}
			else if ((mBuilder.items != null && mBuilder.items.Length > 0) || mBuilder.adapter != null)
			{
				if (contentScrollView != null)
				{
					contentScrollView.Visibility = mBuilder.content != null && mBuilder.content.ToString().Trim().Length > 0 ? ViewStates.Visible : ViewStates.Gone;
				}
				bool canScroll = titleFrame.Visibility == ViewStates.Visible && (CanListViewScroll() || CanContentScroll());
				SetDividerVisibility(canScroll, canScroll);
			}
			else
			{
				if (contentScrollView != null)
				{
					contentScrollView.Visibility = ViewStates.Visible;
				}
				bool canScroll = CanContentScroll();
				if (canScroll)
				{
					if (content != null)
					{
						int contentVerticalPadding = (int) mBuilder.context.Resources.GetDimension(Resource.Dimension.md_title_frame_margin_bottom);
						content.SetPadding(contentHorizontalPadding, contentVerticalPadding, contentHorizontalPadding, contentVerticalPadding);
					}

					// Same effect as when there's a ListView. Padding below title is reduced for divider.
					int titlePaddingBottom = (int) mBuilder.context.Resources.GetDimension(Resource.Dimension.md_title_frame_margin_bottom_list);
					titleFrame.SetPadding(titleFrame.PaddingLeft, titleFrame.PaddingTop, titleFrame.PaddingRight, titlePaddingBottom);
				}
				SetDividerVisibility(canScroll, canScroll);
			}
		}

		/// <summary>
		/// Set the visibility of the bottom divider and adjusts the layout margin,
		/// when the divider is visible the button bar bottom margin (8dp From
		/// http://www.google.com/design/spec/components/dialogs.html#dialogs-specs )
		/// is removed as it makes the button bar look off balanced with different amounts of padding
		/// above and below the divider.
		/// </summary>
		private void SetDividerVisibility(bool topVisible, bool bottomVisible)
		{
			topVisible = topVisible && titleFrame.Visibility == ViewStates.Visible;
			bottomVisible = bottomVisible && HasActionButtons();

			if (mBuilder.dividerColor == 0)
			{
				mBuilder.dividerColor = DialogUtils.ResolveColor(mBuilder.context, Resource.Attribute.md_divider_color);
			}
			if (mBuilder.dividerColor == 0)
			{
				mBuilder.dividerColor = DialogUtils.ResolveColor(Context, Resource.Attribute.md_divider);
			}

			View titleBarDivider = view.FindViewById(Resource.Id.titleBarDivider);
			if (topVisible)
			{
				titleBarDivider.Visibility = ViewStates.Visible;
				titleBarDivider.SetBackgroundColor(mBuilder.dividerColor);
			}
			else
			{
				titleBarDivider.Visibility = ViewStates.Gone;
			}

			View buttonBarDivider = view.FindViewById(Resource.Id.buttonBarDivider);
			if (bottomVisible)
			{
				buttonBarDivider.Visibility = ViewStates.Visible;
				buttonBarDivider.SetBackgroundColor((mBuilder.dividerColor));
				SetVerticalMargins(view.FindViewById(Resource.Id.buttonStackedFrame), 0, 0);
				SetVerticalMargins(view.FindViewById(Resource.Id.buttonDefaultFrame), 0, 0);
			}
			else
			{
				Resources r = Context.Resources;
				buttonBarDivider.Visibility = ViewStates.Gone;

				int BottomMargin = r.GetDimensionPixelSize(Resource.Dimension.md_button_frame_vertical_padding);

				/* Only enable the bottom margin if our available window space can hold the margin,
				   we don't want to enable this and cause the content to scroll, which is bad
				   experience itself but it also causes a vibrating window as this will keep getting
				   enabled/disabled over and over again.
				 */
				Rect maxWindowFrame = new Rect();
				Window.DecorView.GetWindowVisibleDisplayFrame(maxWindowFrame);
				int currentHeight = Window.DecorView.MeasuredHeight;
				if (currentHeight + BottomMargin < maxWindowFrame.Height())
				{
					SetVerticalMargins(view.FindViewById(Resource.Id.buttonStackedFrame), BottomMargin, BottomMargin);
					SetVerticalMargins(view.FindViewById(Resource.Id.buttonDefaultFrame), BottomMargin, BottomMargin);
				}
			}
		}

		/// <summary>
		/// Constructs the dialog's list content and sets up click listeners.
		/// </summary>
		private void InvalidateList()
		{
			if ((mBuilder.items == null || mBuilder.items.Length == 0) && mBuilder.adapter == null)
			{
				return;
			}

			// Hide content
			view.FindViewById(Resource.Id.contentScrollView).Visibility = mBuilder.content != null && mBuilder.content.ToString().Trim().Length > 0 ? ViewStates.Visible : ViewStates.Gone;

			// Set up list with adapter
			FrameLayout listViewContainer = (FrameLayout) view.FindViewById(Resource.Id.contentListViewFrame);
			listView.Adapter = mBuilder.adapter;
			if (listType != null || mBuilder.listCallbackCustom != null)
			{
				listView.OnItemClickListener = this;
			}
		}

		/// <summary>
		/// Find the view touching the bottom of this ViewGroup. Non visible children are ignored,
		/// however getChildDrawingOrder is not taking into account for simplicity and because it behaves
		/// inconsistently across platform versions.
		/// </summary>
		/// <returns> View touching the bottom of this viewgroup or null </returns>
		private static View GetBottomView(ViewGroup viewGroup)
		{
			View bottomView = null;
			for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
			{
				View child = viewGroup.GetChildAt(i);
				if (child.Visibility == ViewStates.Visible && child.Bottom == viewGroup.Bottom)
				{
					bottomView = child;
					break;
				}
			}
			return bottomView;
		}

		private static View GetTopView(ViewGroup viewGroup)
		{
			View topView = null;
			for (int i = viewGroup.ChildCount - 1; i >= 0; i--)
			{
				View child = viewGroup.GetChildAt(i);
				if (child.Visibility == ViewStates.Visible && child.Top == viewGroup.Top)
				{
					topView = child;
					break;
				}
			}
			return topView;
		}

		private static bool CanViewOrChildScroll(View view, bool atBottom)
		{
			if (view == null || !(view is ViewGroup))
			{
				return false;
			}
				/* Is the bottom view something that scrolls? */
			if (view is ScrollView)
			{
				ScrollView sv = (ScrollView) view;
				if (sv.ChildCount == 0)
				{
					return false;
				}
				int childHeight = sv.GetChildAt(0).MeasuredHeight;
				return sv.MeasuredHeight < childHeight;
			}
			else if (view is AdapterView)
			{
				return CanAdapterViewScroll((AdapterView) view);
			}
			else if (view is WebView)
			{
				return CanWebViewScroll((WebView) view);
			}
			else if (IsRecyclerView(view))
			{
				return RecyclerUtil.CanRecyclerViewScroll(view);
			}
			else
			{
				if (atBottom)
				{
					return CanViewOrChildScroll(GetBottomView((ViewGroup) view), true);
				}
				else
				{
					return CanViewOrChildScroll(GetTopView((ViewGroup) view), false);
				}
			}
		}

		private static bool IsRecyclerView(View view)
		{
			bool isRecyclerView = false;
			try
			{
				Type.GetType("android.support.v7.widget.RecyclerView");

				// We got here, so now we can safely check
				isRecyclerView = RecyclerUtil.IsRecyclerView(view);
			}
			catch (ClassNotFoundException ignored)
			{
			}

			return isRecyclerView;
		}

		private static bool CanWebViewScroll(WebView view)
		{
			return view.MeasuredHeight > view.ContentHeight;
		}

		private static bool CanAdapterViewScroll(AdapterView lv)
		{
			/* Force it to layout it's children */
			if (lv.LastVisiblePosition == -1)
			{
				return false;
			}

			/* We can scroll if the first or last item is not visible */
			bool firstItemVisible = lv.FirstVisiblePosition == 0;
			bool lastItemVisible = lv.LastVisiblePosition == lv.Count - 1;

			if (firstItemVisible && lastItemVisible)
			{
				/* Or the first item's top is above or own top */
				if (lv.GetChildAt(0).Top < lv.PaddingTop)
				{
					return true;
				}

				/* or the last item's bottom is beyond our own bottom */
				return lv.GetChildAt(lv.ChildCount - 1).Bottom > lv.Height - lv.PaddingBottom;
			}

			return true;
		}

		private bool CanListViewScroll()
		{
			return CanAdapterViewScroll(listView);
		}

		public void OnItemClick(AdapterView parent, View view, int position, long id)
		{
			if (mBuilder.listCallbackCustom != null)
			{
				// Custom adapter
				string text = null;
				if (view is TextView)
				{
					text = ((TextView) view).Text;
				}
				mBuilder.listCallbackCustom.OnSelection(this, view, position, text);
			}
			else if (listType == null || listType == ListType.REGULAR)
			{
				// Default adapter, non choice mode
				if (mBuilder.autoDismiss)
				{
					// If auto dismiss is enabled, dismiss the dialog when a list item is selected
					Dismiss();
				}
				mBuilder.listCallback.OnSelection(this, view, position, mBuilder.items[position]);
			}
			else
			{
				// Default adapter, choice mode
				if (listType == ListType.MULTI)
				{
					bool shouldBeChecked = !selectedIndicesList.Contains(position);
					CheckBox cb = (CheckBox)((LinearLayout) view).GetChildAt(0);
					if (shouldBeChecked)
					{
						// Add the selection to the states first so the callback includes it (when alwaysCallMultiChoiceCallback)
						selectedIndicesList.Add(position);
						if (mBuilder.alwaysCallMultiChoiceCallback)
						{
							// If the checkbox wasn't previously selected, and the callback returns true, add it to the states and check it
							if (SendMultichoiceCallback())
							{
								cb.Checked = true;
							}
							else
							{
								// The callback cancelled selection, remove it From the states
                                selectedIndicesList.Remove(position);
							}
						}
						else
						{
							// The callback was not used to check if selection is allowed, just select it
							cb.Checked = true;
						}
					}
					else
					{
                        Console.WriteLine("THIS POSITION IS -> " + position);
						// The checkbox was unchecked
                        selectedIndicesList.Remove(position);
						cb.Checked = false;
						if (mBuilder.alwaysCallMultiChoiceCallback)
						{
							SendMultichoiceCallback();
						}
					}
				}
				else if (listType == ListType.SINGLE)
				{
					bool allowSelection = true;
					if (mBuilder.autoDismiss && mBuilder.positiveText == null)
					{
						// If auto dismiss is enabled, and no action button is visible to approve the selection, dismiss the dialog
						Dismiss();
						// Don't allow the selection to be updated since the dialog is being dismissed anyways
						allowSelection = false;
						// Update selected index and send callback
						mBuilder.selectedIndex = position;
						SendSingleChoiceCallback(view);
					}
					else if (mBuilder.alwaysCallSingleChoiceCallback)
					{
						int oldSelected = mBuilder.selectedIndex;
						// Temporarily set the new index so the callback uses the right one
						mBuilder.selectedIndex = position;
						// Only allow the radio button to be checked if the callback returns true
						allowSelection = SendSingleChoiceCallback(view);
						// Restore the old selected index, so the state is updated below
						mBuilder.selectedIndex = oldSelected;
					}
					// Update the checked states
					if (allowSelection && mBuilder.selectedIndex != position)
					{
						mBuilder.selectedIndex = position;
						((MaterialDialogAdapter) mBuilder.adapter).NotifyDataSetChanged();
					}
				}

			}
		}

        //public class NotImplementedException : System.Exception
        //{
        //    public NotImplementedException(string message) 
        //        : base(message)
        //    {

        //    }
        //}

		public class DialogException : WindowManagerBadTokenException
		{
			public DialogException(string message) 
                : base(message)
			{
			}
		}

		/// <summary>
		/// Detects whether or not the content TextView can be scrolled.
		/// </summary>
		private bool CanContentScroll()
		{
			ScrollView scrollView = (ScrollView) view.FindViewById(Resource.Id.contentScrollView);
			if (scrollView == null)
			{
				return false;
			}
			int childHeight = content.MeasuredHeight;
			return scrollView.MeasuredHeight < childHeight;
		}

		/// <summary>
		/// Measures the action button's and their text to decide whether or not the button should be stacked.
		/// </summary>
		private void CheckIfStackingNeeded()
		{
			if (NumberOfActionButtons() <= 1)
			{
				return;
			}
			else if (mBuilder.forceStacking)
			{
				isStacked = true;
				InvalidateActions();
				return;
			}
			isStacked = false;
			int buttonsWidth = 0;

			positiveButton.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
			neutralButton.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);
			negativeButton.Measure((int)MeasureSpecMode.Unspecified, (int)MeasureSpecMode.Unspecified);

			if (mBuilder.positiveText != null)
			{
				buttonsWidth += positiveButton.MeasuredWidth;
			}
			if (mBuilder.neutralText != null)
			{
				buttonsWidth += neutralButton.MeasuredWidth;
			}
			if (mBuilder.negativeText != null)
			{
				buttonsWidth += negativeButton.MeasuredWidth;
			}

			int buttonFrameWidth = view.FindViewById(Resource.Id.buttonDefaultFrame).Width;
			isStacked = buttonsWidth > buttonFrameWidth;
			InvalidateActions();
		}

		protected internal Drawable ListSelector
		{
			get
			{
				if (mBuilder.listSelector != 0)
				{
					return mBuilder.context.Resources.GetDrawable(mBuilder.listSelector);
				}
				Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, Resource.Attribute.md_list_selector);
				if (d != null)
				{
					return d;
				}
				return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_list_selector);
			}
		}

		private Drawable GetButtonSelector(DialogAction which)
		{
			if (isStacked)
			{
				if (mBuilder.btnSelectorStacked != 0)
				{
					return mBuilder.context.Resources.GetDrawable(mBuilder.btnSelectorStacked);
				}
				Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, Resource.Attribute.md_btn_stacked_selector);
				if (d != null)
				{
					return d;
				}
				return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_stacked_selector);
			}
			else
			{
				switch (which)
				{
					default:
					{
						if (mBuilder.btnSelectorPositive != 0)
						{
							return mBuilder.context.Resources.GetDrawable(mBuilder.btnSelectorPositive);
						}
						Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, Resource.Attribute.md_btn_positive_selector);
						if (d != null)
						{
							return d;
						}
						return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_positive_selector);
					}
					case MaterialDialogs.DialogAction.NEUTRAL:
					{
						if (mBuilder.btnSelectorNeutral != 0)
						{
							return mBuilder.context.Resources.GetDrawable(mBuilder.btnSelectorNeutral);
						}
						Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, Resource.Attribute.md_btn_neutral_selector);
						if (d != null)
						{
							return d;
						}
						return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_neutral_selector);
					}
					case MaterialDialogs.DialogAction.NEGATIVE:
					{
						if (mBuilder.btnSelectorNegative != 0)
						{
							return mBuilder.context.Resources.GetDrawable(mBuilder.btnSelectorNegative);
						}
						Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, Resource.Attribute.md_btn_negative_selector);
						if (d != null)
						{
							return d;
						}
						return DialogUtils.ResolveDrawable(Context, Resource.Attribute.md_btn_negative_selector);
					}
				}
			}
		}

		/// <summary>
		/// Invalidates the positive/neutral/negative action buttons. Decides whether they should be visible
		/// and sets their properties (such as height, text color, etc.).
		/// </summary>
		protected internal bool InvalidateActions()
		{
			if (!HasActionButtons())
			{
				// If the dialog is a plain list dialog, no buttons are shown.
				view.FindViewById(Resource.Id.buttonDefaultFrame).Visibility = ViewStates.Gone;
				view.FindViewById(Resource.Id.buttonStackedFrame).Visibility = ViewStates.Gone;
				InvalidateList();
				return false;
			}

			if (isStacked)
			{
				view.FindViewById(Resource.Id.buttonDefaultFrame).Visibility = ViewStates.Gone;
				view.FindViewById(Resource.Id.buttonStackedFrame).Visibility = ViewStates.Visible;
			}
			else
			{
				view.FindViewById(Resource.Id.buttonDefaultFrame).Visibility = ViewStates.Visible;
				view.FindViewById(Resource.Id.buttonStackedFrame).Visibility = ViewStates.Gone;
			}

			positiveButton = view.FindViewById(isStacked ? Resource.Id.buttonStackedPositive : Resource.Id.buttonDefaultPositive);
			if (mBuilder.positiveText != null)
			{
				TextView positiveTextView = (TextView)((FrameLayout) positiveButton).GetChildAt(0);
				SetTypeface(positiveTextView, mBuilder.mediumFont);
				positiveTextView.Text = mBuilder.positiveText;
				positiveTextView.SetTextColor(GetActionTextStateList(mBuilder.positiveColor));
				SetBackgroundCompat(positiveButton, GetButtonSelector(DialogAction.POSITIVE));
				positiveButton.Tag = POSITIVE;
				positiveButton.SetOnClickListener(this);
				if (isStacked)
				{
					positiveTextView.Gravity = GravityIntToGravity(mBuilder.btnStackedGravity);
				}
			}
			else
			{
				positiveButton.Visibility = ViewStates.Gone;
			}

			neutralButton = view.FindViewById(isStacked ? Resource.Id.buttonStackedNeutral : Resource.Id.buttonDefaultNeutral);
			if (mBuilder.neutralText != null)
			{
				TextView neutralTextView = (TextView)((FrameLayout) neutralButton).GetChildAt(0);
				SetTypeface(neutralTextView, mBuilder.mediumFont);
				neutralButton.Visibility = ViewStates.Visible;
				neutralTextView.SetTextColor(GetActionTextStateList(mBuilder.neutralColor));
				SetBackgroundCompat(neutralButton, GetButtonSelector(DialogAction.NEUTRAL));
				neutralTextView.Text = mBuilder.neutralText;
				neutralButton.Tag = NEUTRAL;
				neutralButton.SetOnClickListener(this);
				if (isStacked)
				{
					neutralTextView.Gravity = GravityIntToGravity(mBuilder.btnStackedGravity);
				}
			}
			else
			{
				neutralButton.Visibility = ViewStates.Gone;
			}

			negativeButton = view.FindViewById(isStacked ? Resource.Id.buttonStackedNegative : Resource.Id.buttonDefaultNegative);
			if (mBuilder.negativeText != null)
			{
				TextView negativeTextView = (TextView)((FrameLayout) negativeButton).GetChildAt(0);
				SetTypeface(negativeTextView, mBuilder.mediumFont);
				negativeButton.Visibility = ViewStates.Visible;
				negativeTextView.SetTextColor(GetActionTextStateList(mBuilder.negativeColor));
				SetBackgroundCompat(negativeButton, GetButtonSelector(DialogAction.NEGATIVE));
				negativeTextView.Text = mBuilder.negativeText;
				negativeButton.Tag = NEGATIVE;
				negativeButton.SetOnClickListener(this);

				if (!isStacked)
				{
					RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent, (int) Context.Resources.GetDimension(Resource.Dimension.md_button_height));
					if (mBuilder.positiveText != null)
					{
						if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
						{
							lp.AddRule(LayoutRules.StartOf, Resource.Id.buttonDefaultPositive);
						}
						else
						{
							lp.AddRule(LayoutRules.LeftOf, Resource.Id.buttonDefaultPositive);
						}
					}
					else
					{
						if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1)
						{
							lp.AddRule(LayoutRules.AlignParentEnd);
						}
						else
						{
							lp.AddRule(LayoutRules.AlignParentRight);
						}
					}
					negativeButton.LayoutParameters = lp;
				}
				else
				{
					negativeTextView.Gravity = GravityIntToGravity(mBuilder.btnStackedGravity);
				}
			}
			else
			{
				negativeButton.Visibility = ViewStates.Gone;
			}

			InvalidateList();
			return true;
		}



		private bool SendSingleChoiceCallback(View v)
		{
			string text = null;
			if (mBuilder.selectedIndex >= 0)
			{
				text = mBuilder.items[mBuilder.selectedIndex];
			}
			return mBuilder.listCallbackSingleChoice.OnSelection(this, v, mBuilder.selectedIndex, text);
		}

		private bool SendMultichoiceCallback()
		{
			selectedIndicesList.OrderBy(x => x); // make sure the indicies are in order
			IList<string> selectedTitles = new List<string>();
			foreach (int i in selectedIndicesList)
			{
				selectedTitles.Add(mBuilder.items[i]);
			}
			return mBuilder.listCallbackMultiChoice.OnSelection(this, selectedIndicesList.ToArray(), selectedTitles.ToArray());
		}

        void buttonTextView_Click(object sender, EventArgs e)
        {
            if (mBuilder.autoDismiss)
            {
                Dismiss();
            }
        }

		public void OnClick(View v)
		{
			string tag = (string)v.Tag;

            Console.WriteLine("is this getting called?");
            Console.WriteLine("TAG -> " + tag);

			switch (tag)
			{
				case POSITIVE:
				{
					if (mBuilder.callback != null)
					{
						mBuilder.callback.OnPositive(this);
					}
                    if (mBuilder.PositiveHandler != null)
                    {
                        mBuilder.OnPositiveButtonClicked(null);
                    }
					if (mBuilder.listCallbackSingleChoice != null)
					{
						SendSingleChoiceCallback(v);
					}
					if (mBuilder.listCallbackMultiChoice != null)
					{
						SendMultichoiceCallback();
					}
					if (mBuilder.autoDismiss)
					{
						Dismiss();
					}
					break;
				}
				case NEGATIVE:
				{
					if (mBuilder.callback != null)
					{
						mBuilder.callback.OnNegative(this);
					}
                    if (mBuilder.NegativeHandler != null)
                    {
                        mBuilder.OnNegativeButtonClicked(null);
                    }
					if (mBuilder.autoDismiss)
					{
						Dismiss();
					}
					break;
				}
				case NEUTRAL:
				{
					if (mBuilder.callback != null)
					{
						mBuilder.callback.OnNeutral(this);
					}
                    if (mBuilder.NegativeHandler != null)
                    {
                        mBuilder.OnNegativeButtonClicked(null);
                    }
					if (mBuilder.autoDismiss)
					{
						Dismiss();
					}
					break;
				}
			}
		}

		/// <summary>
		/// The class used to construct a MaterialDialog.
		/// </summary>
		public class Builder
		{
            public delegate void ButtonClickEventHandler(object sender, EventArgs e);

            public EventHandler PositiveHandler = null;
            public EventHandler NegativeHandler = null;
            public EventHandler NeutralHandler = null;

            public void OnPositiveButtonClicked(EventArgs e)
            {
                PositiveHandler(this, e);
            }
            public void OnNegativeButtonClicked(EventArgs e)
            {
                NegativeHandler(this, e);
            }
            public void OnNeutralButtonClicked(EventArgs e)
            {
                NeutralHandler(this, e);
            }

			protected internal readonly Context context;
			protected internal string title;
			protected internal GravityEnum titleGravity = GravityEnum.START;
			protected internal GravityEnum contentGravity = GravityEnum.START;
			protected internal GravityEnum btnStackedGravity = GravityEnum.END;
			protected internal Color titleColor = Color.Rgb(-1, -1, -1);
            protected internal Color contentColor = Color.Rgb(-1, -1, -1);
			protected internal string content;
			protected internal string[] items;
			protected internal string positiveText;
			protected internal string neutralText;
			protected internal string negativeText;
			protected internal View customView;
			protected internal Color accentColor;
            protected internal Color positiveColor;
            protected internal Color negativeColor;
            protected internal Color neutralColor;
			protected internal ButtonCallback callback;

			protected internal IListCallback listCallback;
			protected internal IListCallbackSingleChoice listCallbackSingleChoice;
			protected internal IListCallbackMultiChoice listCallbackMultiChoice;
			protected internal IListCallback listCallbackCustom;
			protected internal bool alwaysCallMultiChoiceCallback = false;
			protected internal bool alwaysCallSingleChoiceCallback = false;
			protected internal DialogTheme theme = DialogTheme.LIGHT;
			protected internal bool cancelable = true;
			protected internal float contentLineSpacingMultiplier = 1.3f;
			protected internal int selectedIndex = -1;
			protected internal int[] selectedIndices = null;
			protected internal bool autoDismiss = true;
			protected internal Typeface regularFont;
			protected internal Typeface mediumFont;
			protected internal bool useCustomFonts;
			protected internal Drawable icon;
			protected internal bool limitIconToDefaultSize;
			protected internal int maxIconSize = -1;
			protected internal IListAdapter adapter;
			protected internal IDialogInterfaceOnDismissListener dismissListener;
			protected internal IDialogInterfaceOnCancelListener cancelListener;
			protected internal IDialogInterfaceOnKeyListener keyListener;
			protected internal IDialogInterfaceOnShowListener showListener;
			protected internal bool forceStacking;
			protected internal bool wrapCustomViewInScroll;
			protected internal Color dividerColor;
            protected internal Color backgroundColor;
			protected internal int itemColor;
			protected internal bool mIndeterminateProgress;
			protected internal bool mShowMinMax;
			protected internal int mProgress = -2;
			protected internal int mProgressMax = 0;

			// Since 0 is black and -1 is white, no default value is good for indicating if a color was set.
			// So this is a decent solution to this problem.
			protected internal bool titleColorSet;
			protected internal bool contentColorSet;
			protected internal bool itemColorSet;

			protected internal int listSelector;
			protected internal int btnSelectorStacked;
			protected internal int btnSelectorPositive;
			protected internal int btnSelectorNeutral;
			protected internal int btnSelectorNegative;

			public Builder(Context context)
			{
				this.context = context;
				Color materialBlue = context.Resources.GetColor(Resource.Color.md_material_blue_600);
				if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
				{
					TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ Android.Resource.Attribute.ColorAccent });
					try
					{
						this.accentColor = a.GetColor(0, materialBlue);
						this.positiveColor = this.accentColor;
						this.negativeColor = this.accentColor;
						this.neutralColor = this.accentColor;
					}
					catch (System.Exception e)
					{
						this.accentColor = materialBlue;
						this.positiveColor = materialBlue;
						this.negativeColor = materialBlue;
						this.neutralColor = materialBlue;
					}
					finally
					{
						a.Recycle();
					}
				}
				else
				{
					TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ Resource.Attribute.colorAccent });
					try
					{
						this.accentColor = a.GetColor(0, materialBlue);
						this.positiveColor = this.accentColor;
						this.negativeColor = this.accentColor;
						this.neutralColor = this.accentColor;
					}
					catch (System.Exception e)
					{
						this.accentColor = materialBlue;
						this.positiveColor = materialBlue;
						this.negativeColor = materialBlue;
						this.neutralColor = materialBlue;
					}
					finally
					{
						a.Recycle();
					}
				}
				CheckSingleton();
			}

			internal virtual void CheckSingleton()
			{
				if (ThemeSingleton.Get(false) == null)
				{
					return;
				}
				ThemeSingleton s = ThemeSingleton.Get();
				SetTheme(s.DarkTheme ? DialogTheme.DARK : DialogTheme.LIGHT);
				if (s.TitleColor != 0)
				{
					this.titleColor = s.TitleColor;
				}
				if (s.ContentColor != 0)
				{
					this.contentColor = s.ContentColor;
				}
				if (s.PositiveColor != 0)
				{
					this.positiveColor = s.PositiveColor;
				}
				if (s.NeutralColor != 0)
				{
					this.neutralColor = s.NeutralColor;
				}
				if (s.NegativeColor != 0)
				{
					this.negativeColor = s.NegativeColor;
				}
				if (s.ItemColor != 0)
				{
					this.itemColor = s.ItemColor;
				}
				if (s.Icon != null)
				{
					this.icon = s.Icon;
				}
				if (s.BackgroundColor != 0)
				{
					this.backgroundColor = s.BackgroundColor;
				}
				if (s.DividerColor != 0)
				{
					this.dividerColor = s.DividerColor;
				}
				if (s.BtnSelectorStacked != 0)
				{
					this.btnSelectorStacked = s.BtnSelectorStacked;
				}
				if (s.ListSelector != 0)
				{
					this.listSelector = s.ListSelector;
				}
				if (s.BtnSelectorPositive != 0)
				{
					this.btnSelectorPositive = s.BtnSelectorPositive;
				}
				if (s.BtnSelectorNeutral != 0)
				{
					this.btnSelectorNeutral = s.BtnSelectorNeutral;
				}
				if (s.BtnSelectorNegative != 0)
				{
					this.btnSelectorNegative = s.BtnSelectorNegative;
				}
			}

			public virtual Builder SetTitle(int titleRes)
			{
				SetTitle(this.context.GetString(titleRes));
				return this;
			}

            public virtual Builder SetTitle(string title)
			{
				this.title = title;
				return this;
			}

			public virtual Builder SetTitleGravity(GravityEnum gravity)
			{
				this.titleGravity = gravity;
				return this;
			}

			public virtual Builder SetTitleColor(Color color)
			{
				this.titleColor = color;
				this.titleColorSet = true;
				return this;
			}

			public virtual Builder SetTitleColorRes(int colorRes)
			{
				SetTitleColor(this.context.Resources.GetColor(colorRes));
				return this;
			}

			public virtual Builder SetTitleColorAttr(int colorAttr)
			{
				SetTitleColor(DialogUtils.ResolveColor(this.context, colorAttr));
				return this;
			}

			/// <summary>
			/// Disable usage of the default fonts. This is automatically set by
			/// <seealso cref="#typeface(String, String)"/> and <seealso cref="#typeface(Typeface, Typeface)"/>.
			/// </summary>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder DisableDefaultFonts()
			{
				this.useCustomFonts = true;
				return this;
			}

			/// <summary>
			/// Sets the fonts used in the dialog. It's recommended that you use <seealso cref="#typeface(String, String)"/> instead,
			/// to avoid duplicate Typeface allocations and high memory usage.
			/// </summary>
			/// <param name="medium">  The font used on titles and action buttons. Null uses device default. </param>
			/// <param name="regular"> The font used everywhere else, like on the content and list items. Null uses device default. </param>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder SetTypeface(Typeface medium, Typeface regular)
			{
				this.mediumFont = medium;
				this.regularFont = regular;
				this.useCustomFonts = true;
				return this;
			}

			/// <summary>
			/// Sets the fonts used in the dialog, by file names. This also uses TypefaceHelper in order
			/// to avoid any un-needed allocations (it Recycles typefaces for you).
			/// </summary>
			/// <param name="medium">  The name of font in assets/fonts, minus the extension (null uses device default). E.g. [your-project]/app/main/assets/fonts/[medium].ttf </param>
			/// <param name="regular"> The name of font in assets/fonts, minus the extension (null uses device default). E.g. [your-project]/app/main/assets/fonts/[regular].ttf </param>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder SetTypeface(string medium, string regular)
			{
				if (medium != null)
				{
					this.mediumFont = TypefaceHelper.Get(this.context, medium);
				}
				if (regular != null)
				{
					this.regularFont = TypefaceHelper.Get(this.context, regular);
				}
				this.useCustomFonts = true;
				return this;
			}

			public virtual Builder SetIcon(Drawable icon)
			{
				this.icon = icon;
				return this;
			}

			public virtual Builder SetIconRes(int icon)
			{
				this.icon = context.Resources.GetDrawable(icon);
				return this;
			}

			public virtual Builder SetIconAttr(int iconAttr)
			{
				this.icon = DialogUtils.ResolveDrawable(context, iconAttr);
				return this;
			}

			public virtual Builder SetContentColor(Color color)
			{
				this.contentColor = color;
				this.contentColorSet = true;
				return this;
			}

			public virtual Builder SetContentColorRes(int colorRes)
			{
				SetContentColor(this.context.Resources.GetColor(colorRes));
				return this;
			}

			public virtual Builder ContentColorAttr(int colorAttr)
			{
				SetContentColor(DialogUtils.ResolveColor(this.context, colorAttr));
				return this;
			}

			public virtual Builder SetContent(int contentRes)
			{
				SetContent(this.context.GetString(contentRes));
				return this;
			}

            public virtual Builder SetContent(string content)
			{
				this.content = content;
				return this;
			}

			public virtual Builder SetContent(int contentRes, params Java.Lang.Object[] formatArgs)
			{
				SetContent(this.context.GetString(contentRes, formatArgs));
				return this;
			}

			public virtual Builder SetContentGravity(GravityEnum gravity)
			{
				this.contentGravity = gravity;
				return this;
			}

			public virtual Builder SetContentLineSpacing(float multiplier)
			{
				this.contentLineSpacingMultiplier = multiplier;
				return this;
			}

			public virtual Builder SetItems(int itemsRes)
			{
				SetItems(this.context.Resources.GetTextArray(itemsRes));
				return this;
			}

            public virtual Builder SetItems(string[] items)
			{
				this.items = items;
				return this;
			}

			public virtual Builder SetItemsCallback(IListCallback callback)
			{
				this.listCallback = callback;
				this.listCallbackSingleChoice = null;
				this.listCallbackMultiChoice = null;
				return this;
			}

			/// <summary>
			/// Pass anything below 0 (such as -1) for the selected index to leave all options unselected initially.
			/// Otherwise pass the index of an item that will be selected initially.
			/// </summary>
			/// <param name="selectedIndex"> The checkbox index that will be selected initially. </param>
			/// <param name="callback">      The callback that will be called when the presses the positive button. </param>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder SetItemsCallbackSingleChoice(int selectedIndex, IListCallbackSingleChoice callback)
			{
				this.selectedIndex = selectedIndex;
				this.listCallback = null;
				this.listCallbackSingleChoice = callback;
				this.listCallbackMultiChoice = null;
				return this;
			}

			/// <summary>
			/// By default, the single choice callback is only called when the user clicks the positive button
			/// or if there are no buttons. Call this to force it to always call on item clicks even if the
			/// positive button exists.
			/// </summary>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder AlwaysCallSingleChoiceCallback()
			{
				this.alwaysCallSingleChoiceCallback = true;
				return this;
			}

			/// <summary>
			/// Pass null for the selected indices to leave all options unselected initially. Otherwise pass
			/// an array of indices that will be selected initially.
			/// </summary>
			/// <param name="selectedIndices"> The radio button indices that will be selected initially. </param>
			/// <param name="callback">        The callback that will be called when the presses the positive button. </param>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder SetItemsCallbackMultiChoice(int[] selectedIndices, IListCallbackMultiChoice callback)
			{
				this.selectedIndices = selectedIndices;
				this.listCallback = null;
				this.listCallbackSingleChoice = null;
				this.listCallbackMultiChoice = callback;
				return this;
			}

			/// <summary>
			/// By default, the multi choice callback is only called when the user clicks the positive button
			/// or if there are no buttons. Call this to force it to always call on item clicks even if the
			/// positive button exists.
			/// </summary>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder AlwaysCallMultiChoiceCallback()
			{
				this.alwaysCallMultiChoiceCallback = true;
				return this;
			}

			public virtual Builder SetPositiveText(int postiveRes)
			{
				SetPositiveText(this.context.GetString(postiveRes));
				return this;
			}

            public virtual Builder SetPositiveText(string message)
			{
				this.positiveText = message;
				return this;
			}

            public virtual Builder SetPositiveText(string message, EventHandler handler)
            {
                this.positiveText = message;
                this.PositiveHandler = handler;
                return this;
            }

			public virtual Builder SetNeutralText(int neutralRes)
			{
				return SetNeutralText(this.context.GetString(neutralRes));
			}

            public virtual Builder SetNeutralText(string message)
			{
				this.neutralText = message;
				return this;
			}

            public virtual Builder SetNeutralText(string message, EventHandler handler)
            {
                this.neutralText = message;
                this.NeutralHandler = handler;
                return this;
            }

			public virtual Builder SetNegativeText(int negativeRes)
			{
				return SetNegativeText(this.context.GetString(negativeRes));
			}

            public virtual Builder SetNegativeText(string message)
			{
				this.negativeText = message;
				return this;
			}

            public virtual Builder SetNegativeText(string message, EventHandler handler)
            {
                this.negativeText = message;
                this.NegativeHandler = handler;
                return this;
            }

			public virtual Builder SetListSelector(int selectorRes)
			{
				this.listSelector = selectorRes;
				return this;
			}

			public virtual Builder SetBtnSelectorStacked(int selectorRes)
			{
				this.btnSelectorStacked = selectorRes;
				return this;
			}

			public virtual Builder SetBtnSelector(int selectorRes)
			{
				this.btnSelectorPositive = selectorRes;
				this.btnSelectorNeutral = selectorRes;
				this.btnSelectorNegative = selectorRes;
				return this;
			}

			public virtual Builder SetBtnSelector(int selectorRes, DialogAction which)
			{
				switch (which)
				{
					default:
						this.btnSelectorPositive = selectorRes;
						break;
					case MaterialDialogs.DialogAction.NEUTRAL:
						this.btnSelectorNeutral = selectorRes;
						break;
					case MaterialDialogs.DialogAction.NEGATIVE:
						this.btnSelectorNegative = selectorRes;
						break;
				}
				return this;
			}

			/// <summary>
			/// Sets the gravity used for the text in stacked action buttons. By default, it's #<seealso cref="GravityEnum#END"/>.
			/// </summary>
			/// <param name="gravity"> The gravity to use. </param>
			/// <returns> The Builder instance so calls can be chained. </returns>
			public virtual Builder SetBtnStackedGravity(GravityEnum gravity)
			{
				this.btnStackedGravity = gravity;
				return this;
			}

			/// <summary>
			/// Use <seealso cref="#customView(int, boolean)"/> instead.
			/// </summary>
			[Obsolete]
			public virtual Builder SetCustomView(int layoutRes)
			{
				return SetCustomView(layoutRes, true);
			}

			public virtual Builder SetCustomView(int layoutRes, bool wrapInScrollView)
			{
				LayoutInflater li = LayoutInflater.From(this.context);
				return SetCustomView(li.Inflate(layoutRes, null), wrapInScrollView);
			}

			/// <summary>
			/// Use <seealso cref="#customView(android.view.View, boolean)"/> instead.
			/// </summary>
			[Obsolete]
			public virtual Builder SetCustomView(View view)
			{
				return SetCustomView(view, true);
			}

			public virtual Builder SetCustomView(View view, bool wrapInScrollView)
			{
				this.customView = view;
				this.wrapCustomViewInScroll = wrapInScrollView;
				return this;
			}

			/// <summary>
			/// Makes this dialog a progress dialog.
			/// </summary>
			/// <param name="indeterminate"> If true, an infinite circular spinner is shown. If false, a horizontal progress bar is shown that is incremented or set via the built MaterialDialog instance. </param>
			/// <param name="max">           When indeterminate is false, the max value the horizontal progress bar can get to. </param>
			/// <returns> An instance of the Builder so calls can be chained. </returns>
			public virtual Builder SetProgress(bool indeterminate, int max)
			{
				if (indeterminate)
				{
					this.mIndeterminateProgress = true;
					this.mProgress = -2;
				}
				else
				{
					this.mIndeterminateProgress = false;
					this.mProgress = -1;
					this.mProgressMax = max;
				}
				return this;
			}

			/// <summary>
			/// Makes this dialog a progress dialog.
			/// </summary>
			/// <param name="indeterminate"> If true, an infinite circular spinner is shown. If false, a horizontal progress bar is shown that is incremented or set via the built MaterialDialog instance. </param>
			/// <param name="max">           When indeterminate is false, the max value the horizontal progress bar can get to. </param>
			/// <param name="showMinMax">    For determinate dialogs, the min and max will be displayed to the left (start) of the progress bar, e.g. 50/100. </param>
			/// <returns> An instance of the Builder so calls can be chained. </returns>
			public virtual Builder SetProgress(bool indeterminate, int max, bool showMinMax)
			{
				this.mShowMinMax = showMinMax;
				return SetProgress(indeterminate, max);
			}

            public virtual Builder SetPositiveColor(Color color)
			{
				this.positiveColor = color;
				return this;
			}

			public virtual Builder SetPositiveColorRes(int colorRes)
			{
				SetPositiveColor(this.context.Resources.GetColor(colorRes));
				return this;
			}

			public virtual Builder SetPositiveColorAttr(int colorAttr)
			{
				SetPositiveColor(DialogUtils.ResolveColor(this.context, colorAttr));
				return this;
			}

			public virtual Builder SetNegativeColor(Color color)
			{
				this.negativeColor = color;
				return this;
			}

			public virtual Builder SetNegativeColorRes(int colorRes)
			{
				SetNegativeColor(this.context.Resources.GetColor(colorRes));
				return this;
			}

			public virtual Builder SetNegativeColorAttr(int colorAttr)
			{
				SetNegativeColor(DialogUtils.ResolveColor(this.context, colorAttr));
				return this;
			}

            public virtual Builder SetNeutralColor(Color color)
			{
				this.neutralColor = color;
				return this;
			}

			public virtual Builder SetNeutralColorRes(int colorRes)
			{
				return SetNeutralColor(this.context.Resources.GetColor(colorRes));
			}

			public virtual Builder SetNeutralColorAttr(int colorAttr)
			{
				return SetNeutralColor(DialogUtils.ResolveColor(this.context, colorAttr));
			}

			public virtual Builder SetDividerColor(Color color)
			{
				this.dividerColor = color;
				return this;
			}

			public virtual Builder SetDividerColorRes(int colorRes)
			{
				return SetDividerColor(this.context.Resources.GetColor(colorRes));
			}

			public virtual Builder SetDividerColorAttr(int colorAttr)
			{
				return SetDividerColor(DialogUtils.ResolveColor(this.context, colorAttr));
			}

			public virtual Builder SetBackgroundColor(Color color)
			{
				this.backgroundColor = color;
				return this;
			}

			public virtual Builder SetBackgroundColorRes(int colorRes)
			{
				return SetBackgroundColor(this.context.Resources.GetColor(colorRes));
			}

			public virtual Builder SetBackgroundColorAttr(int colorAttr)
			{
				return SetBackgroundColor(DialogUtils.ResolveColor(this.context, colorAttr));
			}

			public virtual Builder SetItemColor(int color)
			{
				this.itemColor = color;
				this.itemColorSet = true;
				return this;
			}

			public virtual Builder SetItemColorRes(int colorRes)
			{
				return SetItemColor(this.context.Resources.GetColor(colorRes));
			}

			public virtual Builder SetItemColorAttr(int colorAttr)
			{
				return SetItemColor(DialogUtils.ResolveColor(this.context, colorAttr));
			}

			public virtual Builder SetCallback(ButtonCallback callback)
			{
				this.callback = callback;
				return this;
			}

			public virtual Builder SetTheme(DialogTheme theme)
			{
				this.theme = theme;
				return this;
			}

			public virtual Builder SetCancelable(bool cancelable)
			{
				this.cancelable = cancelable;
				return this;
			}

			/// <summary>
			/// This defaults to true. If set to false, the dialog will not automatically be dismissed
			/// when an action button is pressed, and not automatically dismissed when the user selects
			/// a list item.
			/// </summary>
			/// <param name="dismiss"> Whether or not to dismiss the dialog automatically. </param>
			/// <returns> The Builder instance so you can chain calls to it. </returns>
			public virtual Builder SetAutoDismiss(bool dismiss)
			{
				this.autoDismiss = dismiss;
				return this;
			}

			/// <summary>
			/// Sets a custom <seealso cref="android.widget.ListAdapter"/> for the dialog's list
			/// </summary>
			/// <param name="adapter"> The adapter to set to the list. </param>
			/// <returns> This Builder object to allow for chaining of calls to set methods </returns>
			/// @deprecated Use <seealso cref="#adapter(ListAdapter, ListCallback)"/> instead. 
			[Obsolete("Use adapter(ListAdapter, ListCallback) instead.")]
			public virtual Builder SetAdapter(IListAdapter adapter)
			{
				this.adapter = adapter;
				return this;
			}

			/// <summary>
			/// Sets a custom <seealso cref="android.widget.ListAdapter"/> for the dialog's list
			/// </summary>
			/// <param name="adapter">  The adapter to set to the list. </param>
			/// <param name="callback"> The callback invoked when an item in the list is selected. </param>
			/// <returns> This Builder object to allow for chaining of calls to set methods </returns>
			public virtual Builder SetAdapter(IListAdapter adapter, IListCallback callback)
			{
				this.adapter = adapter;
				this.listCallbackCustom = callback;
				return this;
			}

			public virtual Builder LimitIconToDefaultSize()
			{
				this.limitIconToDefaultSize = true;
				return this;
			}

			public virtual Builder SetMaxIconSize(int maxIconSize)
			{
				this.maxIconSize = maxIconSize;
				return this;
			}

			public virtual Builder SetMaxIconSizeRes(int maxIconSizeRes)
			{
				return SetMaxIconSize((int) this.context.Resources.GetDimension(maxIconSizeRes));
			}

			public virtual Builder SetShowListener(IDialogInterfaceOnShowListener listener)
			{
				this.showListener = listener;
				return this;
			}

			public virtual Builder SetDismissListener(IDialogInterfaceOnDismissListener listener)
			{
				this.dismissListener = listener;
				return this;
			}

			public virtual Builder SetCancelListener(IDialogInterfaceOnCancelListener listener)
			{
				this.cancelListener = listener;
				return this;
			}

			public virtual Builder SetKeyListener(IDialogInterfaceOnKeyListener listener)
			{
				this.keyListener = listener;
				return this;
			}

			public virtual Builder SetForceStacking(bool stacked)
			{
				this.forceStacking = stacked;
				return this;
			}

			public virtual MaterialDialog Build()
			{
				if ((content == null || content.ToString().Trim().Length == 0) && title != null && (items == null || items.Length == 0) && customView == null && adapter == null)
				{
					this.content = this.title;
					this.title = null;
				}
				return new MaterialDialog(this);
			}

			public virtual MaterialDialog Show()
			{
				MaterialDialog dialog = Build();
				dialog.Show();
				return dialog;
			}
		}

		public override void Show()
		{
			if (Looper.MyLooper() != Looper.MainLooper)
			{
				throw new IllegalStateException("Dialogs can only be shown From the UI thread.");
			}
			try
			{
				base.Show();
			}
			catch (WindowManagerBadTokenException e)
			{
				throw new DialogException("Bad window token, you cannot show a dialog before an Activity is created or after it's hidden.");
			}
		}

		private ColorStateList GetActionTextStateList(int newPrimaryColor)
		{
			int fallBackButtonColor = DialogUtils.ResolveColor(Context, Android.Resource.Attribute.TextColorPrimary);
			if (newPrimaryColor == 0)
			{
				newPrimaryColor = fallBackButtonColor;
			}
			int[][] states = new int[][]{ new int[]{ -Android.Resource.Attribute.Enabled }, new int[]{} };
			int[] colors = new int[]{ DialogUtils.AdjustAlpha(newPrimaryColor, 0.4f), newPrimaryColor };
			return new ColorStateList(states, colors);
		}

		/// <summary>
		/// Retrieves the view of an action button, allowing you to modify properties such as whether or not it's enabled.
		/// Use <seealso cref="#setActionButton(DialogAction, int)"/> to change text, since the view returned here is not
		/// the view that displays text.
		/// </summary>
		/// <param name="which"> The action button of which to get the view for. </param>
		/// <returns> The view From the dialog's layout representing this action button. </returns>
		public View GetActionButton(DialogAction which)
		{
			if (isStacked)
			{
				switch (which)
				{
					default:
						return view.FindViewById(Resource.Id.buttonStackedPositive);
					case MaterialDialogs.DialogAction.NEUTRAL:
						return view.FindViewById(Resource.Id.buttonStackedNeutral);
					case MaterialDialogs.DialogAction.NEGATIVE:
						return view.FindViewById(Resource.Id.buttonStackedNegative);
				}
			}
			else
			{
				switch (which)
				{
					default:
						return view.FindViewById(Resource.Id.buttonDefaultPositive);
					case MaterialDialogs.DialogAction.NEUTRAL:
						return view.FindViewById(Resource.Id.buttonDefaultNeutral);
					case MaterialDialogs.DialogAction.NEGATIVE:
						return view.FindViewById(Resource.Id.buttonDefaultNegative);
				}
			}
		}

		/// <summary>
		/// This will not return buttons that are actually in the layout itself, since the layout doesn't
		/// contain buttons. This is only implemented to avoid crashing issues on Huawei devices. Huawei's
		/// stock OS requires this method in order to detect visible buttons.
		/// </summary>
		/// @deprecated Use getActionButton(MaterialDialogs.DialogAction)} instead. 
		[Obsolete("Use getActionButton(MaterialDialogs.DialogAction)} instead.")]
		public override Button GetButton(int whichButton)
		{
			Console.WriteLine("MaterialDialog", "Warning: getButton() is a deprecated method that does not return valid references to action buttons.");
			if (whichButton == (int)DialogButtonType.Positive)
			{
				return mBuilder.positiveText != null ? new Button(Context) : null;
			}
            else if (whichButton == (int)DialogButtonType.Neutral)
			{
				return mBuilder.neutralText != null ? new Button(Context) : null;
			}
			else
			{
				return mBuilder.negativeText != null ? new Button(Context) : null;
			}
		}

		/// <summary>
		/// Retrieves the frame view containing the title and icon. You can manually change visibility and retrieve children.
		/// </summary>
		public View TitleFrame
		{
			get
			{
				return titleFrame;
			}
		}

		/// <summary>
		/// Retrieves the custom view that was inflated or set to the MaterialDialog during building.
		/// </summary>
		/// <returns> The custom view that was passed into the Builder. </returns>
		public View CustomView
		{
			get
			{
				return mBuilder.customView;
			}
		}

        /// <summary>
		/// Updates an action button's title, causing invalidation to check if the action buttons should be stacked.
		/// </summary>
		/// <param name="which"> The action button to update. </param>
		/// <param name="title"> The new title of the action button. </param>
		public void SetActionButton(DialogAction which, string title)
		{
			switch (which)
			{
				default:
					mBuilder.positiveText = title;
					break;
				case MaterialDialogs.DialogAction.NEUTRAL:
					mBuilder.neutralText = title;
					break;
				case MaterialDialogs.DialogAction.NEGATIVE:
					mBuilder.negativeText = title;
					break;
			}
			InvalidateActions();
		}

		/// <summary>
		/// Updates an action button's title, causing invalidation to check if the action buttons should be stacked.
		/// </summary>
		/// <param name="which">    The action button to update. </param>
		/// <param name="titleRes"> The string resource of the new title of the action button. </param>
		public void SetActionButton(DialogAction which, int titleRes)
		{
			SetActionButton(which, Context.GetString(titleRes));
		}

		/// <summary>
		/// Gets whether or not the positive, neutral, or negative action button is visible.
		/// </summary>
		/// <returns> Whether or not 1 or more action buttons is visible. </returns>
		public bool HasActionButtons()
		{
			return NumberOfActionButtons() > 0;
		}

		/// <summary>
		/// Gets the number of visible action buttons.
		/// </summary>
		/// <returns> 0 through 3, depending on how many should be or are visible. </returns>
		public int NumberOfActionButtons()
		{
			int number = 0;
			if (mBuilder.positiveText != null)
			{
				number++;
			}
			if (mBuilder.neutralText != null)
			{
				number++;
			}
			if (mBuilder.negativeText != null)
			{
				number++;
			}
			return number;
		}

        /// <summary>
		/// Updates the dialog's title.
		/// </summary>
		public void SetTitle (string title)
		{
            this.title.Text = title;
		}

        public override void SetIcon(int resId)
		{
            icon.SetImageResource(resId);
            icon.Visibility = resId != 0 ? ViewStates.Visible : ViewStates.Gone;
		}

		public override void SetIcon(Drawable d)
		{
			icon.SetImageDrawable(d);
			icon.Visibility = d != null ? ViewStates.Visible : ViewStates.Gone;
		}

		public override void SetIconAttribute(int attrId)
		{
			Drawable d = DialogUtils.ResolveDrawable(mBuilder.context, attrId);
			icon.SetImageDrawable(d);
			icon.Visibility = d != null ? ViewStates.Visible : ViewStates.Gone;
		}

        public void SetContent(string content)
		{
			this.content.Text = content;
			InvalidateCustomViewAssociations(); // invalidates padding in value area scroll (if needed)
		}

		public void SetItems(string[] items)
		{
			if (mBuilder.adapter == null)
			{
				throw new IllegalStateException("This MaterialDialog instance does not yet have an adapter set to it. You cannot use setItems().");
			}
			if (mBuilder.adapter is MaterialDialogAdapter)
			{
				mBuilder.adapter = new MaterialDialogAdapter(this, GetLayoutForType(listType), Resource.Id.title, items);
			}
			else
			{
				throw new IllegalStateException("When using a custom adapter, setItems() cannot be used. Set items through the adapter instead.");
			}
            mBuilder.items = items;
			listView.Adapter = mBuilder.adapter;
			InvalidateCustomViewAssociations();
		}

		public int CurrentProgress
		{
			get
			{
				if (mProgress == null)
				{
					return -1;
				}
				return mProgress.Progress;
			}
		}

		public void IncrementProgress(int by)
		{
			if (mBuilder.mProgress <= -2)
			{
				throw new IllegalStateException("Cannot use incrementProgress() on this dialog.");
			}
			SetProgress(CurrentProgress + by);
		}

		public void SetProgress(int progress)
		{
			if (Looper.MyLooper() != Looper.MainLooper)
			{
				throw new IllegalStateException("You can only set the dialog's progress From the UI thread.");
			}
			else if (mBuilder.mProgress <= -2)
			{
				throw new IllegalStateException("Cannot use setProgress() on this dialog.");
			}
            mProgress.Progress = progress;
			int percentage = (int)(((float) CurrentProgress / (float) MaxProgress) * 100f);
			mProgressLabel.Text = percentage + "%";
			if (mProgressMinMax != null)
			{
				mProgressMinMax.Text = CurrentProgress + "/" + MaxProgress;
			}
		}

		public int MaxProgress
		{
			set
			{
				if (Looper.MyLooper() != Looper.MainLooper)
				{
					throw new IllegalStateException("You can only set the dialog's progress From the UI thread.");
				}
				else if (mBuilder.mProgress <= -2)
				{
					throw new IllegalStateException("Cannot use setMaxProgress() on this dialog.");
				}
				mProgress.Max = value;
			}
			get
			{
				if (mProgress == null)
				{
					return -1;
				}
				return mProgress.Max;
			}
		}

		public bool IsIndeterminateProgress
		{
			get
			{
				return mBuilder.mIndeterminateProgress;
			}
		}


		public bool IsCancelled
		{
			get
			{
				return !IsShowing;
			}
		}

		/// <summary>
		/// Use this to customize any list-specific logic for this dialog (OnItemClickListener, OnLongItemClickListener, etc.)
		/// </summary>
		/// <returns> The ListView instance used by this dialog, or null if not using a list. </returns>
		public virtual ListView GetListView()
		{
			return listView;
		}

		/// <summary>
		/// Convenience method for getting the currently selected index of a single choice list.
		/// </summary>
		/// <returns> Currently selected index of a single choice list, or -1 if not showing a single choice list </returns>
		public virtual int SelectedIndex
		{
			get
			{
				if (mBuilder.listCallbackSingleChoice != null)
				{
					return mBuilder.selectedIndex;
				}
				else
				{
					return -1;
				}
			}
			set
			{
				mBuilder.selectedIndex = value;
				if (mBuilder.adapter != null && mBuilder.adapter is MaterialDialogAdapter)
				{
					((MaterialDialogAdapter) mBuilder.adapter).NotifyDataSetChanged();
				}
				else
				{
					throw new IllegalStateException("You can only use setSelectedIndex() with the default adapter implementation.");
				}
			}
		}

		/// <summary>
		/// Convenience method for getting the currently selected indices of a multi choice list
		/// </summary>
		/// <returns> Currently selected index of a multi choice list, or null if not showing a multi choice list </returns>
		public virtual List<int> SelectedIndices
		{
			get
			{
				if (mBuilder.listCallbackMultiChoice != null)
				{
					return selectedIndicesList;
				}
				else
				{
					return null;
				}
			}
			set
			{
				mBuilder.selectedIndices = value.ToArray();
				selectedIndicesList = value;
				if (mBuilder.adapter != null && mBuilder.adapter is MaterialDialogAdapter)
				{
					((MaterialDialogAdapter) mBuilder.adapter).NotifyDataSetChanged();
				}
				else
				{
					throw new IllegalStateException("You can only use setSelectedIndices() with the default adapter implementation.");
				}
			}
		}


		/// <summary>
		/// Convenience method for setting the currently selected indices of a multi choice list.
		/// This only works if you are not using a custom adapter; if you're using a custom adapter,
		/// an IllegalStateException is thrown. Note that this does not call the respective multi choice callback.
		/// </summary>
		/// <param name="indices"> The indices of the list items to check. </param>
		public int GetLayoutForType(ListType type)
		{
			switch (type)
			{
				case ListType.REGULAR:
					return Resource.Layout.md_listitem;
				case ListType.SINGLE:
					return Resource.Layout.md_listitem_singlechoice;
				case ListType.MULTI:
					return Resource.Layout.md_listitem_multichoice;
				default:
					throw new IllegalArgumentException("Not a valid list type");
			}
		}

		/// <summary>
		/// A callback used for regular list dialogs.
		/// </summary>
		public interface IListCallback
		{
			void OnSelection(MaterialDialog dialog, View itemView, int which, string text);
		}

		/// <summary>
		/// A callback used for multi choice (check box) list dialogs.
		/// </summary>
		public interface IListCallbackSingleChoice
		{
			/// <summary>
			/// Return true to allow the radio button to be checked, if the alwaysCallSingleChoice() option is used.
			/// </summary>
			/// <param name="dialog"> The dialog of which a list item was selected. </param>
			/// <param name="which">  The index of the item that was selected. </param>
			/// <param name="text">   The text of the  item that was selected. </param>
			/// <returns> True to allow the radio button to be selected. </returns>
			bool OnSelection(MaterialDialog dialog, View itemView, int which, string text);
		}

		/// <summary>
		/// A callback used for multi choice (check box) list dialogs.
		/// </summary>
		public interface IListCallbackMultiChoice
		{
			/// <summary>
			/// Return true to allow the check box to be checked, if the alwaysCallSingleChoice() option is used.
			/// </summary>
			/// <param name="dialog"> The dialog of which a list item was selected. </param>
			/// <param name="which">  The indices of the items that were selected. </param>
			/// <param name="text">   The text of the items that were selected. </param>
			/// <returns> True to allow the checkbox to be selected. </returns>
			bool OnSelection(MaterialDialog dialog, int[] which, string[] text);
		}

		/// <summary>
		/// Override these as needed, so no needing to sub empty methods From an interface
		/// </summary>
		public abstract class ButtonCallback : Java.Lang.Object
		{
			public virtual void OnPositive(MaterialDialog dialog)
			{

			}

			public virtual void OnNegative(MaterialDialog dialog)
			{

			}

			public virtual void OnNeutral(MaterialDialog dialog)
			{

			}

			public ButtonCallback()
                : base()
			{

			}

            protected override Java.Lang.Object Clone()
            {
                return base.Clone();
            }


            protected override void JavaFinalize()
            {
                base.JavaFinalize();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

			public override string ToString()
			{
				return base.ToString();
			}
		}
	}
}