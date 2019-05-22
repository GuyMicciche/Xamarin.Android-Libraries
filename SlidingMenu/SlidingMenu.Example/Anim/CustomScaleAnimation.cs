using Android.App;
using Android.Graphics;
using SlidingMenuLibrary;
using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace Example.Anim
{
    [Activity(Label = "Scale Animation", Theme = "@style/ExampleTheme")]
    public class CustomScaleAnimation : CustomAnimation
    {
        public CustomScaleAnimation() 
            : base(Resource.String.anim_scale)
        {
            Transformer = new ScaleTransformer();
        }

        public class ScaleTransformer : ICanvasTransformer
        {
            public void TransformCanvas(Canvas canvas, float percentOpen)
            {
                canvas.Scale(percentOpen, 1, 0, 0);
            }
        }
    }
}