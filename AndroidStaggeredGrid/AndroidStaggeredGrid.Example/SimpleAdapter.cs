using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidStaggeredGrid.Util;
using System;
using System.Collections.Generic;

namespace AndroidStaggeredGrid.Example
{
    public class SampleAdapter : ArrayAdapter<string>
    {

        private const string TAG = "SampleAdapter";

        public class ViewHolder : Java.Lang.Object
        {
            public DynamicHeightTextView TxtLineOne;
            public Button btnGo;
        }

        private readonly LayoutInflater MLayoutInflater;
        private readonly Random MRandom;
        private readonly List<int> MBackgroundColors;

        private static readonly SparseArray<double> SPositionHeightRatios = new SparseArray<double>();

        public SampleAdapter(Context context, int textViewResourceId)
            : base(context, textViewResourceId)
        {
            MLayoutInflater = LayoutInflater.From(context);
            MRandom = new Random();
            MBackgroundColors = new List<int>();
            MBackgroundColors.Add(Resource.Color.orange);
            MBackgroundColors.Add(Resource.Color.green);
            MBackgroundColors.Add(Resource.Color.blue);
            MBackgroundColors.Add(Resource.Color.yellow);
            MBackgroundColors.Add(Resource.Color.grey);
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {

            ViewHolder vh;
            if (convertView == null)
            {
                convertView = MLayoutInflater.Inflate(Resource.Layout.list_item_sample, parent, false);
                vh = new ViewHolder();
                vh.TxtLineOne = (DynamicHeightTextView)convertView.FindViewById(Resource.Id.txt_line1);
                vh.btnGo = (Button)convertView.FindViewById(Resource.Id.btn_go);

                convertView.Tag = vh;
            }
            else
            {
                vh = (ViewHolder)convertView.Tag;
            }

            double positionHeight = GetPositionRatio(position);
            int backgroundIndex = position >= MBackgroundColors.Count ? position % MBackgroundColors.Count : position;

            convertView.SetBackgroundResource(MBackgroundColors[backgroundIndex]);

            Console.WriteLine(TAG, "getView position:" + position + " h:" + positionHeight);

            vh.TxtLineOne.HeightRatio = positionHeight;
            vh.TxtLineOne.Text = GetItem(position) + position;

            vh.btnGo.Click += delegate(object sender, EventArgs e)
            {
                Toast.MakeText(Context, "Button Clicked Position " + position, ToastLength.Short).Show();
            };

            return convertView;
        }

        private double GetPositionRatio(int position)
        {
            double ratio = SPositionHeightRatios.Get(position, 0.0);
            // if not yet done generate and stash the columns height
            // in our real world scenario this will be determined by
            // some match based on the known height and width of the image
            // and maybe a helpful way to get the column height!
            if (ratio == 0)
            {
                ratio = RandomHeightRatio;
                SPositionHeightRatios.Append(position, ratio);
                Console.WriteLine(TAG, "getPositionRatio:" + position + " ratio:" + ratio);
            }
            return ratio;
        }

        private double RandomHeightRatio
        {
            get
            {
                return (MRandom.NextDouble() / 2.0) + 1.0; // height will be 1.0 - 1.5 the width
            }
        }
    }
}