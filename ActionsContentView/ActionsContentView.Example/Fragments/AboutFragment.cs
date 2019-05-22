using Android.App;
using Android.Net;
using Android.OS;
using Android.Views;
using Android.Runtime;
using System;

using Fragment = Android.Support.V4.App.Fragment;

namespace ActionsContentView.Example
{
    public class AboutFragment : Fragment
    {
        public static readonly string TAG = typeof(AboutFragment).FullName;

        private static string ABOUT_SCHEME = "settings";
        private static string ABOUT_AUTHORITY = "about";
        public static Android.Net.Uri ABOUT_URI = new Android.Net.Uri.Builder().Scheme(ABOUT_SCHEME).Authority(ABOUT_AUTHORITY).Build();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            View v = inflater.Inflate(Resource.Layout.about, container, false);

            v.FindViewById(Resource.Id.play).Click += AboutFragment_Click;

            return v;
        }

        void AboutFragment_Click(object sender, EventArgs e)
        {
            Activity a = Activity;
            if (a is ExamplesActivity)
            {
                ExamplesActivity examplesActivity = (ExamplesActivity)a;
                examplesActivity.UpdateContent(SandboxFragment.SETTINGS_URI);
            }
        }
    }
}