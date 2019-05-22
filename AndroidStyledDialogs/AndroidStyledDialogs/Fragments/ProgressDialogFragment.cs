using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Java.Lang;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Simple progress dialog that shows indeterminate progress bar together with message and dialog title (optional).<br/>
	/// <p>
	/// To show the dialog, start with <seealso cref="#createBuilder(android.content.Context, android.support.v4.app.FragmentManager)"/>.
	/// </p>
	/// <p>
	/// Dialog can be cancelable - to listen to cancellation, activity or target fragment must implement <seealso cref="com.avast.android.dialogs.iface.ISimpleDialogCancelListener"/>
	/// </p>
	/// 
	/// @author Tomas Vondracek
	/// </summary>
	public class ProgressDialogFragment : BaseDialogFragment
	{
		protected internal const string ARG_MESSAGE = "message";
		protected internal const string ARG_TITLE = "title";

		public static ProgressDialogBuilder CreateBuilder(Context context, FragmentManager fragmentManager)
		{
			return new ProgressDialogBuilder(context, fragmentManager);
		}

		protected override Builder Build(Builder builder)
		{
			LayoutInflater inflater = builder.LayoutInflater;
			View view = inflater.Inflate(Resource.Layout.sdl_progress, null, false);
			TextView tvMessage = (TextView) view.FindViewById(Resource.Id.sdl_message);

			tvMessage.Text = Arguments.GetCharSequence(ARG_MESSAGE);

			builder.SetView(view);

			builder.SetTitle(Arguments.GetCharSequence(ARG_TITLE));

			return builder;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			if (Arguments == null)
			{
				throw new System.ArgumentException("use ProgressDialogBuilder to construct this dialog");
			}
		}

		public class ProgressDialogBuilder : BaseDialogBuilder<ProgressDialogBuilder>
		{
			internal string mTitle;
            internal string mMessage;

			protected internal ProgressDialogBuilder(Context context, FragmentManager fragmentManager)
                : base(context, fragmentManager, Java.Lang.Class.FromType(typeof(ProgressDialogFragment)))
			{
			}

			protected override ProgressDialogBuilder Self()
			{
				return this;
			}

			public virtual ProgressDialogBuilder SetTitle(int titleResourceId)
			{
				mTitle = mContext.GetText(titleResourceId);
				return this;
			}

			public virtual ProgressDialogBuilder SetTitle(ICharSequence title)
			{
				mTitle = title.ToString();
				return this;
			}

            public virtual ProgressDialogBuilder SetTitle(string title)
            {
                mTitle = title;
                return this;
            }

			public virtual ProgressDialogBuilder SetMessage(int messageResourceId)
			{
                mMessage = mContext.GetText(messageResourceId);
				return this;
			}

            public virtual ProgressDialogBuilder SetMessage(string message)
            {
                mMessage = message;
                return this;
            }

			protected override Bundle PrepareArguments()
			{
				Bundle args = new Bundle();
				args.PutCharSequence(SimpleDialogFragment.ARG_MESSAGE, mMessage);
				args.PutCharSequence(SimpleDialogFragment.ARG_TITLE, mTitle);

				return args;
			}
		}
	}
}