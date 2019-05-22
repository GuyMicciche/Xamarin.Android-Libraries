using Android.Content;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;

namespace AndroidStyledDialogs
{
	/// <summary>
	/// Dialog with a time picker.
	/// <p/>
	/// Implement <seealso cref="com.avast.android.dialogs.iface.IDateDialogListener"/>
	/// and/or <seealso cref="com.avast.android.dialogs.iface.ISimpleDialogCancelListener"/> to handle events.
	/// </summary>
	public class TimePickerDialogFragment : DatePickerDialogFragment
	{
		internal TimePicker mTimePicker;
        internal new Calendar mCalendar;

		public static SimpleDialogBuilder CreateBuilder(Context context, FragmentManager fragmentManager)
		{
			return new SimpleDialogBuilder(context, fragmentManager, Java.Lang.Class.FromType(typeof(TimePickerDialogFragment)));
		}

		protected override BaseDialogFragment.Builder Build(BaseDialogFragment.Builder builder)
		{   
			builder = base.Build(builder);
			mTimePicker = (TimePicker)LayoutInflater.From(Activity).Inflate(Resource.Layout.sdl_timepicker, null);
            mTimePicker.SetIs24HourView((Java.Lang.Boolean)Arguments.GetBoolean(ARG_24H));
			builder.SetView(mTimePicker);

			TimeZone zone = TimeZone.GetTimeZone(Arguments.GetString(ARG_ZONE));
            mCalendar = Calendar.GetInstance(zone);
			mCalendar.TimeInMillis = Arguments.GetLong(ARG_DATE, JavaSystem.CurrentTimeMillis());

            mTimePicker.CurrentHour = (Java.Lang.Integer)mCalendar.Get(CalendarField.HourOfDay);
            mTimePicker.CurrentMinute = (Java.Lang.Integer)mCalendar.Get(CalendarField.Minute);
			return builder;
		}

		public override Date Date
		{
			get
			{
                mCalendar.Set(CalendarField.HourOfDay, (int)mTimePicker.CurrentHour);
                mCalendar.Set(CalendarField.Minute, (int)mTimePicker.CurrentMinute);
				return mCalendar.Time;
			}
		}
	}
}