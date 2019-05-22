using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Lang;
using Java.Util;
using System;

namespace AndroidStaggeredGrid
{
	/// <summary>
	/// A staggered grid view which supports multiple columns with rows of varying sizes.
	/// <p/>
	/// Builds multiple columns on top of <seealso cref="ExtendableListView"/>
	/// <p/>
	/// Partly inspired by - https://github.com/huewu/PinterestLikeAdapterView
	/// </summary>
	public class StaggeredGridView : ExtendableListView
	{

		private const string TAG = "StaggeredGridView";
		private const bool DBG = false;

		private const int DEFAULT_COLUMNS_PORTRAIT = 2;
		private const int DEFAULT_COLUMNS_LANDSCAPE = 3;

		private int mColumnCount;
		private int mItemMargin;
		private int mColumnWidth;
		private new bool mNeedSync;

		private int mColumnCountPortrait = DEFAULT_COLUMNS_PORTRAIT;
		private int mColumnCountLandscape = DEFAULT_COLUMNS_LANDSCAPE;

		/// <summary>
		/// A key-value collection where the key is the position and the
		/// <seealso cref="GridItemRecord"/> with some info about that position
		/// so we can maintain it's position - and reorg on orientation change.
		/// </summary>
		private SparseArray<GridItemRecord> mPositionData;
		private int mGridPaddingLeft;
		private int mGridPaddingRight;
		private int mGridPaddingTop;
		private int mGridPaddingBottom;

		/// <summary>
		///*
		/// Our grid item state record with <seealso cref="Parcelable"/> implementation
		/// so we can persist them across the SGV lifecycle.
		/// </summary>
		public class GridItemRecord : Java.Lang.Object, IParcelable
		{
            public int column;
            public double heightRatio;
            public bool isHeaderFooter;

			public GridItemRecord()
			{

			}

			/// <summary>
			/// Constructor called from <seealso cref="#CREATOR"/>
			/// </summary>
            public GridItemRecord(Parcel source)
			{
                column = source.ReadInt();
                heightRatio = source.ReadDouble();
                isHeaderFooter = source.ReadByte() == 1;
			}

			public int DescribeContents()
			{
				return 0;
			}

            public void WriteToParcel(Parcel source, ParcelableWriteFlags flags)
			{
                source.WriteInt(column);
                source.WriteDouble(heightRatio);
                source.WriteByte((sbyte)(isHeaderFooter ? 1 : 0));
			}

			public override string ToString()
			{
				return "GridItemRecord.ListSavedState{" + JavaSystem.IdentityHashCode(this).ToString("x") + " column:" + column + " heightRatio:" + heightRatio + " isHeaderFooter:" + isHeaderFooter + "}";
			}

            [ExportField("CREATOR")]
            public static GridItemParcelableCreator GridItemInitializeCreator()
            {
                Console.WriteLine("GridItemParcelableCreator.GridItemInitializeCreator");
                return new GridItemParcelableCreator();
            }

			public class GridItemParcelableCreator : Java.Lang.Object, IParcelableCreator
			{
                public GridItemParcelableCreator()
				{
				}

                public Java.Lang.Object CreateFromParcel(Parcel source)
				{
                    return new GridItemRecord(source);
				}

                public Java.Lang.Object[] NewArray(int size)
				{
					return new GridItemRecord[size];
				}
			}
		}

		/// <summary>
		/// The location of the top of each top item added in each column.
		/// </summary>
		private int[] mColumnTops;

		/// <summary>
		/// The location of the bottom of each bottom item added in each column.
		/// </summary>
		private int[] mColumnBottoms;

		/// <summary>
		/// The left location to put items for each column
		/// </summary>
		private int[] mColumnLefts;

		/// <summary>
		///*
		/// Tells us the distance we've offset from the top.
		/// Can be slightly off on orientation change - TESTING
		/// </summary>
		private int mDistanceToTop;

		public StaggeredGridView(Context context) 
            : this(context, null)
		{
		}

		public StaggeredGridView(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
		{
		}

		public StaggeredGridView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
		{

			if (attrs != null)
			{
				// get the number of columns in portrait and landscape
				TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.StaggeredGridView, defStyle, 0);

				mColumnCount = typedArray.GetInteger(Resource.Styleable.StaggeredGridView_column_count, 0);

				if (mColumnCount > 0)
				{
					mColumnCountPortrait = mColumnCount;
					mColumnCountLandscape = mColumnCount;
				}
				else
				{
					mColumnCountPortrait = typedArray.GetInteger(Resource.Styleable.StaggeredGridView_column_count_portrait, DEFAULT_COLUMNS_PORTRAIT);
					mColumnCountLandscape = typedArray.GetInteger(Resource.Styleable.StaggeredGridView_column_count_landscape, DEFAULT_COLUMNS_LANDSCAPE);
				}

				mItemMargin = typedArray.GetDimensionPixelSize(Resource.Styleable.StaggeredGridView_item_margin, 0);
				mGridPaddingLeft = typedArray.GetDimensionPixelSize(Resource.Styleable.StaggeredGridView_grid_paddingLeft, 0);
				mGridPaddingRight = typedArray.GetDimensionPixelSize(Resource.Styleable.StaggeredGridView_grid_paddingRight, 0);
				mGridPaddingTop = typedArray.GetDimensionPixelSize(Resource.Styleable.StaggeredGridView_grid_paddingTop, 0);
				mGridPaddingBottom = typedArray.GetDimensionPixelSize(Resource.Styleable.StaggeredGridView_grid_paddingBottom, 0);

				typedArray.Recycle();
			}

			mColumnCount = 0; // determined onMeasure
			// Creating these empty arrays to avoid saving null states
			mColumnTops = new int[0];
			mColumnBottoms = new int[0];
			mColumnLefts = new int[0];
			mPositionData = new SparseArray<GridItemRecord>();
		}

        public StaggeredGridView(IntPtr handle, JniHandleOwnership transfer)
            : base(handle, transfer)
        {

        }

		// //////////////////////////////////////////////////////////////////////////////////////////
		// PROPERTIES
		//

		// Grid padding is applied to the list item rows but not the header and footer
		public int RowPaddingLeft
		{
			get
			{
				return ListPaddingLeft + mGridPaddingLeft;
			}
		}

		public int RowPaddingRight
		{
			get
			{
				return ListPaddingRight + mGridPaddingRight;
			}
		}

		public int RowPaddingTop
		{
			get
			{
				return ListPaddingTop + mGridPaddingTop;
			}
		}

		public int RowPaddingBottom
		{
			get
			{
				return ListPaddingBottom + mGridPaddingBottom;
			}
		}

		public void SetGridPadding(int left, int top, int right, int bottom)
		{
			mGridPaddingLeft = left;
			mGridPaddingTop = top;
			mGridPaddingRight = right;
			mGridPaddingBottom = bottom;
		}

		public int ColumnCountPortrait
		{
			set
			{
				mColumnCountPortrait = value;
                OnSizeChanged(Width, Height);
                RequestLayoutChildren();
			}
		}

		public int ColumnCountLandscape
		{
			set
			{
				mColumnCountLandscape = value;
                OnSizeChanged(Width, Height);
                RequestLayoutChildren();
			}
		}

		public int ColumnCount
		{
			set
			{
				mColumnCountPortrait = value;
				mColumnCountLandscape = value;
				// mColumnCount set onSizeChanged();
				OnSizeChanged(Width, Height);
				RequestLayoutChildren();
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// MEASUREMENT
		//
		private bool Landscape
		{
			get
			{
				return Resources.Configuration.Orientation == Android.Content.Res.Orientation.Landscape;
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

			if (mColumnCount <= 0)
			{
				bool isLandscape = Landscape;
				mColumnCount = isLandscape ? mColumnCountLandscape : mColumnCountPortrait;
			}

			// our column width is the width of the listview
			// minus it's padding
			// minus the total items margin
			// divided by the number of columns
			mColumnWidth = CalculateColumnWidth(MeasuredWidth);

			if (mColumnTops == null || mColumnTops.Length != mColumnCount)
			{
				mColumnTops = new int[mColumnCount];
				InitColumnTops();
			}
			if (mColumnBottoms == null || mColumnBottoms.Length != mColumnCount)
			{
				mColumnBottoms = new int[mColumnCount];
				InitColumnBottoms();
			}
			if (mColumnLefts == null || mColumnLefts.Length != mColumnCount)
			{
				mColumnLefts = new int[mColumnCount];
				InitColumnLefts();
			}
		}

		protected override void OnMeasureChild(View child, LayoutParams layoutParams)
		{
			int viewType = layoutParams.viewType;
			int position = layoutParams.position;

            if (viewType == ItemViewTypeHeaderOrFooter || viewType == ItemViewTypeIgnore)
			{
				// for headers and weird ignored views
				base.OnMeasureChild(child, layoutParams);
			}
			else
			{
				if (DBG)
				{
					Console.WriteLine(TAG, "onMeasureChild BEFORE position:" + position + " h:" + MeasuredHeight);
				}
				// measure it to the width of our column.
				int childWidthSpec = MeasureSpec.MakeMeasureSpec(mColumnWidth, MeasureSpecMode.Exactly);
				int childHeightSpec;
				if (layoutParams.Height > 0)
				{
                    childHeightSpec = MeasureSpec.MakeMeasureSpec(layoutParams.Height, MeasureSpecMode.Exactly);
				}
				else
				{
                    childHeightSpec = MeasureSpec.MakeMeasureSpec(LayoutParams.WrapContent, MeasureSpecMode.Unspecified);
				}
				child.Measure(childWidthSpec, childHeightSpec);
			}

			int childHeight = GetChildHeight(child);
			SetPositionHeightRatio(position, childHeight);

			if (DBG)
			{
				Console.WriteLine(TAG, "onMeasureChild AFTER position:" + position + " h:" + childHeight);
			}
		}

		public int ColumnWidth
		{
			get
			{
				return mColumnWidth;
			}
		}

		public override void ResetToTop()
		{
			if (mColumnCount > 0)
			{

				if (mColumnTops == null)
				{
					mColumnTops = new int[mColumnCount];
				}
				if (mColumnBottoms == null)
				{
					mColumnBottoms = new int[mColumnCount];
				}
				InitColumnTopsAndBottoms();

				mPositionData.Clear();
				mNeedSync = false;
				mDistanceToTop = 0;
				SetSelection(0);
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// POSITIONING
		//

		protected override void OnChildCreated(int position, bool flowDown)
		{
            base.OnChildCreated(position, flowDown);

            if (!IsHeaderOrFooter(position))
			{
				// do we already have a column for this position?
				int column = GetChildColumn(position, flowDown);
				SetPositionColumn(position, column);
				if (DBG)
				{
					Console.WriteLine(TAG, "onChildCreated position:" + position + " is in column:" + column);
				}
			}
			else
			{
				PositionIsHeaderFooter = position;
			}
		}

		private void RequestLayoutChildren()
		{
			int count = ChildCount;
			for (int i = 0; i < count; i++)
			{
				View v = GetChildAt(i);
				if (v != null)
				{
					v.RequestLayout();
				}
			}
		}

		protected override void LayoutChildren()
		{
			PreLayoutChildren();
			base.LayoutChildren();
		}

		private void PreLayoutChildren()
		{
			// on a major re-layout reset for our next layout pass
			if (!mNeedSync)
			{
				Arrays.Fill(mColumnBottoms, 0);
			}
			else
			{
				mNeedSync = false;
			}
			// copy the tops into the bottom
			// since we're going to redo a layout pass that will draw down from
			// the top
			Array.Copy(mColumnTops, 0, mColumnBottoms, 0, mColumnCount);
		}

		// NOTE : Views will either be layout out via onLayoutChild
		// OR
		// Views will be offset if they are active but offscreen so that we can recycle!
		// Both onLayoutChild() and onOffsetChild are called after we measure our view
		// see ExtensibleListView.setupChild();

		protected override void OnLayoutChild(View child, int position, bool flowDown, int childrenLeft, int childTop, int childRight, int childBottom)
		{
            if (IsHeaderOrFooter(position))
			{
				LayoutGridHeaderFooter(child, position, flowDown, childrenLeft, childTop, childRight, childBottom);
			}
			else
			{
				LayoutGridChild(child, position, flowDown, childrenLeft, childRight);
			}
		}

		private void LayoutGridHeaderFooter(View child, int position, bool flowDown, int childrenLeft, int childTop, int childRight, int childBottom)
		{
			// offset the top and bottom of all our columns
			// if it's the footer we want it below the lowest child bottom
			int gridChildTop;
			int gridChildBottom;

			if (flowDown)
			{
				gridChildTop = LowestPositionedBottom;
				gridChildBottom = gridChildTop + GetChildHeight(child);
			}
			else
			{
				gridChildBottom = HighestPositionedTop;
				gridChildTop = gridChildBottom - GetChildHeight(child);
			}

			for (int i = 0; i < mColumnCount; i++)
			{
				UpdateColumnTopIfNeeded(i, gridChildTop);
				UpdateColumnBottomIfNeeded(i, gridChildBottom);
			}

			base.OnLayoutChild(child, position, flowDown, childrenLeft, gridChildTop, childRight, gridChildBottom);
		}

		private void LayoutGridChild(View child, int position, bool flowDown, int childrenLeft, int childRight)
		{
			// stash the bottom and the top if it's higher positioned
			int column = GetPositionColumn(position);

			int gridChildTop;
			int gridChildBottom;

			int childTopMargin = GetChildTopMargin(position);
			int childBottomMargin = ChildBottomMargin;
			int verticalMargins = childTopMargin + childBottomMargin;

			if (flowDown)
			{
				gridChildTop = mColumnBottoms[column]; // the next items top is the last items bottom
				gridChildBottom = gridChildTop + (GetChildHeight(child) + verticalMargins);
			}
			else
			{
				gridChildBottom = mColumnTops[column]; // the bottom of the next column up is our top
				gridChildTop = gridChildBottom - (GetChildHeight(child) + verticalMargins);
			}

			if (DBG)
			{
				Console.WriteLine(TAG, "onLayoutChild position:" + position + " column:" + column + " gridChildTop:" + gridChildTop + " gridChildBottom:" + gridChildBottom);
			}

			// we also know the column of this view so let's stash it in the
			// view's layout params
			GridLayoutParams layoutParams = (GridLayoutParams)child.LayoutParameters;
			layoutParams.column = column;

			UpdateColumnBottomIfNeeded(column, gridChildBottom);
			UpdateColumnTopIfNeeded(column, gridChildTop);

			// subtract the margins before layout
			gridChildTop += childTopMargin;
			gridChildBottom -= childBottomMargin;

			child.Layout(childrenLeft, gridChildTop, childRight, gridChildBottom);
		}

		protected override void OnOffsetChild(View child, int position, bool flowDown, int childrenLeft, int childTop)
		{
			// if the child is recycled and is just offset
			// we still want to add its deets into our store
            if (IsHeaderOrFooter(position))
			{

				OffsetGridHeaderFooter(child, position, flowDown, childrenLeft, childTop);
			}
			else
			{
				OffsetGridChild(child, position, flowDown, childrenLeft, childTop);
			}
		}

		private void OffsetGridHeaderFooter(View child, int position, bool flowDown, int childrenLeft, int childTop)
		{
			// offset the top and bottom of all our columns
			// if it's the footer we want it below the lowest child bottom
			int gridChildTop;
			int gridChildBottom;

			if (flowDown)
			{
				gridChildTop = LowestPositionedBottom;
				gridChildBottom = gridChildTop + GetChildHeight(child);
			}
			else
			{
				gridChildBottom = HighestPositionedTop;
				gridChildTop = gridChildBottom - GetChildHeight(child);
			}

			for (int i = 0; i < mColumnCount; i++)
			{
				UpdateColumnTopIfNeeded(i, gridChildTop);
				UpdateColumnBottomIfNeeded(i, gridChildBottom);
			}

			base.OnOffsetChild(child, position, flowDown, childrenLeft, gridChildTop);
		}

		private void OffsetGridChild(View child, int position, bool flowDown, int childrenLeft, int childTop)
		{
			// stash the bottom and the top if it's higher positioned
			int column = GetPositionColumn(position);

			int gridChildTop;
			int gridChildBottom;

			int childTopMargin = GetChildTopMargin(position);
			int childBottomMargin = ChildBottomMargin;
			int verticalMargins = childTopMargin + childBottomMargin;

			if (flowDown)
			{
				gridChildTop = mColumnBottoms[column]; // the next items top is the last items bottom
				gridChildBottom = gridChildTop + (GetChildHeight(child) + verticalMargins);
			}
			else
			{
				gridChildBottom = mColumnTops[column]; // the bottom of the next column up is our top
				gridChildTop = gridChildBottom - (GetChildHeight(child) + verticalMargins);
			}

			if (DBG)
			{
				Console.WriteLine(TAG, "onOffsetChild position:" + position + " column:" + column + " childTop:" + childTop + " gridChildTop:" + gridChildTop + " gridChildBottom:" + gridChildBottom);
			}

			// we also know the column of this view so let's stash it in the
			// view's layout params
			GridLayoutParams layoutParams = (GridLayoutParams)child.LayoutParameters;
			layoutParams.column = column;

			UpdateColumnBottomIfNeeded(column, gridChildBottom);
			UpdateColumnTopIfNeeded(column, gridChildTop);

			base.OnOffsetChild(child, position, flowDown, childrenLeft, gridChildTop + childTopMargin);
		}

		private int GetChildHeight(View child)
		{
			return child.MeasuredHeight;
		}

		private int GetChildTopMargin(int position)
		{
			bool isFirstRow = position < (HeaderViewsCount + mColumnCount);
			return isFirstRow ? mItemMargin : 0;
		}

		private int ChildBottomMargin
		{
			get
			{
				return mItemMargin;
			}
		}

		protected override LayoutParams GenerateChildLayoutParams(View child)
		{
			GridLayoutParams layoutParams = null;

			ViewGroup.LayoutParams childParams = child.LayoutParameters;
			if (childParams != null)
			{
				if (childParams is GridLayoutParams)
				{
					layoutParams = (GridLayoutParams) childParams;
				}
				else
				{
					layoutParams = new GridLayoutParams(childParams);
				}
			}
			if (layoutParams == null)
			{
				layoutParams = new GridLayoutParams(mColumnWidth, ViewGroup.LayoutParams.WrapContent);
			}

			return layoutParams;
		}

		private void UpdateColumnTopIfNeeded(int column, int childTop)
		{
			if (childTop < mColumnTops[column])
			{
				mColumnTops[column] = childTop;
			}
		}

		private void UpdateColumnBottomIfNeeded(int column, int childBottom)
		{
			if (childBottom > mColumnBottoms[column])
			{
				mColumnBottoms[column] = childBottom;
			}
		}

		protected override int GetChildLeft(int position)
		{
            if (IsHeaderOrFooter(position))
			{
				return base.GetChildLeft(position);
			}
			else
			{
				int column = GetPositionColumn(position);
				return mColumnLefts[column];
			}
		}

		protected override int GetChildTop(int position)
		{
            if (IsHeaderOrFooter(position))
			{
				return base.GetChildTop(position);
			}
			else
			{
				int column = GetPositionColumn(position);
				if (column == -1)
				{
					return HighestPositionedBottom;
				}
				return mColumnBottoms[column];
			}
		}

		/// <summary>
		/// Get the top for the next child down in our view
		/// (maybe a column across) so we can fill down.
		/// </summary>
		protected override int GetNextChildDownsTop(int position)
		{
            if (IsHeaderOrFooter(position))
			{
				return base.GetNextChildDownsTop(position);
			}
			else
			{
				return HighestPositionedBottom;
			}
		}

		protected override int GetChildBottom(int position)
		{
            if (IsHeaderOrFooter(position))
			{
				return base.GetChildBottom(position);
			}
			else
			{
				int column = GetPositionColumn(position);
				if (column == -1)
				{
					return LowestPositionedTop;
				}
				return mColumnTops[column];
			}
		}

		/// <summary>
		/// Get the bottom for the next child up in our view
		/// (maybe a column across) so we can fill up.
		/// </summary>
		protected override int GetNextChildUpsBottom(int position)
		{
            if (IsHeaderOrFooter(position))
			{
				return base.GetNextChildUpsBottom(position);
			}
			else
			{
				return LowestPositionedTop;
			}
		}

		protected override int LastChildBottom
		{
			get
			{
				int lastPosition = mFirstPosition + (ChildCount - 1);
                if (IsHeaderOrFooter(lastPosition))
				{
					return base.LastChildBottom;
				}
				return HighestPositionedBottom;
			}
		}

		protected override int FirstChildTop
		{
			get
			{
                if (IsHeaderOrFooter(mFirstPosition))
				{
					return base.FirstChildTop;
				}
				return LowestPositionedTop;
			}
		}

		protected override int HighestChildTop
		{
			get
			{
				if (IsHeaderOrFooter(mFirstPosition))
				{
					return base.HighestChildTop;
				}
				return HighestPositionedTop;
			}
		}

		protected override int LowestChildBottom
		{
			get
			{
				int lastPosition = mFirstPosition + (ChildCount - 1);
				if (IsHeaderOrFooter(lastPosition))
				{
					return base.LowestChildBottom;
				}
				return LowestPositionedBottom;
			}
		}

		protected override void OffsetChildrenTopAndBottom(int offset)
		{
			base.OffsetChildrenTopAndBottom(offset);
			OffsetAllColumnsTopAndBottom(offset);
			OffsetDistanceToTop(offset);
		}

        protected void OffsetChildrenTopAndBottom(int offset, int column)
		{
			if (DBG)
			{
				Console.WriteLine(TAG, "offsetChildrenTopAndBottom: " + offset + " column:" + column);
			}
			int count = ChildCount;
			for (int i = 0; i < count; i++)
			{
				View v = GetChildAt(i);
				if (v != null && v.LayoutParameters != null && v.LayoutParameters is GridLayoutParams)
				{
					GridLayoutParams lp = (GridLayoutParams) v.LayoutParameters;
					if (lp.column == column)
					{
						v.OffsetTopAndBottom(offset);
					}
				}
			}
			OffsetColumnTopAndBottom(offset, column);
		}

		private void OffsetDistanceToTop(int offset)
		{
			mDistanceToTop += offset;
			if (DBG)
			{
				Console.WriteLine(TAG, "offset mDistanceToTop:" + mDistanceToTop);
			}
		}

		public int DistanceToTop
		{
			get
			{
				return mDistanceToTop;
			}
		}

		private void OffsetAllColumnsTopAndBottom(int offset)
		{
			if (offset != 0)
			{
				for (int i = 0; i < mColumnCount; i++)
				{
					OffsetColumnTopAndBottom(offset, i);
				}
			}
		}

		private void OffsetColumnTopAndBottom(int offset, int column)
		{
			if (offset != 0)
			{
				mColumnTops[column] += offset;
				mColumnBottoms[column] += offset;
			}
		}

		protected override void AdjustViewsAfterFillGap(bool down)
		{
			base.AdjustViewsAfterFillGap(down);
			// fix vertical gaps when hitting the top after a rotate
			// only when scrolling back up!
			if (!down)
			{
				AlignTops();
			}
		}

		private void AlignTops()
		{
			if (mFirstPosition == HeaderViewsCount)
			{
				// we're showing all the views before the header views
				int[] nonHeaderTops = HighestNonHeaderTops;
				// we should now have our non header tops
				// align them
				bool isAligned = true;
				int highestColumn = -1;
				int highestTop = int.MaxValue;
				for (int i = 0; i < nonHeaderTops.Length; i++)
				{
					// are they all aligned
					if (isAligned && i > 0 && nonHeaderTops[i] != highestTop)
					{
						isAligned = false; // not all the tops are aligned
					}
					// what's the highest
					if (nonHeaderTops[i] < highestTop)
					{
						highestTop = nonHeaderTops[i];
						highestColumn = i;
					}
				}

				// skip the rest.
				if (isAligned)
				{
					return;
				}

				// we've got the highest column - lets align the others
				for (int i = 0; i < nonHeaderTops.Length; i++)
				{
					if (i != highestColumn)
					{
						// there's a gap in this column
						int offset = highestTop - nonHeaderTops[i];
						OffsetChildrenTopAndBottom(offset, i);
					}
				}
				Invalidate();
			}
		}

		private int[] HighestNonHeaderTops
		{
			get
			{
				int[] nonHeaderTops = new int[mColumnCount];
				int childCount = ChildCount;
				if (childCount > 0)
				{
					for (int i = 0; i < childCount; i++)
					{
						View child = GetChildAt(i);
						if (child != null && child.LayoutParameters != null && child.LayoutParameters is GridLayoutParams)
						{
							// is this child's top the highest non
							GridLayoutParams lp = (GridLayoutParams) child.LayoutParameters;
							// is it a child that isn't a header
                            if (lp.viewType != ItemViewTypeHeaderOrFooter && child.Top < nonHeaderTops[lp.column])
							{
								nonHeaderTops[lp.column] = child.Top;
							}
						}
					}
				}
				return nonHeaderTops;
			}
		}

		protected override void OnChildrenDetached(int start, int count)
		{
            base.OnChildrenDetached(start, count);
			// go through our remaining views and sync the top and bottom stash.

			// Repair the top and bottom column boundaries from the views we still have
			Arrays.Fill(mColumnTops, int.MaxValue);
			Arrays.Fill(mColumnBottoms, 0);

			for (int i = 0; i < ChildCount; i++)
			{
				View child = GetChildAt(i);
				if (child != null)
				{
					LayoutParams childParams = (LayoutParams) child.LayoutParameters;
                    if (childParams.viewType != ItemViewTypeHeaderOrFooter && childParams is GridLayoutParams)
					{
						GridLayoutParams layoutParams = (GridLayoutParams) childParams;
						int column = layoutParams.column;
						int position = layoutParams.position;
						int childTop = child.Top;
						if (childTop < mColumnTops[column])
						{
							mColumnTops[column] = childTop - GetChildTopMargin(position);
						}
						int childBottom = child.Bottom;
						if (childBottom > mColumnBottoms[column])
						{
							mColumnBottoms[column] = childBottom + ChildBottomMargin;
						}
					}
					else
					{
						// the header and footer here
						int childTop = child.Top;
						int childBottom = child.Bottom;

						for (int col = 0; col < mColumnCount; col++)
						{
							if (childTop < mColumnTops[col])
							{
								mColumnTops[col] = childTop;
							}
							if (childBottom > mColumnBottoms[col])
							{
								mColumnBottoms[col] = childBottom;
							}
						}

					}
				}
			}
		}

		protected override bool HasSpaceUp()
		{
			int end = mClipToPadding ? RowPaddingTop : 0;
			return LowestPositionedTop > end;
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// SYNCING ACROSS ROTATION
		//

		protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
		{
            base.OnSizeChanged(w, h, oldw, oldh);
			OnSizeChanged(w, h);
		}

        protected override void OnSizeChanged(int w, int h)
		{
            base.OnSizeChanged(w, h);
			bool isLandscape = Landscape;
			int newColumnCount = isLandscape ? mColumnCountLandscape : mColumnCountPortrait;
			if (mColumnCount != newColumnCount)
			{
				mColumnCount = newColumnCount;

				mColumnWidth = CalculateColumnWidth(w);

				mColumnTops = new int[mColumnCount];
				mColumnBottoms = new int[mColumnCount];
				mColumnLefts = new int[mColumnCount];

				mDistanceToTop = 0;

				// rebuild the columns
				InitColumnTopsAndBottoms();
				InitColumnLefts();

				// if we have data
				if (Count > 0 && mPositionData.Size() > 0)
				{
					OnColumnSync();
				}

				RequestLayout();
			}
		}

		private int CalculateColumnWidth(int gridWidth)
		{
			int listPadding = RowPaddingLeft + RowPaddingRight;
			return (gridWidth - listPadding - mItemMargin * (mColumnCount + 1)) / mColumnCount;
		}

		private int CalculateColumnLeft(int colIndex)
		{
			return RowPaddingLeft + mItemMargin + ((mItemMargin + mColumnWidth) * colIndex);
		}

		/// <summary>
		///*
		/// Our mColumnTops and mColumnBottoms need to be re-built up to the
		/// mSyncPosition - the following layout request will then
		/// layout the that position and then fillUp and fillDown appropriately.
		/// </summary>
		private void OnColumnSync()
		{
			// re-calc tops for new column count!
			int syncPosition = System.Math.Min(mSyncPosition, Count - 1);

			SparseArray<double?> positionHeightRatios = new SparseArray<double?>(syncPosition);
			for (int pos = 0; pos < syncPosition; pos++)
			{
				// check for weirdness
				GridItemRecord rec = mPositionData.Get(pos);
				if (rec == null)
				{
					break;
				}

				Console.WriteLine(TAG, "onColumnSync:" + pos + " ratio:" + rec.heightRatio);
				positionHeightRatios.Append(pos, rec.heightRatio);
			}

			mPositionData.Clear();

			// re-calc our relative position while at the same time
			// rebuilding our GridItemRecord collection

			if (DBG)
			{
				Console.WriteLine(TAG, "onColumnSync column width:" + mColumnWidth);
			}

			for (int pos = 0; pos < syncPosition; pos++)
			{
				//Check for weirdness again
				double? heightRatio = positionHeightRatios.Get(pos);
				if (heightRatio == null)
				{
					break;
				}

				GridItemRecord rec = GetOrCreateRecord(pos);
				int height = (int)(mColumnWidth * heightRatio);
				rec.heightRatio = (double)heightRatio;

				int top;
				int bottom;
				// check for headers
				if (IsHeaderOrFooter(pos))
				{
					// the next top is the bottom for that column
					top = LowestPositionedBottom;
					bottom = top + height;

					for (int i = 0; i < mColumnCount; i++)
					{
						mColumnTops[i] = top;
						mColumnBottoms[i] = bottom;
					}
				}
				else
				{
					// what's the next column down ?
					int column = HighestPositionedBottomColumn;
					// the next top is the bottom for that column
					top = mColumnBottoms[column];
					bottom = top + height + GetChildTopMargin(pos) + ChildBottomMargin;

					mColumnTops[column] = top;
					mColumnBottoms[column] = bottom;

					rec.column = column;
				}


				if (DBG)
				{
					Console.WriteLine(TAG, "onColumnSync position:" + pos + " top:" + top + " bottom:" + bottom + " height:" + height + " heightRatio:" + heightRatio);
				}
			}

			// our sync position will be displayed in this column
			int syncColumn = HighestPositionedBottomColumn;
			SetPositionColumn(syncPosition, syncColumn);

			// we want to offset from height of the sync position
			// minus the offset
			int syncToBottom = mColumnBottoms[syncColumn];
			int offset = -syncToBottom + mSpecificTop;
			// offset all columns by
			OffsetAllColumnsTopAndBottom(offset);

			// sync the distance to top
			mDistanceToTop = -syncToBottom;

			// stash our bottoms in our tops - though these will be copied back to the bottoms
			Array.Copy(mColumnBottoms, 0, mColumnTops, 0, mColumnCount);
		}


		// //////////////////////////////////////////////////////////////////////////////////////////
		// GridItemRecord UTILS
		//

		private void SetPositionColumn(int position, int column)
		{
			GridItemRecord rec = GetOrCreateRecord(position);
			rec.column = column;
		}

		private void SetPositionHeightRatio(int position, int height)
		{
			GridItemRecord rec = GetOrCreateRecord(position);
			rec.heightRatio = (double) height / (double) mColumnWidth;
			if (DBG)
			{
				Console.WriteLine(TAG, "position:" + position + " width:" + mColumnWidth + " height:" + height + " heightRatio:" + rec.heightRatio);
			}
		}

		private int PositionIsHeaderFooter
		{
			set
			{
				GridItemRecord rec = GetOrCreateRecord(value);
				rec.isHeaderFooter = true;
			}
		}

		private GridItemRecord GetOrCreateRecord(int position)
		{
			GridItemRecord rec = mPositionData.Get(position, null);
			if (rec == null)
			{
				rec = new GridItemRecord();
				mPositionData.Append(position, rec);
			}
			return rec;
		}

		private int GetPositionColumn(int position)
		{
			GridItemRecord rec = mPositionData.Get(position, null);
			return rec != null ? rec.column : -1;
		}


		// //////////////////////////////////////////////////////////////////////////////////////////
		// HELPERS
		//

		private bool IsHeaderOrFooter(int position)
		{
			int viewType = mAdapter.GetItemViewType(position);
			return viewType == ItemViewTypeHeaderOrFooter;
		}

		private int GetChildColumn(int position, bool flowDown)
		{

			// do we already have a column for this child position?
			int column = GetPositionColumn(position);
			// we don't have the column or it no longer fits in our grid
			int columnCount = mColumnCount;
			if (column < 0 || column >= columnCount)
			{
				// if we're going down -
				// get the highest positioned (lowest value)
				// column bottom
				if (flowDown)
				{
					column = HighestPositionedBottomColumn;
				}
				else
				{
					column = LowestPositionedTopColumn;

				}
			}
			return column;
		}

		private void InitColumnTopsAndBottoms()
		{
			InitColumnTops();
			InitColumnBottoms();
		}

		private void InitColumnTops()
		{
			Arrays.Fill(mColumnTops, PaddingTop + mGridPaddingTop);
		}

		private void InitColumnBottoms()
		{
			Arrays.Fill(mColumnBottoms, PaddingTop + mGridPaddingTop);
		}

		private void InitColumnLefts()
		{
			for (int i = 0; i < mColumnCount; i++)
			{
				mColumnLefts[i] = CalculateColumnLeft(i);
			}
		}


		// //////////////////////////////////////////////////////////////////////////////////////////
		// BOTTOM
		//

		private int HighestPositionedBottom
		{
			get
			{
				int column = HighestPositionedBottomColumn;
				return mColumnBottoms[column];
			}
		}

		private int HighestPositionedBottomColumn
		{
			get
			{
				int columnFound = 0;
				int highestPositionedBottom = int.MaxValue;
				// the highest positioned bottom is the one with the lowest value :D
				for (int i = 0; i < mColumnCount; i++)
				{
					int bottom = mColumnBottoms[i];
					if (bottom < highestPositionedBottom)
					{
						highestPositionedBottom = bottom;
						columnFound = i;
					}
				}
				return columnFound;
			}
		}

		private int LowestPositionedBottom
		{
			get
			{
				int column = LowestPositionedBottomColumn;
				return mColumnBottoms[column];
			}
		}

		private int LowestPositionedBottomColumn
		{
			get
			{
				int columnFound = 0;
				int lowestPositionedBottom = int.MinValue;
				// the lowest positioned bottom is the one with the highest value :D
				for (int i = 0; i < mColumnCount; i++)
				{
					int bottom = mColumnBottoms[i];
					if (bottom > lowestPositionedBottom)
					{
						lowestPositionedBottom = bottom;
						columnFound = i;
					}
				}
				return columnFound;
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// TOP
		//

		private int LowestPositionedTop
		{
			get
			{
				int column = LowestPositionedTopColumn;
				return mColumnTops[column];
			}
		}

		private int LowestPositionedTopColumn
		{
			get
			{
				int columnFound = 0;
				// we'll go backwards through since the right most
				// will likely be the lowest positioned Top
				int lowestPositionedTop = int.MinValue;
				// the lowest positioned top is the one with the highest value :D
				for (int i = 0; i < mColumnCount; i++)
				{
					int top = mColumnTops[i];
					if (top > lowestPositionedTop)
					{
						lowestPositionedTop = top;
						columnFound = i;
					}
				}
				return columnFound;
			}
		}

		private int HighestPositionedTop
		{
			get
			{
				int column = HighestPositionedTopColumn;
				return mColumnTops[column];
			}
		}

		private int HighestPositionedTopColumn
		{
			get
			{
				int columnFound = 0;
				int highestPositionedTop = int.MaxValue;
				// the highest positioned top is the one with the lowest value :D
				for (int i = 0; i < mColumnCount; i++)
				{
					int top = mColumnTops[i];
					if (top < highestPositionedTop)
					{
						highestPositionedTop = top;
						columnFound = i;
					}
				}
				return columnFound;
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// LAYOUT PARAMS
		//

		/// <summary>
		/// Extended LayoutParams to column position and anything else we may been for the grid
		/// </summary>
		public class GridLayoutParams : LayoutParams
		{

			// The column the view is displayed in
            public int column;

			public GridLayoutParams(Context c, IAttributeSet attrs) 
                : base(c, attrs)
			{
                EnforceStaggeredLayout();
			}

			public GridLayoutParams(int w, int h) : base(w, h)
			{
                EnforceStaggeredLayout();
			}

			public GridLayoutParams(int w, int h, int viewType) : base(w, h)
			{
                EnforceStaggeredLayout();
			}

			public GridLayoutParams(ViewGroup.LayoutParams source) : base(source)
			{
                EnforceStaggeredLayout();
			}

			/// <summary>
			/// Here we're making sure that all grid view items
			/// are width MATCH_PARENT and height WRAP_CONTENT.
			/// That's what this grid is designed for
			/// </summary>
			public void EnforceStaggeredLayout()
			{
				if (Width != MatchParent)
				{
                    Width = MatchParent;
				}
                if (Height == MatchParent)
				{
                    Height = MatchParent;
				}
			}
		}

		// //////////////////////////////////////////////////////////////////////////////////////////
		// SAVED STATE


		public class GridListSavedState : ListSavedState
		{
            public int ColumnCount;
            public int[] ColumnTops;
            public SparseArray PositionData;

			public GridListSavedState(IParcelable superState, AbsListView listview) 
                : base(superState, listview)
			{
			}

			/// <summary>
			/// Constructor called from <seealso cref="#CREATOR"/>
			/// </summary>
			public GridListSavedState(Parcel source) : base(source)
			{
				ColumnCount = source.ReadInt();
				ColumnTops = new int[ColumnCount >= 0 ? ColumnCount : 0];
				source.ReadIntArray(ColumnTops);
				PositionData = source.ReadSparseArray(new GridItemRecord().Class.ClassLoader);
			}

			public override void WriteToParcel(Parcel source, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(source, flags);

				source.WriteInt(ColumnCount);
				source.WriteIntArray(ColumnTops);
				source.WriteSparseArray(PositionData);
			}

			public override string ToString()
			{
				return "StaggeredGridView.GridListSavedState{" + JavaSystem.IdentityHashCode(this).ToString("x") + "}";
			}

            [ExportField("CREATOR")]
            public static GridListParcelableCreator GridListInitializeCreator()
            {
                Console.WriteLine("GridListParcelableCreator.GridListInitializeCreator");
                return new GridListParcelableCreator();
            }

            public class GridListParcelableCreator : Java.Lang.Object, IParcelableCreator
			{
                public Java.Lang.Object CreateFromParcel(Parcel source)
				{
                    return new GridListSavedState(source);
				}

                public Java.Lang.Object[] NewArray(int size)
				{
					return new GridListSavedState[size];
				}
			}
		}


        public override IParcelable OnSaveInstanceState()
		{
			ListSavedState listState = (ListSavedState) base.OnSaveInstanceState();
			GridListSavedState ss = new GridListSavedState(listState.SuperState, this);

			// from the list state
			ss.SelectedId = listState.SelectedId;
			ss.FirstId = listState.FirstId;
			ss.ViewTop = listState.ViewTop;
			ss.Position = listState.Position;
			ss.Height = listState.Height;

			// our state

			bool haveChildren = ChildCount > 0 && Count > 0;

			if (haveChildren && mFirstPosition > 0)
			{
				ss.ColumnCount = mColumnCount;
				ss.ColumnTops = mColumnTops;
				ss.PositionData = mPositionData;
			}
			else
			{
				ss.ColumnCount = mColumnCount >= 0 ? mColumnCount : 0;
				ss.ColumnTops = new int[ss.ColumnCount];
				ss.PositionData = new SparseArray<object>();
			}

			return ss;
		}

		public override void OnRestoreInstanceState(IParcelable state)
		{
			GridListSavedState ss = (GridListSavedState)state;
			mColumnCount = ss.ColumnCount;
			mColumnTops = ss.ColumnTops;
			mColumnBottoms = new int[mColumnCount];
			mPositionData = (SparseArray<GridItemRecord>)ss.PositionData;
			mNeedSync = true;

			base.OnRestoreInstanceState(ss);
		}
	}
}