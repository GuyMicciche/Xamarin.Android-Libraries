using Android.Content;
using Android.OS;
using Android.Webkit;
using Java.Interop;
using Java.Lang;
using System;
namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// This javascript interface allows the page to communicate that text has been selected by the user.
	/// </summary>
    public class TextSelectionJavascriptInterface : Java.Lang.Object
    {
        /// <summary>
        /// The TAG for logging. </summary>
        private const string TAG = "TextSelectionJavascriptInterface";

        /// <summary>
        /// The javascript interface name for adding to web view. </summary>
        private readonly string interfaceName = "TextSelection";

        /// <summary>
        /// The webview to work with. </summary>
        private ITextSelectionJavascriptInterfaceListener mListener;

        /// <summary>
        /// The context. </summary>
        private Context mContext;

        // Need handler for callbacks to the UI thread
        private Handler mHandler = new Handler();

        /// <summary>
        /// Constructor accepting context. </summary>
        /// <param name="c"> </param>
        public TextSelectionJavascriptInterface(Context c)
        {
            this.mContext = c;
        }

        /// <summary>
        /// Constructor accepting context and mListener. </summary>
        /// <param name="c"> </param>
        /// <param name="mListener"> </param>
        public TextSelectionJavascriptInterface(Context c, ITextSelectionJavascriptInterfaceListener mListener)
        {
            this.mContext = c;
            this.mListener = mListener;
        }

        /// <summary>
        /// Handles javascript errors. </summary>
        /// <param name="error"> </param>
        [JavascriptInterface]
        [Export("JsError")]
        public void JsError(string error)
        {
            if (this.mListener != null)
            {
                mHandler.Post(() => { mListener.tsjiJSError(error); });
            }
        }

        /// <summary>
        /// Gets the interface name
        /// @return
        /// </summary>
        public string InterfaceName
        {
            get
            {
                return this.interfaceName;
            }
        }

        /// <summary>
        /// Put the app in "selection mode".
        /// </summary>
        [JavascriptInterface]
        [Export("StartSelectionMode")]
        public void StartSelectionMode()
        {
            if (this.mListener != null)
            {
                mHandler.Post(() => { mListener.tsjiStartSelectionMode(); });
            }
        }

        /// <summary>
        /// Take the app out of "selection mode".
        /// </summary>
        [JavascriptInterface]
        [Export("EndSelectionMode")]
        public void EndSelectionMode()
        {

            if (this.mListener != null)
            {
                mHandler.Post(() => { mListener.tsjiEndSelectionMode(); });
            }
        }

        /// <summary>
        /// Show the context menu </summary>
        /// <param name="range"> </param>
        /// <param name="text"> </param>
        /// <param name="menuBounds"> </param>
        [JavascriptInterface]
        [Export("SelectionChanged")]
        public void SelectionChanged(string range, string text, string handleBounds, string menuBounds)
        {
            Console.WriteLine("BTSelectionWebView handleBounds: " + handleBounds);
            Console.WriteLine("BTSelectionWebView menuBounds: " + menuBounds);
            if (this.mListener != null)
            {
                mHandler.Post(() => { mListener.tsjiSelectionChanged(range, text, handleBounds, menuBounds); });
            }
            else
            {
                Console.WriteLine("BTSelectionWebView mListener null");
            }

        }

        [JavascriptInterface]
        [Export("SetContentWidth")]
        public void SetContentWidth(float contentWidth)
        {
            if (this.mListener != null)
            {
                mHandler.Post(() => { mListener.tsjiSetContentWidth(contentWidth); });
            }
        }
    }
}