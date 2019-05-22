using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;

namespace FlowLayout.Example
{
    [Activity(Label = "MainActivity", MainLauncher = true)]
	public class MainActivity : ListActivity
	{
		private ExamplesAdapter mAdapter;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			mAdapter = new ExamplesAdapter(this, this);
			ListAdapter = mAdapter;
		}

		protected override void OnListItemClick(ListView l, View v, int position, long id)
		{
            Intent intent = new Intent(this, mAdapter.GetClassItem(position).ActivityClass);
			StartActivity(intent);
		}

		private class ExamplesAdapter : ArrayAdapter
		{
			private MainActivity activity;
			public List<ClassItem> mItems = new List<ClassItem>();

			public LayoutInflater mInflater;
			public ExamplesAdapter(MainActivity activity, Context context) 
                : base(context, 0)
			{
				this.activity = activity;

                mItems = new List<ClassItem>();
                mItems.Add(new ClassItem(activity.GetString(Resource.String.activity_basic), typeof(BasicActivity)));
                mItems.Add(new ClassItem(activity.GetString(Resource.String.activity_all_in_one), typeof(AllInOneActivity)));
                mItems.Add(new ClassItem(activity.GetString(Resource.String.activity_scroll), typeof(ScrollActivity)));

                mInflater = LayoutInflater.From(activity);
			}

			public override int Count
			{
				get
				{
                    return mItems.Count;
				}
			}

            public ClassItem GetClassItem(int position)
            {
                return mItems[position];
            }

			public override Java.Lang.Object GetItem(int position)
			{
                return null;
			}

			public override View GetView(int position, View convertView, ViewGroup parent)
			{
				View v = convertView;
				if (v == null)
				{
					v = mInflater.Inflate(Android.Resource.Layout.SimpleListItem1, parent, false);
				}

				TextView text = (TextView)v.FindViewById(Android.Resource.Id.Text1);
                text.Text = GetClassItem(position).Name;

				return v;
			}

			public class ClassItem : Java.Lang.Object
			{
				public string Name;
                public Type ActivityClass;

                public ClassItem(string name, Type activityClass)
                    : base()
				{
                    this.Name = name;
                    this.ActivityClass = activityClass;
				}
			}
		}
	}
}