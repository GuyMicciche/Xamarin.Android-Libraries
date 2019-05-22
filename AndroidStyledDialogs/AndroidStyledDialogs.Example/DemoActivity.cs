using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Java.Lang;
using Java.Util;
using Java.Text;
using Android.Util;
using Android.Content.Res;
using Android.Net;

namespace AndroidStyledDialogs.Example
{
    [Activity(Label = "AndroidStyledDialogs.Example", MainLauncher = true, Theme = "@style/AppTheme", Icon = "@drawable/icon")]
    public class DemoActivity : ActionBarActivity, ISimpleDialogListener, IDateDialogListener, ISimpleDialogCancelListener, IListDialogListener, IMultiChoiceListDialogListener
    {
        private const int REQUEST_PROGRESS = 1;
        private const int REQUEST_LIST_SIMPLE = 9;
        private const int REQUEST_LIST_MULTIPLE = 10;
        private const int REQUEST_LIST_SINGLE = 11;
        private const int REQUEST_DATE_PICKER = 12;
        private const int REQUEST_TIME_PICKER = 13;
        private const int REQUEST_SIMPLE_DIALOG = 42;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.main);

            SupportActionBar.SetDisplayShowHomeEnabled(true);
            SupportActionBar.SetIcon(Resource.Drawable.img_avast_logo_small);

            FindViewById(Resource.Id.message_dialog).Click += (sender, e) =>
            {
                SimpleDialogFragment.CreateBuilder(this, SupportFragmentManager).SetMessage("Love. Can know all the math in the \'verse but take a boat in the air that you don\'t " + "love? She\'ll shake you off just as sure as a turn in the worlds. Love keeps her in the air when " + "she oughtta fall down...tell you she\'s hurtin\' \'fore she keens...makes her a home.").Show();
            };
            FindViewById(Resource.Id.message_title_dialog).Click += (sender, e) =>
            {
                SimpleDialogFragment.CreateBuilder(this, SupportFragmentManager).SetTitle("More Firefly quotes:").SetMessage("Wash: \"Psychic, though? That sounds like something out of science fiction.\"\n\nZoe: \"We live" + " " + "in a space ship, dear.\"\nWash: \"Here lies my beloved Zoe, " + ("my autumn flower ... somewhat less attractive now that she's all corpsified and gross" + ".\"\n\nRiver Tam: \"Also? I can kill you with my brain.\"\n\nKayle: \"Going on a year now, nothins twixed my neathers not run on batteries.\" \n" + "Mal: \"I can't know that.\" \n" + "Jayne: \"I can stand to hear a little more.\"\n\nWash: \"I've been under fire before. " + "Well ... I've been in a fire. Actually, I was fired. I can handle myself.\"")).SetNegativeButtonText("Close").Show();
            };
            FindViewById(Resource.Id.message_title_buttons_dialog).Click += (sender, e) =>
            {
                SimpleDialogFragment.CreateBuilder(this, SupportFragmentManager).SetTitle("Do you like this quote?").SetMessage("Jayne: \"Shiny. Let's be bad guys.\"").SetPositiveButtonText("Love").SetNegativeButtonText("Hate").SetNeutralButtonText("WTF?").SetRequestCode(REQUEST_SIMPLE_DIALOG).Show();
            };
            FindViewById(Resource.Id.long_buttons).Click += (sender, e) =>
            {
                SimpleDialogFragment.CreateBuilder(this, SupportFragmentManager).SetMessage("How will you decide?").SetPositiveButtonText("Time for some thrillin' heroics!").SetNegativeButtonText("Misbehave").SetNeutralButtonText("Keep flying").Show();
            };
            FindViewById(Resource.Id.progress_dialog).Click += (sender, e) =>
            {
                ProgressDialogFragment.CreateBuilder(this, SupportFragmentManager).SetMessage("Mal: I\'m just waiting to see if I pass out. Long story.").SetRequestCode(REQUEST_PROGRESS).Show();
            };
            FindViewById(Resource.Id.list_dialog_simple).Click += (sender, e) =>
            {
                ListDialogFragment.CreateBuilder(this, SupportFragmentManager).SetTitle("Your favorite character:").SetItems(new string[] { "Jayne", "Malcolm", "Kaylee", "Wash", "Zoe", "River" }).SetRequestCode(REQUEST_LIST_SIMPLE).Show();
            };
            FindViewById(Resource.Id.list_dialog_single).Click += (sender, e) =>
            {
                ListDialogFragment.CreateBuilder(this, SupportFragmentManager).SetTitle("Your favorite character:").SetItems(new string[] { "Jayne", "Malcolm", "Kaylee", "Wash", "Zoe", "River" }).SetRequestCode(REQUEST_LIST_SINGLE).SetChoiceMode(AbsListViewChoiceMode.Single).Show();
            };
            FindViewById(Resource.Id.list_dialog_multiple).Click += (sender, e) =>
            {
                ListDialogFragment.CreateBuilder(this, SupportFragmentManager).SetTitle("Your favorite character:").SetItems(new string[] { "Jayne", "Malcolm", "Kaylee", "Wash", "Zoe", "River" }).SetRequestCode(REQUEST_LIST_MULTIPLE).SetChoiceMode(AbsListViewChoiceMode.Multiple).SetCheckedItems(new int[] { 1, 3 }).Show();
            };
            FindViewById(Resource.Id.custom_dialog).Click += (sender, e) =>
            {
                JayneHatDialogFragment.Show(this);
            };
            FindViewById(Resource.Id.time_picker).Click += (sender, e) =>
            {
                TimePickerDialogFragment.CreateBuilder(this, SupportFragmentManager).SetDate(new Date()).SetPositiveButtonText(Android.Resource.String.Ok).SetNegativeButtonText(Android.Resource.String.Cancel).SetRequestCode(REQUEST_TIME_PICKER).Show();
            };
            FindViewById(Resource.Id.date_picker).Click += (sender, e) =>
            {
                DatePickerDialogFragment.CreateBuilder(this, SupportFragmentManager).SetDate(new Date()).SetPositiveButtonText(Android.Resource.String.Ok).SetNegativeButtonText(Android.Resource.String.Cancel).SetRequestCode(REQUEST_DATE_PICKER).Show();
            };
        }

        // IListDialogListener
        public void OnListItemSelected(string value, int number, int requestCode)
        {
            if (requestCode == REQUEST_LIST_SIMPLE || requestCode == REQUEST_LIST_SINGLE)
            {
                Toast.MakeText(this, "Selected: " + value, ToastLength.Short).Show();
            }
        }

        public void OnListItemsSelected(string[] values, int[] selectedPositions, int requestCode)
        {
            if (requestCode == REQUEST_LIST_MULTIPLE)
            {
                StringBuilder sb = new StringBuilder();
                foreach (string value in values)
                {
                    if (sb.Length() > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append(value);

                }
                Toast.MakeText(this, "Selected: " + sb.ToString(), ToastLength.Short).Show();
            }
        }

        // ISimpleDialogCancelListener
        public void OnCancelled(int requestCode)
        {
            switch (requestCode)
            {
                case REQUEST_SIMPLE_DIALOG:
                    Toast.MakeText(this, "Dialog cancelled", ToastLength.Short).Show();
                    break;
                case REQUEST_PROGRESS:
                    Toast.MakeText(this, "Progress dialog cancelled", ToastLength.Short).Show();
                    break;
                case REQUEST_LIST_SIMPLE:
                case REQUEST_LIST_SINGLE:
                case REQUEST_LIST_MULTIPLE:
                    Toast.MakeText(this, "Nothing selected", ToastLength.Short).Show();
                    break;
                case REQUEST_DATE_PICKER:
                    Toast.MakeText(this, "Date picker cancelled", ToastLength.Short).Show();
                    break;
                case REQUEST_TIME_PICKER:
                    Toast.MakeText(this, "Time picker cancelled", ToastLength.Short).Show();
                    break;
            }
        }

        // ISimpleDialogListener
        public void OnPositiveButtonClicked(int requestCode)
        {
            if (requestCode == REQUEST_SIMPLE_DIALOG)
            {
                Toast.MakeText(this, "Positive button clicked", ToastLength.Short).Show();
            }
        }

        public void OnNegativeButtonClicked(int requestCode)
        {
            if (requestCode == REQUEST_SIMPLE_DIALOG)
            {
                Toast.MakeText(this, "Negative button clicked", ToastLength.Short).Show();
            }
        }

        public void OnNeutralButtonClicked(int requestCode)
        {
            if (requestCode == REQUEST_SIMPLE_DIALOG)
            {
                Toast.MakeText(this, "Neutral button clicked", ToastLength.Short).Show();
            }
        }

        // IDateDialogListener
        public void OnNegativeButtonClicked(int resultCode, Date date)
        {
            string text = "";
            if (resultCode == REQUEST_DATE_PICKER)
            {
                text = "Date ";
            }
            else if (resultCode == REQUEST_TIME_PICKER)
            {
                text = "Time ";
            }

            DateFormat dateFormat = DateFormat.GetDateInstance(DateFormat.Default);
            Toast.MakeText(this, text + "Cancelled " + dateFormat.Format(date), ToastLength.Short).Show();
        }

        public void OnPositiveButtonClicked(int resultCode, Date date)
        {
            string text = "";
            if (resultCode == REQUEST_DATE_PICKER)
            {
                text = "Date ";
            }
            else if (resultCode == REQUEST_TIME_PICKER)
            {
                text = "Time ";
            }

            DateFormat dateFormat = DateFormat.DateTimeInstance;
            Toast.MakeText(this, text + "Success! " + dateFormat.Format(date), ToastLength.Short).Show();
        }

        // Menu
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);
            MenuInflater inflater = MenuInflater;
            inflater.Inflate(Resource.Menu.main, menu);

            if (DarkTheme)
            {
                menu.FindItem(Resource.Id.theme_change).SetTitle("Use Light Theme");
            }
            else
            {
                menu.FindItem(Resource.Id.theme_change).SetTitle("Use Dark Theme");
            }
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.theme_change:
					if (DarkTheme)
					{
						SetTheme(Resource.Style.AppTheme);
						Toast.MakeText(this, "Light theme set", ToastLength.Short).Show();
					}
					else
					{
                        SetTheme(Resource.Style.AppThemeDark);
						Toast.MakeText(this, "Dark theme set", ToastLength.Short).Show();
					}
					InvalidateOptionsMenu();
					return true;
				case Resource.Id.about:
					Intent i = new Intent(Intent.ActionView);
					i.SetData(Uri.Parse("https://github.com/avast/android-styled-dialogs"));
					StartActivity(i);
					return true;
			}
			return base.OnOptionsItemSelected(item);
		}

        private bool DarkTheme
        {
            get
            {
                bool darkTheme = false;
                //Try-catch block is used to overcome resource not found exception
                try
                {
                    TypedValue val = new TypedValue();

                    //Reading attr value from current theme
                    Theme.ResolveAttribute(Resource.Attribute.isLightTheme, val, true);

                    //Passing the resource ID to TypedArray to get the attribute value
                    TypedArray arr = ObtainStyledAttributes(val.Data, new int[] { Resource.Attribute.isLightTheme });
                    darkTheme = !arr.GetBoolean(0, false);
                    arr.Recycle();
                }
                catch (Exception e)
                {
                    //Resource not found , so sticking to light theme
                    darkTheme = false;
                }
                return darkTheme;
            }
        }
    }
}