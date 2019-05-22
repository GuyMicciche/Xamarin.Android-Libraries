using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;

namespace AndroidFlowLayout
{
	public class FlowLayout : ViewGroup
	{
		public const int HORIZONTAL = 0;
		public const int VERTICAL = 1;
		public const int LAYOUT_DIRECTION_LTR = 0;
		public const int LAYOUT_DIRECTION_RTL = 1;

		private LayoutConfiguration config;
        private IList<LineDefinition> lines = new List<LineDefinition>();

		public FlowLayout(Context context) : base(context)
		{
			this.config = new LayoutConfiguration(context, null);
		}

		public FlowLayout(Context context, IAttributeSet attributeSet) : base(context, attributeSet)
		{
			this.config = new LayoutConfiguration(context, attributeSet);
		}

		public FlowLayout(Context context, IAttributeSet attributeSet, int defStyle) : base(context, attributeSet, defStyle)
		{
			this.config = new LayoutConfiguration(context, attributeSet);
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			int sizeWidth = MeasureSpec.GetSize(widthMeasureSpec) - this.PaddingRight - this.PaddingLeft;
			int sizeHeight = MeasureSpec.GetSize(heightMeasureSpec) - this.PaddingTop - this.PaddingBottom;

			MeasureSpecMode modeWidth = MeasureSpec.GetMode(widthMeasureSpec);
            MeasureSpecMode modeHeight = MeasureSpec.GetMode(heightMeasureSpec);

			int controlMaxLength = this.config.Orientation == HORIZONTAL ? sizeWidth : sizeHeight;
			int controlMaxThickness = this.config.Orientation == HORIZONTAL ? sizeHeight : sizeWidth;

            MeasureSpecMode modeLength = this.config.Orientation == HORIZONTAL ? modeWidth : modeHeight;
            MeasureSpecMode modeThickness = this.config.Orientation == HORIZONTAL ? modeHeight : modeWidth;

			lines.Clear();
			LineDefinition currentLine = new LineDefinition(controlMaxLength, config);
			lines.Add(currentLine);

			int count = this.ChildCount;
			for (int i = 0; i < count; i++)
			{
				View child = this.GetChildAt(i);
				if (child.Visibility == ViewStates.Gone)
				{
					continue;
				}

                CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;

				child.Measure(GetChildMeasureSpec(widthMeasureSpec, this.PaddingLeft + this.PaddingRight, lp.Width), GetChildMeasureSpec(heightMeasureSpec, this.PaddingTop + this.PaddingBottom, lp.Height));

				lp.ClearCalculatedFields(this.config.Orientation);
				if (this.config.Orientation == FlowLayout.HORIZONTAL)
				{
					lp.Length = child.MeasuredWidth;
					lp.Thickness = child.MeasuredHeight;
				}
				else
				{
					lp.Length = child.MeasuredHeight;
					lp.Thickness = child.MeasuredWidth;
				}

				bool newLine = lp.newLine || (modeLength != MeasureSpecMode.Unspecified && !currentLine.CanFit(child));
				if (newLine)
				{
					currentLine = new LineDefinition(controlMaxLength, config);
					if (this.config.Orientation == VERTICAL && this.config.Direction == LAYOUT_DIRECTION_RTL)
					{
						lines.Insert(0, currentLine);
					}
					else
					{
						lines.Add(currentLine);
					}
				}

				if (this.config.Orientation == HORIZONTAL && this.config.Direction == LAYOUT_DIRECTION_RTL)
				{
					currentLine.AddView(0, child);
				}
				else
				{
					currentLine.AddView(child);
				}
			}

			this.CalculateLinesAndChildPosition(lines);

			int contentLength = 0;
			foreach (LineDefinition l in lines)
			{
				contentLength = Math.Max(contentLength, l.LineLength);
			}
			int contentThickness = currentLine.LineStartThickness + currentLine.LineThickness;

			int realControlLength = this.FindSize(modeLength, controlMaxLength, contentLength);
			int realControlThickness = this.FindSize(modeHeight, controlMaxThickness, contentThickness);

			this.ApplyGravityToLines(lines, realControlLength, realControlThickness);

			foreach (LineDefinition line in lines)
			{
				this.ApplyGravityToLine(line);
				this.ApplyPositionsToViews(line);
			}

			/* need to take padding into account */
			int totalControlWidth = this.PaddingLeft + this.PaddingRight;
			int totalControlHeight = this.PaddingBottom + this.PaddingTop;
			if (this.config.Orientation == HORIZONTAL)
			{
				totalControlWidth += contentLength;
				totalControlHeight += contentThickness;
			}
			else
			{
				totalControlWidth += contentThickness;
				totalControlHeight += contentLength;
			}
			this.SetMeasuredDimension(ResolveSize(totalControlWidth, widthMeasureSpec), ResolveSize(totalControlHeight, heightMeasureSpec));
		}

        private int FindSize(MeasureSpecMode modeSize, int controlMaxSize, int contentSize)
		{
			int realControlLength;
			switch (modeSize)
			{
                case MeasureSpecMode.Unspecified:
					realControlLength = contentSize;
					break;
                case MeasureSpecMode.AtMost:
					realControlLength = Math.Min(contentSize, controlMaxSize);
					break;
                case MeasureSpecMode.Exactly:
					realControlLength = controlMaxSize;
					break;
				default:
					realControlLength = contentSize;
					break;
			}
			return realControlLength;
		}

		private void CalculateLinesAndChildPosition(IList<LineDefinition> lines)
		{
			int prevLinesThickness = 0;
			foreach (LineDefinition line in lines)
			{
				line.AddLineStartThickness(prevLinesThickness);
				prevLinesThickness += line.LineThickness;
				int prevChildThickness = 0;
				foreach (View child in line.Views)
				{
                    CustomLayoutParams layoutParams = (CustomLayoutParams)child.LayoutParameters;
					layoutParams.InlineStartLength = prevChildThickness;
					prevChildThickness += layoutParams.Length + layoutParams.SpacingLength;
				}
			}
		}

		private void ApplyPositionsToViews(LineDefinition line)
		{
			foreach (View child in line.Views)
			{
                CustomLayoutParams layoutParams = (CustomLayoutParams)child.LayoutParameters;
				if (this.config.Orientation == HORIZONTAL)
				{
					layoutParams.SetPosition(this.PaddingLeft + line.LineStartLength + layoutParams.InlineStartLength, this.PaddingTop + line.LineStartThickness + layoutParams.InlineStartThickness);
                    child.Measure(MeasureSpec.MakeMeasureSpec(layoutParams.Length, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(layoutParams.Thickness, MeasureSpecMode.Exactly));
				}
				else
				{
					layoutParams.SetPosition(this.PaddingLeft + line.LineStartThickness + layoutParams.InlineStartThickness, this.PaddingTop + line.LineStartLength + layoutParams.InlineStartLength);
                    child.Measure(MeasureSpec.MakeMeasureSpec(layoutParams.Thickness, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(layoutParams.Length, MeasureSpecMode.Exactly));
				}
			}
		}

		private void ApplyGravityToLines(IList<LineDefinition> lines, int realControlLength, int realControlThickness)
		{
			int linesCount = lines.Count;
			if (linesCount <= 0)
			{
				return;
			}

			int totalWeight = linesCount;
			LineDefinition lastLine = lines[linesCount - 1];
			int excessThickness = realControlThickness - (lastLine.LineThickness + lastLine.LineStartThickness);
			int excessOffset = 0;
			foreach (LineDefinition child in lines)
			{
				int weight = 1;
				GravityFlags gravity = this.Gravity;
				int extraThickness = (int)Math.Round((double)(excessThickness * weight / totalWeight));

				int childLength = child.LineLength;
				int childThickness = child.LineThickness;

				Rect container = new Rect();
				container.Top = excessOffset;
				container.Left = 0;
				container.Right = realControlLength;
				container.Bottom = childThickness + extraThickness + excessOffset;

				Rect result = new Rect();
				Android.Views.Gravity.Apply(gravity, childLength, childThickness, container, result);

				excessOffset += extraThickness;
                child.AddLineStartLength(result.Left);
                child.AddLineStartThickness(result.Top);
				child.Length = result.Width();
				child.Thickness = result.Height();
			}
		}

		private void ApplyGravityToLine(LineDefinition line)
		{
			int viewCount = line.Views.Count;
			if (viewCount <= 0)
			{
				return;
			}

			float totalWeight = 0;
			foreach (View prev in line.Views)
			{
                CustomLayoutParams plp = (CustomLayoutParams)prev.LayoutParameters;
				totalWeight += this.GetWeight(plp);
			}

			View lastChild = line.Views[viewCount - 1];
            CustomLayoutParams lastChildLayoutParams = (CustomLayoutParams)lastChild.LayoutParameters;
			int excessLength = line.LineLength - (lastChildLayoutParams.Length + lastChildLayoutParams.InlineStartLength);
			int excessOffset = 0;
			foreach (View child in line.Views)
			{
                CustomLayoutParams layoutParams = (CustomLayoutParams)child.LayoutParameters;

				float weight = this.GetWeight(layoutParams);
                GravityFlags gravity = this.GetGravity(layoutParams);
				int extraLength = (int)Math.Round((double)(excessLength * weight / totalWeight));

				int childLength = layoutParams.Length + layoutParams.SpacingLength;
				int childThickness = layoutParams.Thickness + layoutParams.SpacingThickness;

				Rect container = new Rect();
				container.Top = 0;
				container.Left = excessOffset;
				container.Right = childLength + extraLength + excessOffset;
				container.Bottom = line.LineThickness;

				Rect result = new Rect();
				Android.Views.Gravity.Apply(gravity, childLength, childThickness, container, result);

				excessOffset += extraLength;
                layoutParams.InlineStartLength = result.Left + layoutParams.InlineStartLength;
                layoutParams.InlineStartThickness = result.Top;
				layoutParams.Length = result.Width() - layoutParams.SpacingLength;
				layoutParams.Thickness = result.Height() - layoutParams.SpacingThickness;
			}
		}

        private GravityFlags GetGravity(CustomLayoutParams lp)
		{
			return lp.GravitySpecified() ? lp.gravity : this.config.Gravity;
		}

        private float GetWeight(CustomLayoutParams lp)
		{
			return lp.WeightSpecified() ? lp.weight : this.config.WeightDefault;
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			int count = this.ChildCount;
			for (int i = 0; i < count; i++)
			{
				View child = this.GetChildAt(i);
                CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;
				child.Layout(lp.x + lp.LeftMargin, lp.y + lp.TopMargin, lp.x + lp.LeftMargin + child.MeasuredWidth, lp.y + lp.TopMargin + child.MeasuredHeight);
			}
		}

		protected override bool DrawChild(Canvas canvas, View child, long drawingTime)
		{
			bool more = base.DrawChild(canvas, child, drawingTime);
			this.DrawDebugInfo(canvas, child);
			return more;
		}

		protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
		{
            return p is CustomLayoutParams;
		}

		protected override LayoutParams GenerateDefaultLayoutParams()
		{
            return new CustomLayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
		}

		public override LayoutParams GenerateLayoutParams(IAttributeSet attributeSet)
		{
            return new CustomLayoutParams(this.Context, attributeSet);
		}

		protected override LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
		{
            return new CustomLayoutParams(p);
		}

		private void DrawDebugInfo(Canvas canvas, View child)
		{
			if (!this.config.DebugDraw)
			{
				return;
			}

			Paint childPaint = this.CreatePaint(Color.ParseColor("#ffffff00"));
			Paint newLinePaint = this.CreatePaint(Color.ParseColor("#ffff0000"));

            CustomLayoutParams lp = (CustomLayoutParams)child.LayoutParameters;

			if (lp.RightMargin > 0)
			{
				float x = child.Right;
				float y = child.Top + child.Height / 2.0f;
				canvas.DrawLine(x, y, x + lp.RightMargin, y, childPaint);
				canvas.DrawLine(x + lp.RightMargin - 4.0f, y - 4.0f, x + lp.RightMargin, y, childPaint);
				canvas.DrawLine(x + lp.RightMargin - 4.0f, y + 4.0f, x + lp.RightMargin, y, childPaint);
			}

			if (lp.LeftMargin > 0)
			{
				float x = child.Left;
				float y = child.Top + child.Height / 2.0f;
				canvas.DrawLine(x, y, x - lp.LeftMargin, y, childPaint);
				canvas.DrawLine(x - lp.LeftMargin + 4.0f, y - 4.0f, x - lp.LeftMargin, y, childPaint);
				canvas.DrawLine(x - lp.LeftMargin + 4.0f, y + 4.0f, x - lp.LeftMargin, y, childPaint);
			}

			if (lp.BottomMargin > 0)
			{
				float x = child.Left + child.Width / 2.0f;
				float y = child.Bottom;
				canvas.DrawLine(x, y, x, y + lp.BottomMargin, childPaint);
				canvas.DrawLine(x - 4.0f, y + lp.BottomMargin - 4.0f, x, y + lp.BottomMargin, childPaint);
				canvas.DrawLine(x + 4.0f, y + lp.BottomMargin - 4.0f, x, y + lp.BottomMargin, childPaint);
			}

			if (lp.TopMargin > 0)
			{
				float x = child.Left + child.Width / 2.0f;
				float y = child.Top;
				canvas.DrawLine(x, y, x, y - lp.TopMargin, childPaint);
				canvas.DrawLine(x - 4.0f, y - lp.TopMargin + 4.0f, x, y - lp.TopMargin, childPaint);
				canvas.DrawLine(x + 4.0f, y - lp.TopMargin + 4.0f, x, y - lp.TopMargin, childPaint);
			}

			if (lp.newLine)
			{
				if (this.config.Orientation == HORIZONTAL)
				{
					float x = child.Left;
					float y = child.Top + child.Height / 2.0f;
					canvas.DrawLine(x, y - 6.0f, x, y + 6.0f, newLinePaint);
				}
				else
				{
					float x = child.Left + child.Width / 2.0f;
					float y = child.Top;
					canvas.DrawLine(x - 6.0f, y, x + 6.0f, y, newLinePaint);
				}
			}
		}

		private Paint CreatePaint(Color color)
		{
			Paint paint = new Paint();
			paint.AntiAlias = true;
			paint.Color = color;
			paint.StrokeWidth = 2.0f;

			return paint;
		}

		public virtual int Orientation
		{
			get
			{
				return this.config.Orientation;
			}
			set
			{
				this.config.Orientation = value;
				this.RequestLayout();
			}
		}


		public virtual bool DebugDraw
		{
			get
			{
				return this.config.DebugDraw;
			}
			set
			{
				this.config.DebugDraw = value;
				this.Invalidate();
			}
		}


		public virtual float WeightDefault
		{
			get
			{
				return this.config.WeightDefault;
			}
			set
			{
				this.config.WeightDefault = value;
				this.RequestLayout();
			}
		}


		public GravityFlags Gravity
		{
			get
			{
				return this.config.Gravity;
			}
			set
			{
				this.config.Gravity = value;
				this.RequestLayout();
			}
		}


		public int Direction
		{
			get
			{
				if (this.config == null)
				{
					// Workaround for android sdk that wants to use virtual methods within constructor.
					return LAYOUT_DIRECTION_LTR;
				}
    
				return this.config.Direction;
			}
			set
			{
				this.config.Direction = value;
				this.RequestLayout();
			}
		}


		public class CustomLayoutParams : MarginLayoutParams
		{
			public bool newLine = false;
            public GravityFlags gravity = GravityFlags.NoGravity;
			public float weight = -1.0f;

            public int spacingLength;
            public int spacingThickness;
            public int inlineStartLength;
            public int length;
            public int thickness;
            public int inlineStartThickness;
            public int x;
            public int y;

			public CustomLayoutParams(Context context, IAttributeSet attributeSet)
                : base(context, attributeSet)
			{
				this.ReadStyleParameters(context, attributeSet);
			}

			public CustomLayoutParams(int width, int height) 
                : base(width, height)
			{
			}

            public CustomLayoutParams(ViewGroup.LayoutParams layoutParams)
                : base(layoutParams)
			{
			}

			public bool GravitySpecified()
			{
				return this.gravity != GravityFlags.NoGravity;
			}

			public bool WeightSpecified()
			{
				return this.weight >= 0;
			}

            public void ReadStyleParameters(Context context, IAttributeSet attributeSet)
			{
				TypedArray a = context.ObtainStyledAttributes(attributeSet, Resource.Styleable.FlowLayout_LayoutParams);
				try
				{
					this.newLine = a.GetBoolean(Resource.Styleable.FlowLayout_LayoutParams_layout_newLine, false);
					this.gravity = (GravityFlags)a.GetInt(Resource.Styleable.FlowLayout_LayoutParams_android_layout_gravity, (int)GravityFlags.NoGravity);
					this.weight = a.GetFloat(Resource.Styleable.FlowLayout_LayoutParams_layout_weight, -1.0f);
				}
				finally
				{
					a.Recycle();
				}
			}

            public void SetPosition(int x, int y)
			{
				this.x = x;
				this.y = y;
			}

            public int InlineStartLength
			{
				get
				{
					return inlineStartLength;
				}
				set
				{
					this.inlineStartLength = value;
				}
			}

            public int Length
			{
				get
				{
					return length;
				}
				set
				{
					this.length = value;
				}
			}

            public int Thickness
			{
				get
				{
					return thickness;
				}
				set
				{
					this.thickness = value;
				}
			}

            public int InlineStartThickness
			{
				get
				{
					return inlineStartThickness;
				}
				set
				{
					this.inlineStartThickness = value;
				}
			}

            public int SpacingLength
			{
				get
				{
					return spacingLength;
				}
			}

            public int SpacingThickness
			{
				get
				{
					return spacingThickness;
				}
			}

            public void ClearCalculatedFields(int orientation)
			{
				if (orientation == FlowLayout.HORIZONTAL)
				{
					this.spacingLength = this.LeftMargin + this.RightMargin;
					this.spacingThickness = this.TopMargin + this.BottomMargin;
				}
				else
				{
					this.spacingLength = this.TopMargin + this.BottomMargin;
					this.spacingThickness = this.LeftMargin + this.RightMargin;
				}
			}
		}
	}
}