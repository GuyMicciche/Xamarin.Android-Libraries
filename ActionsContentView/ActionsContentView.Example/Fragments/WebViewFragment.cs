using Android.OS;
using Android.Text;
using Android.Views;
using Android.Webkit;
using Android.Widget;

using Fragment = Android.Support.V4.App.Fragment;

namespace ActionsContentView.Example
{
    public class WebViewFragment : Fragment
    {
        public static readonly string TAG = Java.Lang.Class.FromType(typeof(WebViewFragment)).SimpleName;

        private WebView viewContentWebView;
        private string Url_Renamed;

        private bool ResetHistory = true;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.webview, container, false);

            ProgressBar viewContentProgress = v.FindViewById<ProgressBar>(Resource.Id.progress);
            viewContentWebView = v.FindViewById<WebView>(Resource.Id.webview);
            viewContentWebView.Settings.JavaScriptEnabled = true;
            viewContentWebView.SetWebViewClient(new MyWebViewClient());
            viewContentWebView.SetWebChromeClient(new MyWebChromeClient(this, viewContentProgress));

            return v;
        }

        private class MyWebViewClient : WebViewClient
        {
            public override bool ShouldOverrideUrlLoading(WebView view, string url)
            {
                return base.ShouldOverrideUrlLoading(view, url);
            }
        }

        private class MyWebChromeClient : WebChromeClient
        {
            private readonly WebViewFragment fragment;

            private ProgressBar ViewContentProgress;

            public MyWebChromeClient(WebViewFragment fragment, ProgressBar viewContentProgress)
            {
                this.fragment = fragment;
                this.ViewContentProgress = viewContentProgress;
            }

            public override void OnProgressChanged(WebView view, int newProgress)
            {
                ViewContentProgress.Progress = newProgress;
                ViewContentProgress.Visibility = newProgress == 100 ? ViewStates.Gone : ViewStates.Visible;

                if (newProgress == 100 && fragment.ResetHistory)
                {
                    fragment.viewContentWebView.ClearHistory();
                    fragment.ResetHistory = false;
                }
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Reload();
        }

        public override void OnHiddenChanged(bool hidden)
        {
            base.OnHiddenChanged(hidden);

            if (hidden)
            {
                viewContentWebView.StopLoading();
            }
            else
            {
                Reload();
            }
        }

        public virtual string Url
        {
            set
            {
                this.Url_Renamed = value;

                if (viewContentWebView != null)
                {
                    viewContentWebView.StopLoading();
                }

                ResetHistory = true;
            }
        }

        public virtual void Reload()
        {
            if (TextUtils.IsEmpty(Url_Renamed))
            {
                return;
            }

            viewContentWebView.LoadUrl(Url_Renamed);
        }

        public virtual bool OnBackPressed()
        {
            if (viewContentWebView.CanGoBack())
            {
                viewContentWebView.GoBack();

                return true;
            }
            return false;
        }
    }
}