using Android.Content;

namespace Example
{
    public class Util
    {
        public static void GoToGitHub(Context context)
        {
            var uriUrl = Android.Net.Uri.Parse("http://github.com/Cheesebaron/SlidingMenu");
            var launchBrowser = new Intent(Intent.ActionView, uriUrl);
            context.StartActivity(launchBrowser);
        }
    }
}