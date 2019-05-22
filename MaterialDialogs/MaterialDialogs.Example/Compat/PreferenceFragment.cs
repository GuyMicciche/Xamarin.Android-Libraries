using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Lang.Reflect;
using System;

namespace MaterialDialogs.Example
{
	public abstract class PreferenceFragment : Fragment, PreferenceManagerCompat.IOnPreferenceTreeClickListener
	{
		private const string PREFERENCES_TAG = "android:preferences";

		private PreferenceManager mPreferenceManager;
		private ListView mList;
		private bool mHavePrefs;
		private bool mInitDone;

		/// <summary>
		/// The starting request code given out to preference framework.
		/// </summary>
		private const int FIRST_REQUEST_CODE = 100;

		private const int MSG_BIND_PREFERENCES = 1;
        private Handler mHandler;
        private IRunnable mRequestFocus;

		private class MyHandler : Handler
		{
            private PreferenceFragment fragment;

            public MyHandler(PreferenceFragment fragment)
			{
                this.fragment = fragment;
			}

			public override void HandleMessage(Message msg)
			{
				switch (msg.What)
				{

					case MSG_BIND_PREFERENCES:
                        fragment.BindPreferences();
						break;
				}
			}
		}

        private class MyRunnable : Java.Lang.Object, IRunnable
		{
            private PreferenceFragment fragment;

            public MyRunnable(PreferenceFragment fragment)
			{
                this.fragment = fragment;
			}

			public virtual void Run()
			{
                fragment.mList.FocusableViewAvailable(fragment.mList);
			}
		}

		/// <summary>
		/// Interface that PreferenceFragment's containing activity should
		/// implement to be able to process preference items that wish to
		/// switch to a new fragment.
		/// </summary>
		public interface IOnPreferenceStartFragmentCallback
		{
			/// <summary>
			/// Called when the user has clicked on a Preference that has
			/// a fragment class name associated with it.  The implementation
			/// to should instantiate and switch to an instance of the given
			/// fragment.
			/// </summary>
			bool OnPreferenceStartFragment(PreferenceFragment caller, Preference pref);
		}

        public PreferenceFragment()
        {
            mHandler = new MyHandler(this);
            mRequestFocus = new MyRunnable(this);
        }

		public override void OnCreate(Bundle paramBundle)
		{
			base.OnCreate(paramBundle);
			mPreferenceManager = PreferenceManagerCompat.NewInstance(Activity, FIRST_REQUEST_CODE);
			PreferenceManagerCompat.SetFragment(mPreferenceManager, this);
		}

		public override View OnCreateView(LayoutInflater paramLayoutInflater, ViewGroup paramViewGroup, Bundle paramBundle)
		{
			return paramLayoutInflater.Inflate(Resource.Layout.preference_list_fragment, paramViewGroup, false);
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);

			if (mHavePrefs)
			{
				BindPreferences();
			}

			mInitDone = true;

			if (savedInstanceState != null)
			{
				Bundle container = savedInstanceState.GetBundle(PREFERENCES_TAG);
				if (container != null)
				{
					PreferenceScreen preferenceScreen = PreferenceScreen;
					if (preferenceScreen != null)
					{
						preferenceScreen.RestoreHierarchyState(container);
					}
				}
			}
		}

		public override void OnStart()
		{
			base.OnStart();
			PreferenceManagerCompat.SetOnPreferenceTreeClickListener(mPreferenceManager, this);
		}

		public override void OnStop()
		{
			base.OnStop();
			PreferenceManagerCompat.DispatchActivityStop(mPreferenceManager);
			PreferenceManagerCompat.SetOnPreferenceTreeClickListener(mPreferenceManager, null);
		}

		public override void OnDestroyView()
		{
			mList = null;
			mHandler.RemoveCallbacks(mRequestFocus);
			mHandler.RemoveMessages(MSG_BIND_PREFERENCES);
			base.OnDestroyView();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			PreferenceManagerCompat.DispatchActivityDestroy(mPreferenceManager);
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			PreferenceScreen preferenceScreen = PreferenceScreen;
			if (preferenceScreen != null)
			{
				Bundle container = new Bundle();
				preferenceScreen.SaveHierarchyState(container);
				outState.PutBundle(PREFERENCES_TAG, container);
			}
		}

		public override void OnActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			PreferenceManagerCompat.DispatchActivityResult(mPreferenceManager, requestCode, resultCode, data);
		}

		/// <summary>
		/// Returns the <seealso cref="PreferenceManager"/> used by this fragment. </summary>
		/// <returns> The <seealso cref="PreferenceManager"/>. </returns>
		public virtual PreferenceManager PreferenceManager
		{
			get
			{
				return mPreferenceManager;
			}
		}

		/// <summary>
		/// Sets the root of the preference hierarchy that this fragment is showing.
		/// </summary>
		/// <param name="preferenceScreen"> The root <seealso cref="PreferenceScreen"/> of the preference hierarchy. </param>
		public virtual PreferenceScreen PreferenceScreen
		{
			set
			{
				if (PreferenceManagerCompat.SetPreferences(mPreferenceManager, value) && value != null)
				{
					mHavePrefs = true;
					if (mInitDone)
					{
						PostBindPreferences();
					}
				}
			}
			get
			{
				return PreferenceManagerCompat.GetPreferenceScreen(mPreferenceManager);
			}
		}


		/// <summary>
		/// Adds preferences from activities that match the given <seealso cref="Intent"/>.
		/// </summary>
		/// <param name="intent"> The <seealso cref="Intent"/> to query activities. </param>
		public virtual void AddPreferencesFromIntent(Intent intent)
		{
			RequirePreferenceManager();

			PreferenceScreen = PreferenceManagerCompat.InflateFromIntent(mPreferenceManager, intent, PreferenceScreen);
		}

		/// <summary>
		/// Inflates the given XML resource and adds the preference hierarchy to the current
		/// preference hierarchy.
		/// </summary>
		/// <param name="preferencesResId"> The XML resource ID to inflate. </param>
		public virtual void AddPreferencesFromResource(int preferencesResId)
		{
			RequirePreferenceManager();

			PreferenceScreen = PreferenceManagerCompat.InflateFromResource(mPreferenceManager, Activity, preferencesResId, PreferenceScreen);
		}

		/// <summary>
		/// {@inheritDoc}
		/// </summary>
		public virtual bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference)
		{
			//if (preference.getFragment() != null &&
			if (Activity is IOnPreferenceStartFragmentCallback)
			{
				return ((IOnPreferenceStartFragmentCallback)Activity).OnPreferenceStartFragment(this, preference);
			}
			return false;
		}

		/// <summary>
		/// Finds a <seealso cref="Preference"/> based on its key.
		/// </summary>
		/// <param name="key"> The key of the preference to retrieve. </param>
		/// <returns> The <seealso cref="Preference"/> with the key, or null. </returns>
		/// <seealso cref= PreferenceGroup#findPreference(CharSequence) </seealso>
		public virtual Preference FindPreference(string key)
		{
			if (mPreferenceManager == null)
			{
				return null;
			}
			return mPreferenceManager.FindPreference(key);
		}

		private void RequirePreferenceManager()
		{
			if (mPreferenceManager == null)
			{
				throw new System.Exception("This should be called after super.onCreate.");
			}
		}

		private void PostBindPreferences()
		{
			if (mHandler.HasMessages(MSG_BIND_PREFERENCES))
			{
				return;
			}
			mHandler.ObtainMessage(MSG_BIND_PREFERENCES).SendToTarget();
		}

		private void BindPreferences()
		{
			PreferenceScreen preferenceScreen = PreferenceScreen;
			if (preferenceScreen != null)
			{
				preferenceScreen.Bind(GetListView());
			}

			if (Build.VERSION.SdkInt <= BuildVersionCodes.GingerbreadMr1)
			{
				// Workaround android bug for SDK 10 and below - see
				// https://github.com/android/platform_frameworks_base/commit/2d43d283fc0f22b08f43c6db4da71031168e7f59
                GetListView().ItemClick += (sender, e) =>
                    {
                        int position = e.Position;

                        // If the list has headers, subtract them from the index.
                        if (e.Parent is ListView)
                        {
                            position -= ((ListView)e.Parent).HeaderViewsCount;
                        }

                        Java.Lang.Object item = preferenceScreen.RootAdapter.GetItem(position);
                        if (!(item is Preference))
                        {
                            return;
                        }

                        Preference preference = (Preference)item;
                        try
                        {
                            Method performClick = Java.Lang.Class.FromType(typeof(Preference)).GetDeclaredMethod("performClick", Java.Lang.Class.FromType(typeof(PreferenceScreen)));
                            performClick.Accessible = true;
                            performClick.Invoke(preference, preferenceScreen);
                        }
                        catch (InvocationTargetException ex)
                        {
                        }
                        catch (IllegalAccessException ex)
                        {
                        }
                        catch (NoSuchMethodException ex)
                        {
                        }
                    };
			}
		}

		public virtual ListView GetListView()
		{
			EnsureList();
			return mList;
		}

		private void EnsureList()
		{
			if (mList != null)
			{
				return;
			}
			View root = View;
			if (root == null)
			{
				throw new IllegalStateException("Content view not yet created");
			}
			View rawListView = root.FindViewById(Android.Resource.Id.List);
			if (!(rawListView is ListView))
			{
				throw new System.Exception("Content has view with id attribute 'android.R.id.list' " + "that is not a ListView class");
			}
			mList = (ListView)rawListView;
			if (mList == null)
			{
                throw new System.Exception("Your content must have a ListView whose id attribute is " + "'android.R.id.list'");
			}
            mList.KeyPress += (sender, e) =>
                {
                    Java.Lang.Object selectedItem = mList.SelectedItem;
                    if (selectedItem is Preference)
                    {
                        View selectedView = mList.SelectedView;
                    }
                };

			mHandler.Post(mRequestFocus);
		}
	}
}