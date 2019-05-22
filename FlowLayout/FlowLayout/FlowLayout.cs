using Android.Annotation;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;

namespace FlowLayout
{
    /// <summary>
    /// FlowLayout will arrange child elements horizontally one next to another. If there is not enough
    /// space for next view new line will be added.
    /// 
    /// User: Guy Micciche
    /// Date: 13 March 2015
    /// Time: 4:27 PM
    /// </summary>
    [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
    public class FlowLayout : ViewGroup
    {
        private GravityFlags mGravity = (Ics ? GravityFlags.Start : GravityFlags.Left) | GravityFlags.Top;

        private readonly IList<IList<View>> mLines = new List<IList<View>>();
        private readonly IList<int> mLineHeights = new List<int>();
        private readonly IList<int> mLineMargins = new List<int>();

        public FlowLayout(Context context)
            : this(context, null)
        {
        }

        public FlowLayout(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public FlowLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.FlowLayout, defStyle, 0);

            try
            {
                int index = a.GetInt(Resource.Styleable.FlowLayout_android_gravity, -1);
                if (index > 0)
                {
                    Gravity = (GravityFlags)index;
                }
            }
            finally
            {
                a.Recycle();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            int sizeWidth = MeasureSpec.GetSize(widthMeasureSpec) - PaddingLeft - PaddingRight;
            int sizeHeight = MeasureSpec.GetSize(heightMeasureSpec);

            MeasureSpecMode modeWidth = MeasureSpec.GetMode(widthMeasureSpec);
            MeasureSpecMode modeHeight = MeasureSpec.GetMode(heightMeasureSpec);

            int width = 0;
            int height = PaddingTop + PaddingBottom;

            int lineWidth = 0;
            int lineHeight = 0;

            int childCount = ChildCount;

            for (int i = 0; i < childCount; i++)
            {

                View child = GetChildAt(i);
                bool lastChild = i == childCount - 1;

                if (child.Visibility == ViewStates.Gone)
                {

                    if (lastChild)
                    {
                        width = Math.Max(width, lineWidth);
                        height += lineHeight;
                    }

                    continue;
                }

                MeasureChildWithMargins(child, widthMeasureSpec, lineWidth, heightMeasureSpec, height);

                CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;

                MeasureSpecMode childWidthMode = MeasureSpecMode.AtMost;
                int childWidthSize = sizeWidth;

                MeasureSpecMode childHeightMode = MeasureSpecMode.AtMost;
                int childHeightSize = sizeHeight;

                if (lp.Width == LayoutParams.MatchParent)
                {
                    childWidthMode = MeasureSpecMode.Exactly;
                    childWidthSize -= lp.LeftMargin + lp.RightMargin;
                }
                else if (lp.Width >= 0)
                {
                    childWidthMode = MeasureSpecMode.Exactly;
                    childWidthSize = lp.Width;
                }

                if (lp.Height >= 0)
                {
                    childHeightMode = MeasureSpecMode.Exactly;
                    childHeightSize = lp.Height;
                }
                else if (modeHeight == MeasureSpecMode.Unspecified)
                {
                    childHeightMode = MeasureSpecMode.Unspecified;
                    childHeightSize = 0;
                }

                child.Measure(MeasureSpec.MakeMeasureSpec(childWidthSize, childWidthMode), MeasureSpec.MakeMeasureSpec(childHeightSize, childHeightMode));

                int childWidth = child.MeasuredWidth + lp.LeftMargin + lp.RightMargin;

                if (lineWidth + childWidth > sizeWidth)
                {

                    width = Math.Max(width, lineWidth);
                    lineWidth = childWidth;

                    height += lineHeight;
                    lineHeight = child.MeasuredHeight + lp.TopMargin + lp.BottomMargin;

                }
                else
                {
                    lineWidth += childWidth;
                    lineHeight = Math.Max(lineHeight, child.MeasuredHeight + lp.TopMargin + lp.BottomMargin);
                }

                if (lastChild)
                {
                    width = Math.Max(width, lineWidth);
                    height += lineHeight;
                }

            }

            width += PaddingLeft + PaddingRight;

            SetMeasuredDimension((modeWidth == MeasureSpecMode.Exactly) ? sizeWidth : width, (modeHeight == MeasureSpecMode.Exactly) ? sizeHeight : height);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            mLines.Clear();
            mLineHeights.Clear();
            mLineMargins.Clear();

            int width = Width;
            int height = Height;

            int linesSum = PaddingTop;

            int lineWidth = 0;
            int lineHeight = 0;
            IList<View> lineViews = new List<View>();

            float horizontalGravityFactor;
            switch ((mGravity & GravityFlags.HorizontalGravityMask))
            {
                case GravityFlags.Left:
                default:
                    horizontalGravityFactor = 0;
                    break;
                case GravityFlags.CenterHorizontal:
                    horizontalGravityFactor = .5f;
                    break;
                case GravityFlags.Right:
                    horizontalGravityFactor = 1;
                    break;
            }

            for (int i = 0; i < ChildCount; i++)
            {
                View child = GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;

                int childWidth = child.MeasuredWidth + lp.LeftMargin + lp.RightMargin;
                int childHeight = child.MeasuredHeight + lp.BottomMargin + lp.TopMargin;

                if (lineWidth + childWidth > width)
                {
                    mLineHeights.Add(lineHeight);
                    mLines.Add(lineViews);
                    mLineMargins.Add((int)((width - lineWidth) * horizontalGravityFactor) + PaddingLeft);

                    linesSum += lineHeight;

                    lineHeight = 0;
                    lineWidth = 0;
                    lineViews = new List<View>();
                }

                lineWidth += childWidth;
                lineHeight = Math.Max(lineHeight, childHeight);
                lineViews.Add(child);
            }

            mLineHeights.Add(lineHeight);
            mLines.Add(lineViews);
            mLineMargins.Add((int)((width - lineWidth) * horizontalGravityFactor) + PaddingLeft);

            linesSum += lineHeight;

            int verticalGravityMargin = 0;
            switch ((mGravity & GravityFlags.VerticalGravityMask))
            {
                case GravityFlags.Top:
                default:
                    break;
                case GravityFlags.CenterVertical:
                    verticalGravityMargin = (height - linesSum) / 2;
                    break;
                case GravityFlags.Bottom:
                    verticalGravityMargin = height - linesSum;
                    break;
            }

            int numLines = mLines.Count;

            int left;
            int top = PaddingTop;

            for (int i = 0; i < numLines; i++)
            {

                lineHeight = mLineHeights[i];
                lineViews = mLines[i];
                left = mLineMargins[i];

                int children = lineViews.Count;

                for (int j = 0; j < children; j++)
                {

                    View child = lineViews[j];

                    if (child.Visibility == ViewStates.Gone)
                    {
                        continue;
                    }

                    CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;

                    // if height is match_parent we need to remeasure child to line height
                    if (lp.Height == LayoutParams.MatchParent)
                    {
                        MeasureSpecMode childWidthMode = MeasureSpecMode.AtMost;
                        int childWidthSize = lineWidth;

                        if (lp.Width == LayoutParams.MatchParent)
                        {
                            childWidthMode = MeasureSpecMode.Exactly;
                        }
                        else if (lp.Width >= 0)
                        {
                            childWidthMode = MeasureSpecMode.Exactly;
                            childWidthSize = lp.Width;
                        }

                        child.Measure(MeasureSpec.MakeMeasureSpec(childWidthSize, childWidthMode), MeasureSpec.MakeMeasureSpec(lineHeight - lp.TopMargin - lp.BottomMargin, MeasureSpecMode.Exactly));
                    }

                    int childWidth = child.MeasuredWidth;
                    int childHeight = child.MeasuredHeight;

                    int gravityMargin = 0;

                    if (Android.Views.Gravity.IsVertical(lp.gravity))
                    {
                        switch (lp.gravity)
                        {
                            case GravityFlags.Top:
                            default:
                                break;
                            case GravityFlags.CenterVertical:
                            case GravityFlags.Center:
                                gravityMargin = (lineHeight - childHeight - lp.TopMargin - lp.BottomMargin) / 2;
                                break;
                            case GravityFlags.Bottom:
                                gravityMargin = lineHeight - childHeight - lp.TopMargin - lp.BottomMargin;
                                break;
                        }
                    }

                    child.Layout(left + lp.LeftMargin, top + lp.TopMargin + gravityMargin + verticalGravityMargin, left + childWidth + lp.LeftMargin, top + childHeight + lp.TopMargin + gravityMargin + verticalGravityMargin);

                    left += childWidth + lp.LeftMargin + lp.RightMargin;
                }

                top += lineHeight;
            }
        }

        protected override LayoutParams GenerateLayoutParams(LayoutParams p)
        {
            return new CustomLayoutParams(p);
        }

        public override LayoutParams GenerateLayoutParams(IAttributeSet attrs)
        {
            return new CustomLayoutParams(Context, attrs);
        }

        protected override LayoutParams GenerateDefaultLayoutParams()
        {
            return new CustomLayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
        }

        [TargetApi(Value = (int)BuildVersionCodes.IceCreamSandwich)]
        public GravityFlags Gravity
        {
            set
            {
                if (mGravity != value)
                {
                    if ((value & GravityFlags.RelativeHorizontalGravityMask) == 0)
                    {
                        value |= Ics ? GravityFlags.Start : GravityFlags.Left;
                    }

                    if ((value & GravityFlags.VerticalGravityMask) == 0)
                    {
                        value |= GravityFlags.Top;
                    }

                    mGravity = value;
                    RequestLayout();
                }
            }
            get
            {
                return mGravity;
            }
        }


        /// <summary>
        /// <returns> <code>true</code> if device is running ICS or grater version of Android. </returns>
        /// </summary>
        private static bool Ics
        {
            get
            {
                return Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
            }
        }

        public class CustomLayoutParams : MarginLayoutParams
        {

            public GravityFlags gravity = (GravityFlags)(-1);

            public CustomLayoutParams(Context c, IAttributeSet attrs)
                : base(c, attrs)
            {

                TypedArray a = c.ObtainStyledAttributes(attrs, Resource.Styleable.FlowLayout_Layout);

                try
                {
                    gravity = (GravityFlags)a.GetInt(Resource.Styleable.FlowLayout_Layout_android_layout_gravity, -1);
                }
                finally
                {
                    a.Recycle();
                }
            }

            public CustomLayoutParams(int width, int height)
                : base(width, height)
            {
            }

            public CustomLayoutParams(ViewGroup.LayoutParams source)
                : base(source)
            {
            }
        }
    }
}