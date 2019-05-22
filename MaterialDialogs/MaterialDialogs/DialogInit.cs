using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaterialDialogs
{
	/// <summary>
	/// Used by MaterialDialog while initializing the dialog. Offloads some of the code to make the main class
	/// cleaner and easier to read/maintain.
	/// 
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	internal class DialogInit
	{
		public static ContextThemeWrapper GetTheme(MaterialDialog.Builder builder)
		{
			TypedArray a = builder.context.Theme.ObtainStyledAttributes(new int[]{ Resource.Attribute.md_dark_theme });
			bool darkTheme = builder.theme == DialogTheme.DARK;
			if (!darkTheme)
			{
				try
				{
					darkTheme = a.GetBoolean(0, false);
					builder.theme = darkTheme ? DialogTheme.DARK : DialogTheme.LIGHT;
				}
				finally
				{
					a.Recycle();
				}
			}
			return new ContextThemeWrapper(builder.context, darkTheme ? Resource.Style.MD_Dark : Resource.Style.MD_Light);
		}

		public static int GetInflateLayout(MaterialDialog.Builder builder)
		{
			if (builder.customView != null)
			{
				return Resource.Layout.md_dialog_custom;
			}
			else if (builder.items != null && builder.items.Length > 0 || builder.adapter != null)
			{
				return Resource.Layout.md_dialog_list;
			}
			else if (builder.mProgress > -2)
			{
				return Resource.Layout.md_dialog_progress;
			}
			else if (builder.mIndeterminateProgress)
			{
				return Resource.Layout.md_dialog_progress_indeterminate;
			}
			else
			{
				return Resource.Layout.md_dialog_basic;
			}
		}
		
        public static void Init(MaterialDialog dialog)
		{
			MaterialDialog.Builder builder = dialog.mBuilder;

			// Check if default library fonts should be used
			if (!builder.useCustomFonts)
			{
				if (builder.mediumFont == null)
				{
					builder.mediumFont = TypefaceHelper.Get(dialog.Context, "Roboto-Medium");
				}
				if (builder.regularFont == null)
				{
					builder.regularFont = TypefaceHelper.Get(dialog.Context, "Roboto-Regular");
				}
			}

			// Set cancelable flag and dialog background color
			dialog.SetCancelable(builder.cancelable);
			if (builder.backgroundColor == 0)
			{
				builder.backgroundColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_background_color);
			}
			if (builder.backgroundColor != 0)
			{
				dialog.view.SetBackgroundColor(builder.backgroundColor);
			}

			// Retrieve action button colors From theme attributes or the Builder
			builder.positiveColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_positive_color, builder.positiveColor);
			builder.neutralColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_neutral_color, builder.neutralColor);
			builder.negativeColor = DialogUtils.ResolveColor(builder.context, Resource.Attribute.md_negative_color, builder.negativeColor);

			// Retrieve references to views
			dialog.title = (TextView) dialog.view.FindViewById(Resource.Id.title);
			dialog.icon = (ImageView) dialog.view.FindViewById(Resource.Id.icon);
			dialog.titleFrame = dialog.view.FindViewById(Resource.Id.titleFrame);
			dialog.content = (TextView) dialog.view.FindViewById(Resource.Id.content);
			dialog.listView = (ListView) dialog.view.FindViewById(Resource.Id.contentListView);

			// Setup icon
			if (builder.icon != null)
			{
				dialog.icon.Visibility = ViewStates.Visible;
				dialog.icon.SetImageDrawable(builder.icon);
			}
			else
			{
				Drawable d = DialogUtils.ResolveDrawable(builder.context, Resource.Attribute.md_icon);
				if (d != null)
				{
					dialog.icon.Visibility = ViewStates.Visible;
                    dialog.icon.SetImageDrawable(d);
				}
				else
				{
					dialog.icon.Visibility = ViewStates.Gone;
				}
			}
			// Setup icon size limiting
			int maxIconSize = builder.maxIconSize;
			if (maxIconSize == -1)
			{
				maxIconSize = DialogUtils.ResolveDimension(builder.context, Resource.Attribute.md_icon_max_size);
			}
			if (builder.limitIconToDefaultSize || DialogUtils.ResolveBoolean(builder.context, Resource.Attribute.md_icon_limit_icon_to_default_size))
			{
				maxIconSize = builder.context.Resources.GetDimensionPixelSize(Resource.Dimension.md_icon_max_size);
			}
			if (maxIconSize > -1)
			{
				dialog.icon.SetAdjustViewBounds(true);
                dialog.icon.SetMaxHeight(maxIconSize);
                dialog.icon.SetMaxWidth(maxIconSize);
				dialog.icon.RequestLayout();
			}

			// Setup title and title frame
			if (builder.title == null)
			{
				dialog.titleFrame.Visibility = ViewStates.Gone;
			}
			else
			{
				dialog.title.Text = builder.title;
				dialog.SetTypeface(dialog.title, builder.mediumFont);
				if (builder.titleColorSet)
				{
					dialog.title.SetTextColor(builder.titleColor);
				}
				else
				{
					int fallback = DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorPrimary);
                    dialog.title.SetTextColor(DialogUtils.ResolveColor(dialog.Context, Resource.Attribute.md_title_color, fallback));
				}
				dialog.title.Gravity = MaterialDialog.GravityIntToGravity(builder.titleGravity);
			}

			// Setup content
			if (dialog.content != null)
			{
				dialog.content.Text = builder.content;
				dialog.content.MovementMethod = new LinkMovementMethod();
				dialog.SetTypeface(dialog.content, builder.regularFont);
				dialog.content.SetLineSpacing(0f, builder.contentLineSpacingMultiplier);
				if (builder.positiveColor == 0)
				{
					dialog.content.SetLinkTextColor(DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorPrimary));
				}
				else
				{
                    dialog.content.SetLinkTextColor(builder.positiveColor);
				}
				dialog.content.Gravity = MaterialDialog.GravityIntToGravity(builder.contentGravity);
				if (builder.contentColorSet)
				{
					dialog.content.SetTextColor(builder.contentColor);
				}
				else
				{
                    int fallback = DialogUtils.ResolveColor(dialog.Context, Android.Resource.Attribute.TextColorSecondary);
					Color contentColor = DialogUtils.ResolveColor(dialog.Context, Resource.Attribute.md_content_color, fallback);
					dialog.content.SetTextColor(contentColor);
				}
			}

			// Load default list item text color
			if (builder.itemColorSet)
			{
				dialog.defaultItemColor = builder.itemColor;
			}
			else if (builder.theme == DialogTheme.LIGHT)
			{
				dialog.defaultItemColor = Color.Black;
			}
			else
			{
				dialog.defaultItemColor = Color.White;
			}

			// Setup list dialog stuff
			if (builder.listCallbackMultiChoice != null)
			{
				dialog.selectedIndicesList = new List<int>();
			}
			if (dialog.listView != null && (builder.items != null && builder.items.Length > 0 || builder.adapter != null))
			{
				dialog.listView.Selector = dialog.ListSelector;

				if (builder.title != null)
				{
					// Cancel out top padding if there's a title
					dialog.listView.SetPadding(dialog.listView.PaddingLeft, 0, dialog.listView.PaddingRight, dialog.listView.PaddingBottom);
				}
				if (dialog.HasActionButtons())
				{
					// No bottom padding if there's action buttons
					dialog.listView.SetPadding(dialog.listView.PaddingLeft, 0, dialog.listView.PaddingRight, 0);
				}

				// No custom adapter specified, setup the list with a MaterialDialogAdapter.
				// Which supports regular lists and single/multi choice dialogs.
				if (builder.adapter == null)
				{
					// Determine list type
					if (builder.listCallbackSingleChoice != null)
					{
						dialog.listType = ListType.SINGLE;
					}
					else if (builder.listCallbackMultiChoice != null)
					{
						dialog.listType = ListType.MULTI;
						if (builder.selectedIndices != null)
						{
							dialog.selectedIndicesList = builder.selectedIndices.ToList();
						}
					}
					else
					{
						dialog.listType = ListType.REGULAR;
					}
					builder.adapter = new MaterialDialogAdapter(dialog, GetLayoutForType(dialog.listType), Resource.Id.title, builder.items);
				}
			}

			// Setup progress dialog stuff if needed
			SetupProgressDialog(dialog);

			if (builder.customView != null)
			{
				dialog.InvalidateCustomViewAssociations();
				FrameLayout frame = (FrameLayout) dialog.view.FindViewById(Resource.Id.customViewFrame);
				dialog.customViewFrame = frame;
				View innerView = builder.customView;
				if (builder.wrapCustomViewInScroll)
				{
					/* Apply the frame padding to the content, this allows the ScrollView to draw it's
					   overscroll glow without clipping */
					Resources r = dialog.Context.Resources;
					int framePadding = r.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);
					ScrollView sv = new ScrollView(dialog.Context);
					int paddingTop;
					int paddingBottom;
					if (dialog.titleFrame.Visibility != ViewStates.Gone)
					{
						paddingTop = r.GetDimensionPixelSize(Resource.Dimension.md_content_vertical_padding);
					}
					else
					{
						paddingTop = r.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);
					}
					if (dialog.HasActionButtons())
					{
						paddingBottom = r.GetDimensionPixelSize(Resource.Dimension.md_content_vertical_padding);
					}
					else
					{
						paddingBottom = r.GetDimensionPixelSize(Resource.Dimension.md_dialog_frame_margin);
					}
					sv.SetClipToPadding(false);
					if (innerView is EditText)
					{
						// Setting padding to an EditText causes visual errors, set it to the parent instead
						sv.SetPadding(framePadding, paddingTop, framePadding, paddingBottom);
					}
					else
					{
						// Setting padding to scroll view pushes the scroll bars out, don't do it if not necessary (like above)
						sv.SetPadding(0, paddingTop, 0, paddingBottom);
						innerView.SetPadding(framePadding, 0, framePadding, 0);
					}
					sv.AddView(innerView, new ScrollView.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
					innerView = sv;
				}
				frame.AddView(innerView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent));
			}
			else
			{
				dialog.InvalidateCustomViewAssociations();
			}

			// Setup user listeners
			if (builder.showListener != null)
			{
				dialog.SetOnShowListener(builder.showListener);
			}
			if (builder.cancelListener != null)
			{
				dialog.SetOnCancelListener(builder.cancelListener);
			}
			if (builder.dismissListener != null)
			{
				dialog.SetOnDismissListener(builder.dismissListener);
			}
			if (builder.keyListener != null)
			{
				dialog.SetOnKeyListener(builder.keyListener);
			}

			// Other internal initialization
			dialog.UpdateFramePadding();
			dialog.InvalidateActions();
			dialog._setOnShowListenerInternal();
			dialog._setViewInternal(dialog.view);
            dialog.view.ViewTreeObserver.GlobalLayout += (sender, e) =>
            {
                if (dialog.view.MeasuredWidth > 0)
                {
                    dialog.InvalidateCustomViewAssociations();
                }
            };

			// Gingerbread compatibility stuff
			if (builder.theme == DialogTheme.LIGHT && Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
			{
                try
                {
                    dialog.SetInverseBackgroundForced(true);
                    if (!builder.titleColorSet)
                    {
                        dialog.title.SetTextColor(Color.Black);
                    }
                    if (!builder.contentColorSet)
                    {
                        dialog.content.SetTextColor(Color.Black);
                    }
                }
                catch(System.Exception e)
                {
                    Console.WriteLine(e.Message);
                }
			}
		}

		private static void SetupProgressDialog(MaterialDialog dialog)
		{
			MaterialDialog.Builder builder = dialog.mBuilder;
			if (builder.mIndeterminateProgress || builder.mProgress > -2)
			{
				dialog.mProgress = (ProgressBar) dialog.view.FindViewById(Android.Resource.Id.Progress);

				// Manually color progress bar on pre-Lollipop, since Material/AppCompat themes only do it on API 21+
				if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
				{
					Drawable indDraw = dialog.mProgress.IndeterminateDrawable;
					if (indDraw != null)
					{
						indDraw.SetColorFilter(builder.accentColor, PorterDuff.Mode.SrcAtop);
						dialog.mProgress.IndeterminateDrawable = indDraw;
					}
					Drawable regDraw = dialog.mProgress.ProgressDrawable;
					if (regDraw != null)
					{
                        regDraw.SetColorFilter(builder.accentColor, PorterDuff.Mode.SrcAtop);
						dialog.mProgress.ProgressDrawable = regDraw;
					}
				}

				if (!builder.mIndeterminateProgress)
				{
					dialog.mProgress.Progress = 0;
					dialog.mProgress.Max = builder.mProgressMax;
					dialog.mProgressLabel = (TextView) dialog.view.FindViewById(Resource.Id.label);
					dialog.mProgressMinMax = (TextView) dialog.view.FindViewById(Resource.Id.minMax);
					if (builder.mShowMinMax)
					{
						dialog.mProgressMinMax.Visibility = ViewStates.Visible;
						dialog.mProgressMinMax.Text = "0/" + builder.mProgressMax;
						ViewGroup.MarginLayoutParams lp = (ViewGroup.MarginLayoutParams) dialog.mProgress.LayoutParameters;
						lp.LeftMargin = 0;
						lp.RightMargin = 0;
					}
					else
					{
						dialog.mProgressMinMax.Visibility = ViewStates.Gone;
					}
					dialog.mProgressLabel.Text = "0%";
				}

				if (builder.title == null)
				{
					// Redistribute main frame's bottom padding to the top padding if there's no title
					View mainFrame = dialog.view.FindViewById(Resource.Id.mainFrame);
					mainFrame.SetPadding(mainFrame.PaddingLeft, mainFrame.PaddingBottom, mainFrame.PaddingRight, mainFrame.PaddingBottom);
				}
			}
		}

        /// <summary>
        /// Convenience method for setting the currently selected indices of a multi choice list.
        /// This only works if you are not using a custom adapter; if you're using a custom adapter,
        /// an IllegalStateException is thrown. Note that this does not call the respective multi choice callback.
        /// </summary>
        /// <param name="indices"> The indices of the list items to check. </param>
        public static int GetLayoutForType(ListType type)
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
	}
}