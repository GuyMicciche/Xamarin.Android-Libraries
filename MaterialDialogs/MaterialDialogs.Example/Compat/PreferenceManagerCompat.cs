using Android.App;
using Android.Content;
using Android.Preferences;
using Java.Lang.Reflect;
using System;

namespace MaterialDialogs.Example
{
	public class PreferenceManagerCompat: Java.Lang.Object
	{
		private static readonly string TAG = Java.Lang.Class.FromType(typeof(PreferenceManagerCompat)).SimpleName;

		/// <summary>
		/// Interface definition for a callback to be invoked when a
		/// <seealso cref="Preference"/> in the hierarchy rooted at this <seealso cref="PreferenceScreen"/> is
		/// clicked.
		/// </summary>
		internal interface IOnPreferenceTreeClickListener
		{
			/// <summary>
			/// Called when a preference in the tree rooted at this
			/// <seealso cref="PreferenceScreen"/> has been clicked.
			/// </summary>
			/// <param name="preferenceScreen"> The <seealso cref="PreferenceScreen"/> that the
			///                         preference is located in. </param>
			/// <param name="preference">       The preference that was clicked. </param>
			/// <returns> Whether the click was handled. </returns>
			bool OnPreferenceTreeClick(PreferenceScreen preferenceScreen, Preference preference);
		}

		internal static PreferenceManager NewInstance(Activity activity, int firstRequestCode)
		{
			try
			{
                Constructor c = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredConstructor(Java.Lang.Class.FromType(typeof(Activity)), Java.Lang.Integer.Type);
				c.Accessible = true;
				return (PreferenceManager)c.NewInstance(activity, firstRequestCode);
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + " -> Couldn't call constructor PreferenceManager by reflection -> " + e.Message);
			}
			return null;
		}

		/// <summary>
		/// Sets the owning preference fragment
		/// </summary>
		internal static void SetFragment(PreferenceManager manager, PreferenceFragment fragment)
		{
			// stub
		}

		/// <summary>
		/// Sets the callback to be invoked when a <seealso cref="Preference"/> in the
		/// hierarchy rooted at this <seealso cref="PreferenceManager"/> is clicked.
		/// </summary>
		/// <param name="listener"> The callback to be invoked. </param>
		internal static void SetOnPreferenceTreeClickListener(PreferenceManager manager, IOnPreferenceTreeClickListener listener)
		{
			try
			{
				Field onPreferenceTreeClickListener = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredField("mOnPreferenceTreeClickListener");
				onPreferenceTreeClickListener.Accessible = true;
				if (listener != null)
				{
					Java.Lang.Object proxy = Proxy.NewProxyInstance(onPreferenceTreeClickListener.Type.ClassLoader, new Java.Lang.Class[]{ onPreferenceTreeClickListener.Type }, new InvocationHandlerAnonymousInnerClassHelper(listener));
					onPreferenceTreeClickListener.Set(manager, proxy);
				}
				else
				{
					onPreferenceTreeClickListener.Set(manager, null);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + "Couldn't set PreferenceManager.mOnPreferenceTreeClickListener by reflection -> " + e.Message);
			}
		}

		private class InvocationHandlerAnonymousInnerClassHelper : Java.Lang.Object, IInvocationHandler
		{
			private PreferenceManagerCompat.IOnPreferenceTreeClickListener listener;

			public InvocationHandlerAnonymousInnerClassHelper(PreferenceManagerCompat.IOnPreferenceTreeClickListener listener)
			{
				this.listener = listener;
			}

            public Java.Lang.Object Invoke(Java.Lang.Object proxy, Method method, Java.Lang.Object[] args)
			{
				if (method.Name.Equals("onPreferenceTreeClick"))
				{
					return Convert.ToBoolean(listener.OnPreferenceTreeClick((PreferenceScreen) args[0], (Preference) args[1]));
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Inflates a preference hierarchy from the preference hierarchies of
		/// <seealso cref="Activity Activities"/> that match the given <seealso cref="Intent"/>. An
		/// <seealso cref="Activity"/> defines its preference hierarchy with meta-data using
		/// the <seealso cref="#METADATA_KEY_PREFERENCES"/> key.
		/// <p/>
		/// If a preference hierarchy is given, the new preference hierarchies will
		/// be merged in.
		/// </summary>
		/// <param name="queryIntent">     The intent to match activities. </param>
		/// <param name="rootPreferences"> Optional existing hierarchy to merge the new
		///                        hierarchies into. </param>
		/// <returns> The root hierarchy (if one was not provided, the new hierarchy's
		/// root). </returns>
		internal static PreferenceScreen InflateFromIntent(PreferenceManager manager, Intent intent, PreferenceScreen screen)
		{
			try
			{
                Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("inflateFromIntent", Java.Lang.Class.FromType(typeof(Intent)), Java.Lang.Class.FromType(typeof(PreferenceScreen)));
				m.Accessible = true;
				PreferenceScreen prefScreen = (PreferenceScreen) m.Invoke(manager, intent, screen);
				return prefScreen;
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + " -> Couldn't call PreferenceManager.inflateFromIntent by reflection -> " + e.Message);
			}
			return null;
		}

		/// <summary>
		/// Inflates a preference hierarchy from XML. If a preference hierarchy is
		/// given, the new preference hierarchies will be merged in.
		/// </summary>
		/// <param name="context">         The context of the resource. </param>
		/// <param name="resId">           The resource ID of the XML to inflate. </param>
		/// <param name="rootPreferences"> Optional existing hierarchy to merge the new
		///                        hierarchies into. </param>
		/// <returns> The root hierarchy (if one was not provided, the new hierarchy's
		/// root).
		/// @hide </returns>
		internal static PreferenceScreen InflateFromResource(PreferenceManager manager, Activity activity, int resId, PreferenceScreen screen)
		{
			try
			{
				Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("inflateFromResource", Java.Lang.Class.FromType(typeof(Context)), Java.Lang.Integer.Type, Java.Lang.Class.FromType(typeof(PreferenceScreen)));
				m.Accessible = true;
				PreferenceScreen prefScreen = (PreferenceScreen)m.Invoke(manager, activity, resId, screen);
				return prefScreen;
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + "Couldn't call PreferenceManager.inflateFromResource by reflection -> " + e.Message);
			}
			return null;
		}

		/// <summary>
		/// Returns the root of the preference hierarchy managed by this class.
		/// </summary>
		/// <returns> The <seealso cref="PreferenceScreen"/> object that is at the root of the hierarchy. </returns>
		internal static PreferenceScreen GetPreferenceScreen(PreferenceManager manager)
		{
			try
			{
				Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("getPreferenceScreen");
				m.Accessible = true;
				return (PreferenceScreen) m.Invoke(manager);
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + "Couldn't call PreferenceManager.getPreferenceScreen by reflection -> " + e.Message);
			}
			return null;
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch a subactivity result.
		/// </summary>
		internal static void DispatchActivityResult(PreferenceManager manager, int requestCode, int resultCode, Intent data)
		{
			try
			{
                Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("dispatchActivityResult", Java.Lang.Integer.Type, Java.Lang.Integer.Type, Java.Lang.Class.FromType(typeof(Intent)));
				m.Accessible = true;
				m.Invoke(manager, requestCode, resultCode, data);
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + "Couldn't call PreferenceManager.dispatchActivityResult by reflection -> " + e.Message);
			}
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch the activity stop
		/// event.
		/// </summary>
		internal static void DispatchActivityStop(PreferenceManager manager)
		{
			try
			{
				Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("dispatchActivityStop");
				m.Accessible = true;
				m.Invoke(manager);
			}
			catch (Exception e)
			{
                Console.WriteLine(TAG + "Couldn't call PreferenceManager.dispatchActivityStop by reflection -> " + e.Message);
			}
		}

		/// <summary>
		/// Called by the <seealso cref="PreferenceManager"/> to dispatch the activity destroy
		/// event.
		/// </summary>
		internal static void DispatchActivityDestroy(PreferenceManager manager)
		{
			try
			{
				Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("dispatchActivityDestroy");
				m.Accessible = true;
				m.Invoke(manager);
			}
			catch (Exception e)
			{
                Console.WriteLine(TAG + "Couldn't call PreferenceManager.dispatchActivityDestroy by reflection -> " + e.Message);
			}
		}

		/// <summary>
		/// Sets the root of the preference hierarchy.
		/// </summary>
		/// <param name="preferenceScreen"> The root <seealso cref="PreferenceScreen"/> of the preference hierarchy. </param>
		/// <returns> Whether the <seealso cref="PreferenceScreen"/> given is different than the previous. </returns>
		internal static bool SetPreferences(PreferenceManager manager, PreferenceScreen screen)
		{
			try
			{
				Method m = Java.Lang.Class.FromType(typeof(PreferenceManager)).GetDeclaredMethod("setPreferences", Java.Lang.Class.FromType(typeof(PreferenceScreen)));
				m.Accessible = true;
				return ((bool) m.Invoke(manager, screen));
			}
			catch (Exception e)
			{
				Console.WriteLine(TAG + "Couldn't call PreferenceManager.setPreferences by reflection -> " + e.Message);
			}
			return false;
		}
	}
}