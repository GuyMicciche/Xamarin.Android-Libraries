using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace AndroidStyledDialogs.Example
{
    public class JayneHatDialogFragment : SimpleDialogFragment
    {
        public static string TAG = "jayne";

        public static void Show(FragmentActivity activity)
        {
            (new JayneHatDialogFragment()).Show(activity.SupportFragmentManager, TAG);
        }

        protected override BaseDialogFragment.Builder Build(BaseDialogFragment.Builder builder)
        {
            builder.SetTitle("Jayne's hat");
            builder.SetView(LayoutInflater.From(Activity).Inflate(Resource.Layout.view_jayne_hat, null));
            builder.SetPositiveButton("I want one", (sender, e) =>
            {
                Dismiss();
                Toast.MakeText(Activity, "Hello", ToastLength.Long).Show();
            });
            return builder;
        }
    }
}