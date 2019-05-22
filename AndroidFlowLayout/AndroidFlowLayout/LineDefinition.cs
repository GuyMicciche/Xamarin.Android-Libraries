using Android.Views;
using System;
using System.Collections.Generic;

namespace AndroidFlowLayout
{
	public class LineDefinition
	{
		private readonly IList<View> views = new List<View>();
		private readonly LayoutConfiguration config;
		private readonly int maxLength;
		private int lineLength;
		private int lineThickness;
		private int lineLengthWithSpacing;
		private int lineThicknessWithSpacing;
		private int lineStartThickness;
		private int lineStartLength;

		public LineDefinition(int maxLength, LayoutConfiguration config)
		{
			this.lineStartThickness = 0;
			this.lineStartLength = 0;
			this.maxLength = maxLength;
			this.config = config;
		}

		public void AddView(View child)
		{
			this.AddView(this.views.Count, child);
		}

		public void AddView(int i, View child)
		{
            FlowLayout.CustomLayoutParams lp = (FlowLayout.CustomLayoutParams)child.LayoutParameters;

			this.views.Insert(i, child);

			this.lineLength = this.lineLengthWithSpacing + lp.Length;
			this.lineLengthWithSpacing = this.lineLength + lp.SpacingLength;
			this.lineThicknessWithSpacing = Math.Max(this.lineThicknessWithSpacing, lp.Thickness + lp.SpacingThickness);
			this.lineThickness = Math.Max(this.lineThickness, lp.Thickness);
		}

		public bool CanFit(View child)
		{
			int childLength;
			if (this.config.Orientation == FlowLayout.HORIZONTAL)
			{
				childLength = child.MeasuredWidth;
			}
			else
			{
				childLength = child.MeasuredHeight;
			}
			return lineLengthWithSpacing + childLength <= maxLength;
		}

		public int LineStartThickness
		{
			get
			{
				return lineStartThickness;
			}
		}

		public int LineThickness
		{
			get
			{
				return lineThicknessWithSpacing;
			}
		}

		public int LineLength
		{
			get
			{
				return lineLength;
			}
		}

		public int LineStartLength
		{
			get
			{
				return lineStartLength;
			}
		}

		public IList<View> Views
		{
			get
			{
				return views;
			}
		}

		public int Thickness
		{
			set
			{
				int thicknessSpacing = this.lineThicknessWithSpacing - this.lineThickness;
				this.lineThicknessWithSpacing = value;
				this.lineThickness = value - thicknessSpacing;
			}
		}

		public int Length
		{
			set
			{
				int lengthSpacing = this.lineLengthWithSpacing - this.lineLength;
				this.lineLength = value;
				this.lineLengthWithSpacing = value + lengthSpacing;
			}
		}

		public void AddLineStartThickness(int extraLineStartThickness)
		{
			this.lineStartThickness += extraLineStartThickness;
		}

		public void AddLineStartLength(int extraLineStartLength)
		{
			this.lineStartLength += extraLineStartLength;
		}
	}
}