using Android.Content;
using Android.Util;
using Android.Views;
using System;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// A layout that lets you specify exact locations (x/y coordinates) of its
	/// children. Absolute layouts are less flexible and harder to maintain than
	/// other types of layouts without absolute positioning.
	/// 
	/// <p><strong>XML attributes</strong></p> <p> See {@link
	/// android.R.styleable#ViewGroup ViewGroup Attributes}, {@link
	/// android.R.styleable#View View Attributes}</p>
	/// 
	/// <p>Note: This class is a clone of AbsoluteLayout, which is now deprecated.
	/// </summary>
	public class MyAbsoluteLayout : ViewGroup
	{
		public MyAbsoluteLayout(Context context) 
            : base(context)
		{
		}

        public MyAbsoluteLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
		{
		}

        public MyAbsoluteLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
		{
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			int count = ChildCount;

			int maxHeight = 0;
			int maxWidth = 0;

			// Find out how big everyone wants to be
			MeasureChildren(widthMeasureSpec, heightMeasureSpec);

			// Find rightmost and bottom-most child
			for (int i = 0; i < count; i++)
			{
				View child = GetChildAt(i);
				if (child.Visibility != ViewStates.Gone)
				{
					int childRight;
					int childBottom;

					MyAbsoluteLayout.LayoutParams lp = (MyAbsoluteLayout.LayoutParams)child.LayoutParameters;

					childRight = lp.X + child.MeasuredWidth;
					childBottom = lp.Y + child.MeasuredHeight;

					maxWidth = Math.Max(maxWidth, childRight);
					maxHeight = Math.Max(maxHeight, childBottom);
				}
			}

			// Account for padding too
			maxWidth += PaddingLeft + PaddingRight;
			maxHeight += PaddingTop + PaddingBottom;
			/* original
			maxWidth += mPaddingLeft + mPaddingRight;
			maxHeight += mPaddingTop + mPaddingBottom;
			*/

			// Check against minimum height and width
			maxHeight = Math.Max(maxHeight, SuggestedMinimumHeight);
			maxWidth = Math.Max(maxWidth, SuggestedMinimumWidth);

			SetMeasuredDimension(ResolveSize(maxWidth, widthMeasureSpec), ResolveSize(maxHeight, heightMeasureSpec));
		}

		/// <summary>
		/// Returns a set of layout parameters with a width of
		/// <seealso cref="android.view.ViewGroup.LayoutParams#WRAP_CONTENT"/>,
		/// a height of <seealso cref="android.view.ViewGroup.LayoutParams#WRAP_CONTENT"/>
		/// and with the coordinates (0, 0).
		/// </summary>
		protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
		{
            return new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent, 0, 0);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			int count = ChildCount;

			int paddingL = PaddingLeft;
			int paddingT = PaddingTop;
			for (int i = 0; i < count; i++)
			{
				View child = GetChildAt(i);
				if (child.Visibility != ViewStates.Gone)
				{

					MyAbsoluteLayout.LayoutParams lp = (MyAbsoluteLayout.LayoutParams)child.LayoutParameters;

					int childLeft = paddingL + lp.X;
					int childTop = paddingT + lp.Y;
					/*
					int childLeft = mPaddingLeft + lp.x;
					int childTop = mPaddingTop + lp.y;
					*/
					child.Layout(childLeft, childTop, childLeft + child.MeasuredWidth, childTop + child.MeasuredHeight);

				}
			}
		}

		public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attrs)
		{
			return new MyAbsoluteLayout.LayoutParams(Context, attrs);
		}

		// Override to allow type-checking of LayoutParams. 
		protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
		{
			return p is MyAbsoluteLayout.LayoutParams;
		}

		protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
		{
			return new LayoutParams(p);
		}

		/// <summary>
		/// Per-child layout information associated with MyAbsoluteLayout.
		/// See
		/// <seealso cref="android.R.styleable#MyAbsoluteLayout_Layout Absolute Layout Attributes"/>
		/// for a list of all child view attributes that this class supports.
		/// </summary>
		public class LayoutParams : ViewGroup.LayoutParams
		{
			/// <summary>
			/// The horizontal, or X, location of the child within the view group.
			/// </summary>
			public int X;
			/// <summary>
			/// The vertical, or Y, location of the child within the view group.
			/// </summary>
			public int Y;

			/// <summary>
			/// Creates a new set of layout parameters with the specified width,
			/// height and location.
			/// </summary>
			/// <param name="width"> the width, either <seealso cref="#MATCH_PARENT"/>,
			///          <seealso cref="#WRAP_CONTENT"/> or a fixed size in pixels </param>
			/// <param name="height"> the height, either <seealso cref="#MATCH_PARENT"/>,
			///          <seealso cref="#WRAP_CONTENT"/> or a fixed size in pixels </param>
			/// <param name="x"> the X location of the child </param>
			/// <param name="y"> the Y location of the child </param>
			public LayoutParams(int width, int height, int x, int y) 
                : base(width, height)
			{
				this.X = x;
				this.Y = y;
			}

			/// <summary>
			/// Creates a new set of layout parameters. The values are extracted from
			/// the supplied attributes set and context. The XML attributes mapped
			/// to this set of layout parameters are:
			/// 
			/// <ul>
			///   <li><code>layout_x</code>: the X location of the child</li>
			///   <li><code>layout_y</code>: the Y location of the child</li>
			///   <li>All the XML attributes from
			///   <seealso cref="android.view.ViewGroup.LayoutParams"/></li>
			/// </ul>
			/// </summary>
			/// <param name="c"> the application environment </param>
			/// <param name="attrs"> the set of attributes from which to extract the layout
			///              parameters values </param>
			public LayoutParams(Context c, IAttributeSet attrs)
                : base(c, attrs)
			{
				/* FIX THIS eventually. Without this, I don't think you can put x and y in layout xml files.
				TypedArray a = c.obtainStyledAttributes(attrs,
				        com.android.internal.R.styleable.AbsoluteLayout_Layout);
				x = a.getDimensionPixelOffset(
				        com.android.internal.R.styleable.AbsoluteLayout_Layout_layout_x, 0);
				y = a.getDimensionPixelOffset(
				        com.android.internal.R.styleable.AbsoluteLayout_Layout_layout_y, 0);
				a.recycle();
				*/
			}

			/// <summary>
			/// {@inheritDoc}
			/// </summary>
			public LayoutParams(ViewGroup.LayoutParams source)
                : base(source)
			{
			}

			public string Debug(string output)
			{
				return output + "Absolute.LayoutParams={width=" + sizeToString(Width) + ", height=" + sizeToString(Height) + " x=" + X + " y=" + Y + "}";
			}

		  /// <summary>
		  /// Converts the specified size to a readable String.
		  /// </summary>
		  /// <param name="size"> the size to convert </param>
		  /// <returns> a String instance representing the supplied size
		  ///   
		  /// @hide </returns>
			protected static string sizeToString(int size)
			{
				if (size == WrapContent)
				{
					return "wrap-content";
				}
				if (size == MatchParent)
				{
					return "match-parent";
				}
				return Convert.ToString(size);
			}
		} // end class

	} // end class
}