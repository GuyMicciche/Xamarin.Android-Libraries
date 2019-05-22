using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Text;
using Java.Util;
using System;
using System.Collections.Generic;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Dialog with a date picker.
	/// <p/>
	/// Implement <seealso cref="IDateDialogListener"/>
	/// and/or <seealso cref="ISimpleDialogCancelListener"/> to handle events.
	/// </summary>
	public class DatePickerDialogFragment : BaseDialogFragment
	{
		protected internal const string ARG_ZONE = "zone";
		protected internal const string ARG_TITLE = "title";
		protected internal const string ARG_POSITIVE_BUTTON = "positive_button";
		protected internal const string ARG_NEGATIVE_BUTTON = "negative_button";
		protected internal const string ARG_DATE = "date";
		protected internal const string ARG_24H = "24h";

		internal DatePicker mDatePicker;
        internal Calendar mCalendar;

		public static SimpleDialogBuilder CreateBuilder(Context context, FragmentManager fragmentManager)
		{
			return new SimpleDialogBuilder(context, fragmentManager, Java.Lang.Class.FromType(typeof(DatePickerDialogFragment)));
		}

		/// <summary>
		/// Get dialog date listeners.
		/// There might be more than one date listener.
		/// </summary>
		/// <returns> Dialog date listeners
		/// @since 2.1.0 </returns>
        //protected internal virtual IList<IDateDialogListener> DialogListeners
        //{
        //    get
        //    {
        //        return GetDialogListeners<IDateDialogListener>(typeof(IDateDialogListener));
        //    }
        //}
        protected internal virtual System.Collections.IList DialogListeners
        {
            get
            {
                Type listenerInterface = typeof(IDateDialogListener);
                Fragment targetFragment = TargetFragment;
                System.Collections.IList listeners = new List<IDateDialogListener>(2);
                if (targetFragment != null && listenerInterface.IsAssignableFrom(targetFragment.GetType()))
                {
                    listeners.Add((IDateDialogListener)targetFragment);
                }
                if (Activity != null && listenerInterface.IsAssignableFrom(Activity.GetType()))
                {
                    listeners.Add((IDateDialogListener)Activity);
                }

                return Collections.UnmodifiableList(listeners);
            }
        }

		protected override BaseDialogFragment.Builder Build(BaseDialogFragment.Builder builder)
		{
			string title = Title;
			if (!TextUtils.IsEmpty(title))
			{
				builder.SetTitle(title);
			}

            string positiveButtonText = PositiveButtonText;
			if (!TextUtils.IsEmpty(positiveButtonText))
			{
                builder.SetPositiveButton(positiveButtonText, new PositiveButtonClickListener(this));
			}

            string negativeButtonText = NegativeButtonText;
			if (!TextUtils.IsEmpty(negativeButtonText))
			{
				builder.SetNegativeButton(negativeButtonText, new NegativeButtonClickListener(this));
			}
			mDatePicker = (DatePicker) LayoutInflater.From(Activity).Inflate(Resource.Layout.sdl_datepicker, null);
			builder.SetView(mDatePicker);

            Java.Util.TimeZone zone = Java.Util.TimeZone.GetTimeZone(Arguments.GetString(ARG_ZONE));
            mCalendar = Calendar.GetInstance(zone);
			mCalendar.TimeInMillis = Arguments.GetLong(ARG_DATE, JavaSystem.CurrentTimeMillis());
            mDatePicker.UpdateDate(mCalendar.Get(CalendarField.Year), mCalendar.Get(CalendarField.Month), mCalendar.Get(CalendarField.Date));

			return builder;
		}

		private class PositiveButtonClickListener : Java.Lang.Object, View.IOnClickListener
		{
			private readonly DatePickerDialogFragment fragment;

            public PositiveButtonClickListener(DatePickerDialogFragment fragment)
			{
                this.fragment = fragment;
			}

			public void OnClick(View view)
			{
                foreach (IDateDialogListener listener in fragment.DialogListeners)
				{
                    listener.OnPositiveButtonClicked(fragment.mRequestCode, fragment.Date);
				}
                fragment.Dismiss();
			}
		}

        private class NegativeButtonClickListener : Java.Lang.Object, View.IOnClickListener
		{
            private readonly DatePickerDialogFragment fragment;

            public NegativeButtonClickListener(DatePickerDialogFragment fragment)
			{
                this.fragment = fragment;
			}

			public void OnClick(View view)
			{
                foreach (IDateDialogListener listener in fragment.DialogListeners)
				{
                    listener.OnNegativeButtonClicked(fragment.mRequestCode, fragment.Date);
				}
                fragment.Dismiss();
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

        public virtual Date Date
		{
			get
			{
                mCalendar.Set(CalendarField.Year, mDatePicker.Year);
                mCalendar.Set(CalendarField.Month, mDatePicker.Month);
                mCalendar.Set(CalendarField.DayOfMonth, mDatePicker.DayOfMonth);

				return mCalendar.Time;
			}
		}

		public class SimpleDialogBuilder : BaseDialogBuilder<SimpleDialogBuilder>
		{
            internal Date mDate = new Date();
			internal string mTimeZone = null;

            internal string mTitle;
            internal string mPositiveButtonText;
            internal string mNegativeButtonText;

			internal bool mShowDefaultButton = true;
			internal bool m24h;

			public SimpleDialogBuilder(Context context, FragmentManager fragmentManager, Class clazz)
                : base(context, fragmentManager, clazz)
			{
				m24h = Android.Text.Format.DateFormat.Is24HourFormat(context);
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

			public virtual SimpleDialogBuilder SetPositiveButtonText(int textResourceId)
			{
                mPositiveButtonText = mContext.GetString(textResourceId);
				return this;
			}

			public virtual SimpleDialogBuilder SetPositiveButtonText(string text)
			{
				mPositiveButtonText = text;
				return this;
			}

			public virtual SimpleDialogBuilder SetNegativeButtonText(int textResourceId)
			{
                mNegativeButtonText = mContext.GetString(textResourceId);
				return this;
			}

			public virtual SimpleDialogBuilder SetNegativeButtonText(string text)
			{
				mNegativeButtonText = text;
				return this;
			}

            public virtual SimpleDialogBuilder SetDate(Date date)
			{
				mDate = date;
				return this;
			}

			public virtual SimpleDialogBuilder SetTimeZone(string zone)
			{
				mTimeZone = zone;
				return this;
			}

			public virtual SimpleDialogBuilder Set24hour(bool state)
			{
				m24h = state;
				return this;
			}

			protected override Bundle PrepareArguments()
			{
				Bundle args = new Bundle();
				args.PutCharSequence(SimpleDialogFragment.ARG_TITLE, mTitle);
				args.PutCharSequence(SimpleDialogFragment.ARG_POSITIVE_BUTTON, mPositiveButtonText);
				args.PutCharSequence(SimpleDialogFragment.ARG_NEGATIVE_BUTTON, mNegativeButtonText);

				args.PutLong(ARG_DATE, mDate.Time);
				args.PutBoolean(ARG_24H, m24h);
				if (mTimeZone != null)
				{
					args.PutString(ARG_ZONE, mTimeZone);
				}
				else
				{
					args.PutString(ARG_ZONE, Java.Util.TimeZone.Default.ID);
				}
				return args;
			}

			protected override SimpleDialogBuilder Self()
			{
				return this;
			}
		}
	}
}