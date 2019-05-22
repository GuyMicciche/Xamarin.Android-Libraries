using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;

namespace MaterialDialogs.Example
{
    [Activity(Label = "@string/preference_dialogs", Icon = "@drawable/ic_launcher")]
	public class SettingsActivity : ActionBarActivity
	{
		public class SettingsFragment : PreferenceFragment
		{
			public override void OnCreate(Bundle savedInstanceState)
			{
				base.OnCreate(savedInstanceState);
				AddPreferencesFromResource(Resource.Layout.preferences);
			}
		}

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.preference_activity_custom);
			SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.content_frame, new SettingsFragment()).Commit();
		}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }
	}
}