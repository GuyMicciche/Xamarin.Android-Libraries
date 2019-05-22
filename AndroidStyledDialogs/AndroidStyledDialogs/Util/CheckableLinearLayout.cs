using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace AndroidStyledDialogs
{
	public class CheckableLinearLayout : LinearLayout, ICheckable
	{
		private static readonly int[] CHECKED_STATE_SET = new int[] { Android.Resource.Attribute.StateChecked };

        private HashSet<ICheckable> mCheckablesSet = new HashSet<ICheckable>();
		private bool mChecked;

		public CheckableLinearLayout(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		protected override void OnFinishInflate()
		{
			base.OnFinishInflate();
			// find checkable items
			int childCount = ChildCount;
			for (int i = 0; i < childCount; ++i)
			{
				View v = GetChildAt(i);
                if (v is ICheckable)
				{
                    mCheckablesSet.Add((ICheckable)v);
				}
			}
		}

		public bool Checked
		{
			get
			{
				return mChecked;
			}
			set
			{
				if (value == this.mChecked)
				{
					return;
				}
				this.mChecked = value;
				foreach (ICheckable checkable in mCheckablesSet)
				{
					checkable.Checked = value;
				}
				RefreshDrawableState();
			}
		}

		public void Toggle()
		{
			Checked = !mChecked;
		}

		public virtual int[] OnCreateDrawableState(int extraSpace)
		{
			int[] drawableState = base.OnCreateDrawableState(extraSpace + 1);
			if (Checked)
			{
				MergeDrawableStates(drawableState, CHECKED_STATE_SET);
			}
			return drawableState;
		}
	}
}