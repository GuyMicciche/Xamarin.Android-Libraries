using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using System;

namespace AndroidFlowLayout
{
	public class LayoutConfiguration
	{
		private int orientation = FlowLayout.HORIZONTAL;
		private bool debugDraw = false;
		private float weightDefault = 0;
        private GravityFlags gravity = GravityFlags.Left | GravityFlags.Top;
		private int layoutDirection = FlowLayout.LAYOUT_DIRECTION_LTR;

		public LayoutConfiguration(Context context, IAttributeSet attributeSet)
		{
			TypedArray a = context.ObtainStyledAttributes(attributeSet, Resource.Styleable.FlowLayout);
			try
			{
				this.Orientation = a.GetInteger(Resource.Styleable.FlowLayout_android_orientation, FlowLayout.HORIZONTAL);
				this.DebugDraw = a.GetBoolean(Resource.Styleable.FlowLayout_debugDraw, false);
				this.WeightDefault = a.GetFloat(Resource.Styleable.FlowLayout_weightDefault, 0.0f);
				this.Gravity = (GravityFlags)a.GetInteger(Resource.Styleable.FlowLayout_android_gravity, (int)GravityFlags.NoGravity);
				this.Direction = a.GetInteger(Resource.Styleable.FlowLayout_layoutDirection, FlowLayout.LAYOUT_DIRECTION_LTR);
			}
			finally
			{
				a.Recycle();
			}
		}

		public int Orientation
		{
			get
			{
				return this.orientation;
			}
			set
			{
				if (value == FlowLayout.VERTICAL)
				{
					this.orientation = value;
				}
				else
				{
					this.orientation = FlowLayout.HORIZONTAL;
				}
			}
		}

		public bool DebugDraw
		{
			get
			{
				return this.debugDraw;
			}
			set
			{
				this.debugDraw = value;
			}
		}

		public float WeightDefault
		{
			get
			{
				return this.weightDefault;
			}
			set
			{
				this.weightDefault = Math.Max(0, value);
			}
		}

        public GravityFlags Gravity
		{
			get
			{
				return this.gravity;
			}
			set
			{
				if ((value & GravityFlags.HorizontalGravityMask) == 0)
				{
					value |= GravityFlags.Left;
				}
    
				if ((value & GravityFlags.VerticalGravityMask) == 0)
				{
					value |= GravityFlags.Top;
				}
    
				this.gravity = value;
			}
		}

		public int Direction
		{
			get
			{
				return layoutDirection;
			}
			set
			{
				if (value == FlowLayout.LAYOUT_DIRECTION_RTL)
				{
					this.layoutDirection = value;
				}
				else
				{
					this.layoutDirection = FlowLayout.LAYOUT_DIRECTION_LTR;
				}
			}
		}
	}
}