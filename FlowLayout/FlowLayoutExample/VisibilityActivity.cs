using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;

namespace FlowLayout.Example
{
    [Activity(Label = "VisibilityActivity")]
    public class VisibilityActivity : Activity
	{
		private FlowLayout mFlowLayout;

		protected override void OnCreate(Bundle savedInstanceState)
		{
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_visibility);

			mFlowLayout = (FlowLayout)FindViewById(Resource.Id.flow);
		}

		public void AddItem(View view)
		{
			Color color = Resources.GetColor(Resource.Color.holo_blue_dark);

			View newView = new View(this);
			newView.SetBackgroundColor(color);

            FlowLayout.CustomLayoutParams lp = new FlowLayout.CustomLayoutParams(100, 100);
			lp.RightMargin = 10;
			newView.LayoutParameters = lp;

			mFlowLayout.AddView(newView);
		}

		public void RemoveItem(View view)
		{
			mFlowLayout.RemoveView(LastView);
		}

		public void ToggleItem(View view)
		{
			View last = LastView;

			if (last.Visibility == ViewStates.Visible)
			{
                last.Visibility = ViewStates.Gone;
			}
			else
			{
                last.Visibility = ViewStates.Visible;
			}
		}

		private View LastView
		{
			get
			{
				return mFlowLayout.GetChildAt(mFlowLayout.ChildCount - 1);
			}
		}
	}
}