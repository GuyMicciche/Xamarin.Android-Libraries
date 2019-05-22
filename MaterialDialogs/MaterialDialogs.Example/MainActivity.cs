using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Lang;
using Android.Text.Method;
using Android.Text;
using Android.Webkit;
using Java.IO;
using Android.Support.V7.App;
using Android.Graphics.Drawables;
using Android.Graphics;

namespace MaterialDialogs.Example
{
    [Activity(Label = "MaterialDialogs.Example", Theme = "@style/AppTheme", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : ActionBarActivity
    {
        private Toast mToast;

        private void ShowToast(string message)
        {
            if (mToast != null)
            {
                mToast.Cancel();
                mToast = null;
            }
            mToast = Toast.MakeText(this, message, ToastLength.Short);
            mToast.Show();
        }

        private void ShowToast(int message)
        {
            ShowToast(GetString(message));
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            FindViewById(Resource.Id.basicNoTitle).Click += (sender, e) =>
            {
                ShowBasicNoTitle();
            };

            FindViewById(Resource.Id.basic).Click += (sender, e) =>
            {
                ShowBasic();
            };

            FindViewById(Resource.Id.basicLongContent).Click += (sender, e) =>
            {
                ShowBasicLongContent();
            };

            FindViewById(Resource.Id.basicIcon).Click += (sender, e) =>
            {
                ShowBasicIcon();
            };

            FindViewById(Resource.Id.stacked).Click += (sender, e) =>
            {
                ShowStacked();
            };

            FindViewById(Resource.Id.neutral).Click += (sender, e) =>
            {
                ShowNeutral();
            };

            FindViewById(Resource.Id.callbacks).Click += (sender, e) =>
            {
                ShowCallbacks();
            };

            FindViewById(Resource.Id.list).Click += (sender, e) =>
            {
                ShowList();
            };

            FindViewById(Resource.Id.listNoTitle).Click += (sender, e) =>
            {
                ShowListNoTitle();
            };

            FindViewById(Resource.Id.longList).Click += (sender, e) =>
            {
                ShowLongList();
            };

            FindViewById(Resource.Id.singleChoice).Click += (sender, e) =>
            {
                ShowSingleChoice();
            };

            FindViewById(Resource.Id.multiChoice).Click += (sender, e) =>
            {
                ShowMultiChoice();
            };

            FindViewById(Resource.Id.multiChoiceLimited).Click += (sender, e) =>
            {
                ShowMultiChoiceLimited();
            };

            FindViewById(Resource.Id.customListItems).Click += (sender, e) =>
            {
                ShowCustomList();
            };

            FindViewById(Resource.Id.customView).Click += (sender, e) =>
            {
                ShowCustomView();
            };

            FindViewById(Resource.Id.customView_webView).Click += (sender, e) =>
            {
                ShowCustomWebView();
            };

            FindViewById(Resource.Id.customView_colorChooser).Click += (sender, e) =>
            {
                ShowCustomColorChooser();
            };

            FindViewById(Resource.Id.themed).Click += (sender, e) =>
            {
                ShowThemed();
            };

            FindViewById(Resource.Id.showCancelDismiss).Click += (sender, e) =>
            {
                ShowShowCancelDismissCallbacks();
            };

            FindViewById(Resource.Id.folder_chooser).Click += (sender, e) =>
            {
                (new FolderSelectorDialog()).Show(this);
            };

            FindViewById(Resource.Id.progress1).Click += (sender, e) =>
            {
                ShowProgressDialog(false);
            };

            FindViewById(Resource.Id.progress2).Click += (sender, e) =>
            {
                ShowProgressDialog(true);
            };

            FindViewById(Resource.Id.preference_dialogs).Click += (sender, e) =>
            {
                StartActivity(new Intent(ApplicationContext, typeof(SettingsActivity)));
            };
        }

        private void ShowBasicNoTitle()
        {
            (new MaterialDialog.Builder(this)).SetContent(Resource.String.shareLocationPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).Show();
        }

        private void ShowBasic()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).Show();
        }

        private void ShowBasicLongContent()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.loremIpsum).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).Show();
        }

        private void ShowBasicIcon()
        {
            (new MaterialDialog.Builder(this)).SetIconRes(Resource.Drawable.ic_launcher).LimitIconToDefaultSize().SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).Show(); // limits the displayed icon size to 32dp
        }

        private void ShowStacked()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.speedBoost).SetNegativeText(Resource.String.noThanks).SetForceStacking(true).Show(); // this generally should not be forced, but is used for demo purposes
        }

        private void ShowNeutral()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).SetNeutralText(Resource.String.more_info).Show();
        }

        private void ShowCallbacks()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).SetNeutralText(Resource.String.more_info).SetCallback(new ButtonCallbackAnonymousInnerClassHelper(this)).Show();
        }

        private class ButtonCallbackAnonymousInnerClassHelper : MaterialDialog.ButtonCallback
        {
            private readonly MainActivity outerInstance;

            public ButtonCallbackAnonymousInnerClassHelper(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void OnPositive(MaterialDialog dialog)
            {
                outerInstance.ShowToast("Positive!");
            }

            public override void OnNeutral(MaterialDialog dialog)
            {
                outerInstance.ShowToast("Neutral");
            }

            public override void OnNegative(MaterialDialog dialog)
            {
                outerInstance.ShowToast("Negative…");
            }
        }

        private void ShowList()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.socialNetworks).SetItems(Resource.Array.socialNetworks).SetItemsCallback(new ListCallbackAnonymousInnerClassHelper(this)).Show();
        }

        private class ListCallbackAnonymousInnerClassHelper : MaterialDialog.IListCallback
        {
            private readonly MainActivity outerInstance;

            public ListCallbackAnonymousInnerClassHelper(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnSelection(MaterialDialog dialog, View view, int which, string text)
            {
                outerInstance.ShowToast(which + ": " + text);
            }
        }

        private void ShowListNoTitle()
        {
            (new MaterialDialog.Builder(this)).SetItems(Resource.Array.socialNetworks).SetItemsCallback(new ListCallbackAnonymousInnerClassHelper2(this)).Show();
        }

        private class ListCallbackAnonymousInnerClassHelper2 : MaterialDialog.IListCallback
        {
            private readonly MainActivity outerInstance;

            public ListCallbackAnonymousInnerClassHelper2(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnSelection(MaterialDialog dialog, View view, int which, string text)
            {
                outerInstance.ShowToast(which + ": " + text);
            }
        }

        private void ShowLongList()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.states).SetItems(Resource.Array.states).SetItemsCallback(new ListCallbackAnonymousInnerClassHelper3(this)).SetPositiveText(Android.Resource.String.Ok).Show();
        }

        private class ListCallbackAnonymousInnerClassHelper3 : MaterialDialog.IListCallback
        {
            private readonly MainActivity outerInstance;

            public ListCallbackAnonymousInnerClassHelper3(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnSelection(MaterialDialog dialog, View view, int which, string text)
            {
                outerInstance.ShowToast(which + ": " + text);
            }
        }

        private void ShowSingleChoice()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.socialNetworks).SetItems(Resource.Array.socialNetworks).SetItemsCallbackSingleChoice(2, new ListCallbackSingleChoiceAnonymousInnerClassHelper(this)).SetPositiveText(Resource.String.choose).Show();
        }

        private class ListCallbackSingleChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackSingleChoice
        {
            private readonly MainActivity outerInstance;

            public ListCallbackSingleChoiceAnonymousInnerClassHelper(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public bool OnSelection(MaterialDialog dialog, View view, int which, string text)
            {
                outerInstance.ShowToast(which + ": " + text);
                return true; // allow selection
            }
        }

        private void ShowMultiChoice()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.socialNetworks).SetItems(Resource.Array.socialNetworks).SetItemsCallbackMultiChoice(new int[] { 1, 3 }, new ListCallbackMultiChoiceAnonymousInnerClassHelper(this)).AlwaysCallMultiChoiceCallback().SetPositiveText(Resource.String.choose).Show();
        }

        private class ListCallbackMultiChoiceAnonymousInnerClassHelper : MaterialDialog.IListCallbackMultiChoice
        {
            private readonly MainActivity outerInstance;

            public ListCallbackMultiChoiceAnonymousInnerClassHelper(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
            {
                StringBuilder str = new StringBuilder();
                for (int i = 0; i < which.Length; i++)
                {
                    if (i > 0)
                    {
                        str.Append('\n');
                    }
                    str.Append(which[i]);
                    str.Append(": ");
                    str.Append(text[i]);
                }
                outerInstance.ShowToast(str.ToString());
                return true; // allow selection
            }
        }


        private void ShowMultiChoiceLimited()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.socialNetworks).SetItems(Resource.Array.socialNetworks).SetItemsCallbackMultiChoice(new int[] { 1 }, new ListCallbackMultiChoiceAnonymousInnerClassHelper2(this))
                 .SetPositiveText(Resource.String.dismiss).AlwaysCallMultiChoiceCallback().Show(); // the callback will always be called, to check if selection is still allowed
        }

        private class ListCallbackMultiChoiceAnonymousInnerClassHelper2 : MaterialDialog.IListCallbackMultiChoice
        {
            private readonly MainActivity outerInstance;

            public ListCallbackMultiChoiceAnonymousInnerClassHelper2(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public bool OnSelection(MaterialDialog dialog, int[] which, string[] text)
            {
                bool allowSelection = which.Length <= 2; // limit selection to 2, the new selection is included in the which array
                if (!allowSelection)
                {
                    outerInstance.ShowToast(Resource.String.selection_limit_reached);
                }
                return allowSelection;
            }
        }

        private void ShowCustomList()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.socialNetworks).SetAdapter(new ButtonItemAdapter(this, Resource.Array.socialNetworks), new ListCallbackAnonymousInnerClassHelper4(this)).Show();
        }

        private class ListCallbackAnonymousInnerClassHelper4 : MaterialDialog.IListCallback
        {
            private readonly MainActivity outerInstance;

            public ListCallbackAnonymousInnerClassHelper4(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnSelection(MaterialDialog dialog, View itemView, int which, string text)
            {
                outerInstance.ShowToast("Clicked item " + which);
            }
        }


        private EditText passwordInput;
        private View positiveAction;

        private void ShowCustomView()
        {
            MaterialDialog dialog = (new MaterialDialog.Builder(this)).SetTitle(Resource.String.googleWifi).SetCustomView(Resource.Layout.dialog_customview, true).SetPositiveText(Resource.String.connect).SetNegativeText(Android.Resource.String.Cancel).SetCallback(new ButtonCallbackAnonymousInnerClassHelper2(this)).Build();

            positiveAction = dialog.GetActionButton(DialogAction.POSITIVE);
            passwordInput = (EditText)dialog.CustomView.FindViewById(Resource.Id.password);
            passwordInput.TextChanged += (sender, e) =>
                {
                    positiveAction.Enabled = e.Text.ToString().Trim().Length > 0;
                };

            // Toggling the show password CheckBox will mask or unmask the password input EditText
            ((CheckBox)dialog.CustomView.FindViewById(Resource.Id.showPassword)).CheckedChange += (sender, e) =>
                {
                    passwordInput.InputType = !e.IsChecked ? InputTypes.TextVariationPassword : InputTypes.ClassText;
                    passwordInput.TransformationMethod = !e.IsChecked ? PasswordTransformationMethod.Instance : null;
                };

            dialog.Show();
            positiveAction.Enabled = false; // disabled by default
        }

        private class ButtonCallbackAnonymousInnerClassHelper2 : MaterialDialog.ButtonCallback
        {
            private readonly MainActivity outerInstance;

            public ButtonCallbackAnonymousInnerClassHelper2(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public override void OnPositive(MaterialDialog dialog)
            {
                outerInstance.ShowToast("Password: " + outerInstance.passwordInput.Text.ToString());
            }

            public override void OnNegative(MaterialDialog dialog)
            {

            }
        }

        private void ShowCustomWebView()
        {
            MaterialDialog dialog = (new MaterialDialog.Builder(this)).SetTitle(Resource.String.changelog).SetCustomView(Resource.Layout.dialog_webview, false).SetPositiveText(Android.Resource.String.Ok).Build();
            WebView webView = (WebView)dialog.CustomView.FindViewById(Resource.Id.webview);
            webView.LoadUrl("file:///android_asset/webview.html");
            dialog.Show();
        }

        internal static int selectedColorIndex = -1;

        private void ShowCustomColorChooser()
        {
            (new ColorChooserDialog()).Show(this, selectedColorIndex, new CallbackAnonymousInnerClassHelper(this));
        }

        private class CallbackAnonymousInnerClassHelper : ColorChooserDialog.ICallback
        {
            private readonly MainActivity activity;

            public CallbackAnonymousInnerClassHelper(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnColorSelection(int index, Color color, Color darker)
            {
                selectedColorIndex = index;
                activity.SupportActionBar.SetBackgroundDrawable(new ColorDrawable(color));
                ThemeSingleton.Get().PositiveColor = color;
                ThemeSingleton.Get().NeutralColor = color;
                ThemeSingleton.Get().NegativeColor = color;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    activity.Window.SetStatusBarColor(darker);
                }
            }
        }

        private void ShowThemed()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).SetPositiveColorRes(Resource.Color.material_red_400).SetNegativeColorRes(Resource.Color.material_red_400).SetTitleGravity(GravityEnum.CENTER).SetTitleColorRes(Resource.Color.material_red_400).SetContentColorRes(Android.Resource.Color.White).SetBackgroundColorRes(Resource.Color.material_blue_grey_800).SetDividerColorRes(Resource.Color.material_pink_500).SetBtnSelector(Resource.Drawable.md_btn_selector_custom, DialogAction.POSITIVE).SetPositiveColor(Color.White).SetNegativeColorAttr(Android.Resource.Attribute.TextColorSecondaryInverse).SetTheme(DialogTheme.DARK).Show();    
        }

        private void ShowShowCancelDismissCallbacks()
        {
            (new MaterialDialog.Builder(this)).SetTitle(Resource.String.useGoogleLocationServices).SetContent(Resource.String.useGoogleLocationServicesPrompt).SetPositiveText(Resource.String.agree).SetNegativeText(Resource.String.disagree).SetNeutralText(Resource.String.more_info).SetShowListener(new OnShowListenerAnonymousInnerClassHelper(this))
                   .SetCancelListener(new OnCancelListenerAnonymousInnerClassHelper(this))
                   .SetDismissListener(new OnDismissListenerAnonymousInnerClassHelper(this))
                   .Show();
        }

        private class OnShowListenerAnonymousInnerClassHelper : Java.Lang.Object, IDialogInterfaceOnShowListener
        {
            private readonly MainActivity outerInstance;

            public OnShowListenerAnonymousInnerClassHelper(MainActivity outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public void OnShow(IDialogInterface dialog)
            {
                outerInstance.ShowToast("onShow");
            }
        }

        private class OnCancelListenerAnonymousInnerClassHelper : Java.Lang.Object, IDialogInterfaceOnCancelListener
        {
            private readonly MainActivity activity;

            public OnCancelListenerAnonymousInnerClassHelper(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnCancel(IDialogInterface dialog)
            {
                activity.ShowToast("onCancel");
            }
        }

        private class OnDismissListenerAnonymousInnerClassHelper : Java.Lang.Object, IDialogInterfaceOnDismissListener
        {
            private readonly MainActivity activity;

            public OnDismissListenerAnonymousInnerClassHelper(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnDismiss(IDialogInterface dialog)
            {
                activity.ShowToast("onDismiss");
            }
        }

        public virtual void ShowProgressDialog(bool indeterminate)
        {
            if (indeterminate)
            {
                (new MaterialDialog.Builder(this)).SetTitle(Resource.String.progress_dialog).SetContent(Resource.String.please_wait).SetProgress(true, 0).Show();
            }
            else
            {
                (new MaterialDialog.Builder(this)).SetTitle(Resource.String.progress_dialog).SetContent(Resource.String.please_wait).SetContentGravity(GravityEnum.CENTER).SetProgress(false, 150, true).SetShowListener(new OnShowListenerAnonymousInnerClassHelper2(this)).Show();
            }
        }

        private class OnShowListenerAnonymousInnerClassHelper2 : Java.Lang.Object, IDialogInterfaceOnShowListener
        {
            private readonly MainActivity activity;

            public OnShowListenerAnonymousInnerClassHelper2(MainActivity activity)
            {
                this.activity = activity;
            }

            public void OnShow(IDialogInterface dialogInterface)
            {
                MaterialDialog dialog = (MaterialDialog)dialogInterface;
                (new Thread(new RunnableAnonymousInnerClassHelper(activity, dialog))).Start();
            }

            private class RunnableAnonymousInnerClassHelper : Java.Lang.Object, IRunnable
            {
                private readonly MainActivity activity;

                private MaterialDialog dialog;

                public RunnableAnonymousInnerClassHelper(MainActivity activity, MaterialDialog dialog)
                {
                    this.activity = activity;
                    this.dialog = dialog;
                }

                public void Run()
                {
                    while (dialog.CurrentProgress != dialog.MaxProgress)
                    {
                        if (dialog.IsCancelled)
                        {
                            break;
                        }
                        try
                        {
                            Thread.Sleep(50);
                        }
                        catch (InterruptedException e)
                        {
                            break;
                        }
                        activity.RunOnUiThread(() => dialog.IncrementProgress(1));
                    }
                    activity.RunOnUiThread(() => dialog.SetContent(activity.GetString(Resource.String.done)));
                }
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.about)
            {
                (new MaterialDialog.Builder(this)).SetTitle(Resource.String.about).SetPositiveText(Resource.String.dismiss).SetContent(Html.FromHtml(GetString(Resource.String.about_body)).ToString()).SetContentLineSpacing(1.6f).Build().Show();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public void OnFolderSelection(File folder)
        {
            ShowToast(folder.AbsolutePath);
        }
    }
}