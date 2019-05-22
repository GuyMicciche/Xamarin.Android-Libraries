using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using System;

namespace MaterialDialogs
{
	/// <summary>
	/// @author Aidan Follestad (afollestad)
	/// </summary>
	public class DialogUtils
	{
		public static int AdjustAlpha(int color, float factor)
		{
			int alpha = (int)Math.Round((Color.GetAlphaComponent(color) * factor));
			int red = Color.GetRedComponent(color);
			int green = Color.GetGreenComponent(color);
			int blue = Color.GetBlueComponent(color);
			return Color.Argb(alpha, red, green, blue);
		}

		public static Color ResolveColor(Context context, int attr)
		{
			return ResolveColor(context, attr, 0);
		}

		public static Color ResolveColor(Context context, int attr, int fallback)
		{
			TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ attr });
			try
			{
				return a.GetColor(0, fallback);
			}
			finally
			{
				a.Recycle();
			}
		}

		public static Drawable ResolveDrawable(Context context, int attr)
		{
			return ResolveDrawable(context, attr, null);
		}

		private static Drawable ResolveDrawable(Context context, int attr, Drawable fallback)
		{
			TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ attr });
			try
			{
				Drawable d = a.GetDrawable(0);
				if (d == null && fallback != null)
				{
					d = fallback;
				}
				return d;
			}
			finally
			{
				a.Recycle();
			}
		}

		public static int ResolveDimension(Context context, int attr)
		{
			return ResolveDimension(context, attr, -1);
		}

		public static int ResolveDimension(Context context, int attr, int fallback)
		{
			TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ attr });
			try
			{
				return a.GetDimensionPixelSize(0, fallback);
			}
			finally
			{
				a.Recycle();
			}
		}

		public static bool ResolveBoolean(Context context, int attr)
		{
			TypedArray a = context.Theme.ObtainStyledAttributes(new int[]{ attr });
			try
			{
				return a.GetBoolean(0, false);
			}
			finally
			{
				a.Recycle();
			}
		}
	}
}