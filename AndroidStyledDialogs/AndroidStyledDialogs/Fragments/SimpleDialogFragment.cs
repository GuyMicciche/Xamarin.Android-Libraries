using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Text;
using Android.Views;
using Java.Interop;
using Java.Lang;
using Java.Util;
using System;
using System.Collections.Generic;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Dialog for displaying simple message, message with title or message with title and two buttons. Implement {@link
	/// com.avast.android.dialogs.iface.ISimpleDialogListener} in your Fragment or Activity to rect on positive and negative button clicks. This class can
	/// be extended and more parameters can be added in overridden build() method.
	/// </summary>
	public class SimpleDialogFragment : BaseDialogFragment
	{
		protected internal const string ARG_MESSAGE = "message";
		protected internal const string ARG_TITLE = "title";

		protected internal const string ARG_POSITIVE_BUTTON = "positive_button";
		protected internal const string ARG_NEGATIVE_BUTTON = "negative_button";
        protected internal const string ARG_NEUTRAL_BUTTON = "neutral_button";

        public SimpleDialogFragment()
        {

        }

		public static SimpleDialogBuilder CreateBuilder(Context context, FragmentManager fragmentManager)
		{
			return new SimpleDialogBuilder(context, fragmentManager, Java.Lang.Class.FromType(typeof(SimpleDialogFragment)));
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);

		}

		/// <summary>
		/// Key method for extending <seealso cref="com.avast.android.dialogs.fragment.SimpleDialogFragment"/>.
		/// Children can extend this to add more things to base builder.
		/// </summary>
		protected override BaseDialogFragment.Builder Build(BaseDialogFragment.Builder builder)
		{
            string title = Title;
			if (!TextUtils.IsEmpty(title))
			{
				builder.SetTitle(title);
			}

            string message = Message;
            if (!TextUtils.IsEmpty(message))
			{
				builder.SetMessage(message);
			}

            string positiveButtonText = PositiveButtonText;
            if (!TextUtils.IsEmpty(positiveButtonText))
			{
                builder.SetPositiveButton(positiveButtonText, (sender, e) =>
                {
                    foreach (IPositiveButtonDialogListener listener in PositiveButtonDialogListeners)
                    {
                        listener.OnPositiveButtonClicked(mRequestCode);
                    }
                    Dismiss();
                });
			}

            string negativeButtonText = NegativeButtonText;
            if (!TextUtils.IsEmpty(negativeButtonText))
			{
                builder.SetNegativeButton(negativeButtonText, (sender, e) =>
                {
                    foreach (INegativeButtonDialogListener listener in NegativeButtonDialogListeners)
                    {
                        listener.OnNegativeButtonClicked(mRequestCode);
                    }
                    Dismiss();
                });
			}

            string neutralButtonText = NeutralButtonText;
            if (!TextUtils.IsEmpty(neutralButtonText))
			{
                builder.SetNeutralButton(neutralButtonText, (sender, e) =>
                {
                    foreach (INeutralButtonDialogListener listener in NeutralButtonDialogListeners)
                    {
                        listener.OnNeutralButtonClicked(mRequestCode);
                    }
                    Dismiss();
                });
			}

			return builder;
		}

		protected internal virtual string Message
		{
			get
			{
                return Arguments.GetString(ARG_MESSAGE);
			}
		}

        protected internal virtual string Title
		{
			get
			{
                return Arguments.GetString(ARG_TITLE);
			}
		}

        protected internal virtual string PositiveButtonText
		{
			get
			{
                return Arguments.GetString(ARG_POSITIVE_BUTTON);
			}
		}

        protected internal virtual string NegativeButtonText
		{
			get
			{
                return Arguments.GetString(ARG_NEGATIVE_BUTTON);
			}
		}

        protected internal virtual string NeutralButtonText
		{
			get
			{
                return Arguments.GetString(ARG_NEUTRAL_BUTTON);
			}
		}

		/// <summary>
		/// Get positive button dialog listeners.
		/// There might be more than one listener.
		/// </summary>
		/// <returns> Dialog listeners
		/// @since 2.1.0 </returns>
        //protected internal virtual IList<IPositiveButtonDialogListener> PositiveButtonDialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<IPositiveButtonDialogListener>(typeof(IPositiveButtonDialogListener));
        //    }
        //}
        protected internal virtual System.Collections.IList PositiveButtonDialogListeners
        {
            get
            {
                Type listenerInterface = typeof(IPositiveButtonDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<IPositiveButtonDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((IPositiveButtonDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((IPositiveButtonDialogListener)Activity);
                }

                return Collections.UnmodifiableList(listeners);
            }
        }

		/// <summary>
		/// Get negative button dialog listeners.
		/// There might be more than one listener.
		/// </summary>
		/// <returns> Dialog listeners
		/// @since 2.1.0 </returns>
        //protected internal virtual IList<INegativeButtonDialogListener> NegativeButtonDialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<INegativeButtonDialogListener>(typeof(INegativeButtonDialogListener));
        //    }
        //}
        protected internal virtual System.Collections.IList NegativeButtonDialogListeners
        {
            get
            {
                Type listenerInterface = typeof(INegativeButtonDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<INegativeButtonDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((INegativeButtonDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((INegativeButtonDialogListener)Activity);
                }

                return Collections.UnmodifiableList(listeners);
            }
        }

		/// <summary>
		/// Get neutral button dialog listeners.
		/// There might be more than one listener.
		/// </summary>
		/// <returns> Dialog listeners
		/// @since 2.1.0 </returns>
        //protected internal virtual IList<INeutralButtonDialogListener> NeutralButtonDialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<INeutralButtonDialogListener>(typeof(INeutralButtonDialogListener));
        //    }
        //}
        protected internal virtual System.Collections.IList NeutralButtonDialogListeners
        {
            get
            {
                Type listenerInterface = typeof(INeutralButtonDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<INeutralButtonDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((INeutralButtonDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((INeutralButtonDialogListener)Activity);
                }

                return Collections.UnmodifiableList(listeners);
            }
        }

		public class SimpleDialogBuilder : BaseDialogBuilder<SimpleDialogBuilder>
		{
			internal string mTitle;
            internal string mMessage;
            internal string mPositiveButtonText;
            internal string mNegativeButtonText;
            internal string mNeutralButtonText;
            internal EventHandler mPositiveButtonHandler;
            internal EventHandler mNegativeButtonHandler;
            internal EventHandler mNeutralButtonHandler;

			public SimpleDialogBuilder(Context context, FragmentManager fragmentManager, Class clazz)
                : base(context, fragmentManager, clazz)
			{

			}

			protected override SimpleDialogBuilder Self()
			{
				return this;
			}

			public virtual SimpleDialogBuilder SetTitle(int titleResourceId)
			{
				mTitle = mContext.GetString(titleResourceId);
				return this;
			}

            public virtual SimpleDialogBuilder SetTitle(string title)
            {
                mTitle = title;
                return this;
            }

			public virtual SimpleDialogBuilder SetMessage(int messageResourceId)
			{
                mMessage = mContext.GetText(messageResourceId);
				return this;
			}

			/// <summary>
			/// Allow to set resource string with HTML formatting and bind %s,%i.
			/// This is workaround for https://code.google.com/p/android/issues/detail?id=2923
			/// </summary>
			public virtual SimpleDialogBuilder SetMessage(int resourceId, params object[] formatArgs)
			{
				mMessage = Html.FromHtml(string.Format(Html.ToHtml(new SpannedString(mContext.GetText(resourceId))), formatArgs)).ToString();
				return this;
			}

            public virtual SimpleDialogBuilder SetMessage(string message)
            {
                mMessage = message;
                return this;
            }

			public virtual SimpleDialogBuilder SetPositiveButtonText(int textResourceId)
			{
				mPositiveButtonText = mContext.GetText(textResourceId);
				return this;
			}

            public virtual SimpleDialogBuilder SetPositiveButtonText(string text)
            {
                mPositiveButtonText = text;
                return this;
            }

            public virtual SimpleDialogBuilder SetPositiveButtonText(string text, EventHandler handler)
            {
                mPositiveButtonText = text;
                mPositiveButtonHandler = handler;
                return this;
            }

			public virtual SimpleDialogBuilder SetNegativeButtonText(int textResourceId)
			{
                mNegativeButtonText = mContext.GetText(textResourceId);
				return this;
			}

            public virtual SimpleDialogBuilder SetNegativeButtonText(string text)
            {
                mNegativeButtonText = text;
                return this;
            }

            public virtual SimpleDialogBuilder SetNegativeButtonText(string text, EventHandler handler)
            {
                mPositiveButtonText = text;
                mNegativeButtonHandler = handler;
                return this;
            }

			public virtual SimpleDialogBuilder SetNeutralButtonText(int textResourceId)
			{
                mNeutralButtonText = mContext.GetText(textResourceId);
				return this;
			}

            public virtual SimpleDialogBuilder SetNeutralButtonText(string text)
            {
                mNeutralButtonText = text;
                return this;
            }

            public virtual SimpleDialogBuilder SetNeutralButtonText(string text, EventHandler handler)
            {
                mPositiveButtonText = text;
                mNeutralButtonHandler = handler;
                return this;
            }

			protected override Bundle PrepareArguments()
			{
				Bundle args = new Bundle();
				args.PutString(SimpleDialogFragment.ARG_MESSAGE, mMessage);
                args.PutString(SimpleDialogFragment.ARG_TITLE, mTitle);
                args.PutString(SimpleDialogFragment.ARG_POSITIVE_BUTTON, mPositiveButtonText);
                args.PutString(SimpleDialogFragment.ARG_NEGATIVE_BUTTON, mNegativeButtonText);
                args.PutString(SimpleDialogFragment.ARG_NEUTRAL_BUTTON, mNeutralButtonText);

				return args;
			}
		}
	}
}