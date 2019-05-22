using Android.Graphics;
using Android.Views.Animations;

using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace SlidingMenuLibrary
{
	public class CanvasTransformerBuilder
	{
		private ICanvasTransformer mTrans;

		private static IInterpolator interp = new CanvasTransformerInterpolator();

		private class CanvasTransformerInterpolator : Java.Lang.Object, IInterpolator
		{
			public virtual float GetInterpolation(float t)
			{
				return t;
			}
		}

		private void InitTransformer()
		{
			if (mTrans == null)
			{
				mTrans = new InitCanvasTransformer();
			}
		}

		private class InitCanvasTransformer : ICanvasTransformer
		{
			public virtual void TransformCanvas(Canvas canvas, float percentOpen)
			{

			}
		}

		public virtual ICanvasTransformer Zoom(int openedX, int closedX, int openedY, int closedY, int px, int py)
		{
			return Zoom(openedX, closedX, openedY, closedY, px, py, interp);
		}

		public virtual ICanvasTransformer Zoom(int openedX, int closedX, int openedY, int closedY, int px, int py, IInterpolator interp)
		{
			InitTransformer();
			mTrans = new ZoomCanvasTransformer(this, openedX, closedX, openedY, closedY, px, py, interp);
			return mTrans;
		}

		private class ZoomCanvasTransformer : ICanvasTransformer
		{
			private readonly CanvasTransformerBuilder builder;

			private int openedX;
			private int closedX;
			private int openedY;
			private int closedY;
			private int px;
			private int py;
			private IInterpolator interp;

            public ZoomCanvasTransformer(CanvasTransformerBuilder builder, int openedX, int closedX, int openedY, int closedY, int px, int py, IInterpolator interp)
			{
                this.builder = builder;
				this.openedX = openedX;
				this.closedX = closedX;
				this.openedY = openedY;
				this.closedY = closedY;
				this.px = px;
				this.py = py;
				this.interp = interp;
			}

			public virtual void TransformCanvas(Canvas canvas, float percentOpen)
			{
				builder.mTrans.TransformCanvas(canvas, percentOpen);
				float f = interp.GetInterpolation(percentOpen);
				canvas.Scale((openedX - closedX) * f + closedX, (openedY - closedY) * f + closedY, px, py);
			}
		}

		public virtual ICanvasTransformer Rotate(int openedDeg, int closedDeg, int px, int py)
		{
            return Rotate(openedDeg, closedDeg, px, py, interp);
		}

		public virtual ICanvasTransformer Rotate(int openedDeg, int closedDeg, int px, int py, IInterpolator interp)
		{
			InitTransformer();
			mTrans = new RotateCanvasTransformer(this, openedDeg, closedDeg, px, py, interp);
			return mTrans;
		}

		private class RotateCanvasTransformer : ICanvasTransformer
		{
			private readonly CanvasTransformerBuilder builder;

			private int openedDeg;
			private int closedDeg;
			private int px;
			private int py;
			private IInterpolator interp;

            public RotateCanvasTransformer(CanvasTransformerBuilder builder, int openedDeg, int closedDeg, int px, int py, IInterpolator interp)
			{
                this.builder = builder;
				this.openedDeg = openedDeg;
				this.closedDeg = closedDeg;
				this.px = px;
				this.py = py;
				this.interp = interp;
			}

			public virtual void TransformCanvas(Canvas canvas, float percentOpen)
			{
				builder.mTrans.TransformCanvas(canvas, percentOpen);
				float f = interp.GetInterpolation(percentOpen);
				canvas.Rotate((openedDeg - closedDeg) * f + closedDeg, px, py);
			}
		}

		public virtual ICanvasTransformer Translate(int openedX, int closedX, int openedY, int closedY)
		{
            return Translate(openedX, closedX, openedY, closedY, interp);
		}

		public virtual ICanvasTransformer Translate(int openedX, int closedX, int openedY, int closedY, IInterpolator interp)
		{
			InitTransformer();
			mTrans = new TranslateCanvasTransformer(this, openedX, closedX, openedY, closedY, interp);
			return mTrans;
		}

		private class TranslateCanvasTransformer : ICanvasTransformer
		{
			private readonly CanvasTransformerBuilder builder;

			private int openedX;
			private int closedX;
			private int openedY;
			private int closedY;
			private IInterpolator interp;

            public TranslateCanvasTransformer(CanvasTransformerBuilder builder, int openedX, int closedX, int openedY, int closedY, IInterpolator interp)
			{
                this.builder = builder;
				this.openedX = openedX;
				this.closedX = closedX;
				this.openedY = openedY;
				this.closedY = closedY;
				this.interp = interp;
			}

			public virtual void TransformCanvas(Canvas canvas, float percentOpen)
			{
                builder.mTrans.TransformCanvas(canvas, percentOpen);
				float f = interp.GetInterpolation(percentOpen);
				canvas.Translate((openedX - closedX) * f + closedX, (openedY - closedY) * f + closedY);
			}
		}

		public virtual ICanvasTransformer ConcatTransformer(ICanvasTransformer t)
		{
			InitTransformer();
			mTrans = new ConcatCanvasTransformer(this, t);
			return mTrans;
		}

		private class ConcatCanvasTransformer : ICanvasTransformer
		{
            private readonly CanvasTransformerBuilder builder;

			private ICanvasTransformer t;

            public ConcatCanvasTransformer(CanvasTransformerBuilder builder, ICanvasTransformer t)
			{
                this.builder = builder;
				this.t = t;
			}

			public virtual void TransformCanvas(Canvas canvas, float percentOpen)
			{
                builder.mTrans.TransformCanvas(canvas, percentOpen);
				t.TransformCanvas(canvas, percentOpen);
			}
		}
	}
}