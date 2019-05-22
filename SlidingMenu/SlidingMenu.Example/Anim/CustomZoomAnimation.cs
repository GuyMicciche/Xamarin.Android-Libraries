using Android.App;
using Android.Graphics;
using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace Example.Anim
{
    [Activity(Label = "Zoom Animation", Theme = "@style/ExampleTheme")]
    public class CustomZoomAnimation : CustomAnimation
    {
        public CustomZoomAnimation()
            : base(Resource.String.anim_zoom)
        {
            Transformer = new ZoomTransformer();
        }

        public class ZoomTransformer : ICanvasTransformer
        {
            public void TransformCanvas(Canvas canvas, float percentOpen)
            {
                float scale = (float)(percentOpen * 0.25 + 0.75);
                canvas.Scale(scale, scale, canvas.Width / 2, canvas.Height / 2);
            }
        }
    }
}