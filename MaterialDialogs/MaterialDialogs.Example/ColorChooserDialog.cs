using Android.App;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using DialogFragment = Android.Support.V4.App.DialogFragment;

namespace MaterialDialogs.Example
{
	/// <summary>
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	public class ColorChooserDialog : DialogFragment, View.IOnClickListener
	{

		private ICallback mCallback;
		private Color[] mColors;

		public void OnClick(View v)
		{
			if (v.Tag != null)
			{
				int index = (int)v.Tag;
				mCallback.OnColorSelection(index, mColors[index], ShiftColor(mColors[index]));
				Dismiss();
			}
		}

		public interface ICallback
		{
			void OnColorSelection(int index, Color color, Color darker);
		}

		public ColorChooserDialog()
		{
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			MaterialDialog dialog = (new MaterialDialog.Builder(Activity)).SetTitle(Resource.String.color_chooser).SetAutoDismiss(false).SetCustomView(Resource.Layout.dialog_color_chooser, false).Build();

			TypedArray ta = Activity.Resources.ObtainTypedArray(Resource.Array.colors);
			mColors = new Color[ta.Length()];
			for (int i = 0; i < ta.Length(); i++)
			{
				mColors[i] = ta.GetColor(i, 0);
			}
			ta.Recycle();
			GridLayout list = (GridLayout) dialog.CustomView.FindViewById(Resource.Id.grid);
			int preselect = Arguments.GetInt("preselect", -1);

			for (int i = 0; i < list.ChildCount; i++)
			{
				FrameLayout child = (FrameLayout) list.GetChildAt(i);
				child.Tag = i;
				child.SetOnClickListener(this);
                child.GetChildAt(0).Visibility = preselect == i ? ViewStates.Visible : ViewStates.Gone;

				Drawable selector = CreateSelector(mColors[i]);
				if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
				{
					int[][] states = new int[][]{ new int[]{ -Android.Resource.Attribute.StatePressed }, new int[]{ Android.Resource.Attribute.StatePressed } };
					int[] colors = new int[]{ ShiftColor(mColors[i]), mColors[i] };
					ColorStateList rippleColors = new ColorStateList(states, colors);
					SetBackgroundCompat(child, new RippleDrawable(rippleColors, selector, null));
				}
				else
				{
					SetBackgroundCompat(child, selector);
				}


			}
			return dialog;
		}

		private void SetBackgroundCompat(View view, Drawable d)
		{
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
			{
				view.Background = d;
			}
			else
			{
				view.SetBackgroundDrawable(d);
			}
		}

		private Color ShiftColor(Color color)
		{
			float[] hsv = new float[3];
			Color.ColorToHSV(color, hsv);
			hsv[2] *= 0.9f; // value component
			return Color.HSVToColor(hsv);
		}

		private Drawable CreateSelector(Color color)
		{
			ShapeDrawable coloredCircle = new ShapeDrawable(new OvalShape());
			coloredCircle.Paint.Color = color;
			ShapeDrawable darkerCircle = new ShapeDrawable(new OvalShape());
			darkerCircle.Paint.Color = ShiftColor(color);

			StateListDrawable stateListDrawable = new StateListDrawable();
			stateListDrawable.AddState(new int[]{ -Android.Resource.Attribute.StatePressed }, coloredCircle);
            stateListDrawable.AddState(new int[] { Android.Resource.Attribute.StatePressed }, darkerCircle);
			return stateListDrawable;
		}

		public virtual void Show(ActionBarActivity context, int preselect, ICallback callback)
		{
			mCallback = callback;
			Bundle args = new Bundle();
			args.PutInt("preselect", preselect);
			Arguments = args;
			Show(context.SupportFragmentManager, "COLOR_SELECTOR");
		}
	}
}