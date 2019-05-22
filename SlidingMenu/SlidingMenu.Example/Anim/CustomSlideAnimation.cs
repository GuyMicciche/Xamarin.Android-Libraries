using Android.App;
using Android.Graphics;
using Android.Views.Animations;
using SlidingMenuLibrary;
using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace Example.Anim
{
    [Activity(Label = "Slide Animation", Theme = "@style/ExampleTheme")]
    public class CustomSlideAnimation : CustomAnimation
    {
        private static IInterpolator interp = new CanvasTransformerInterpolator();
        private class CanvasTransformerInterpolator : Java.Lang.Object, IInterpolator
        {
            public float GetInterpolation(float t)
            {
                t -= 1.0f;
                return t * t * t + 1.0f;
            }
        }

        public CustomSlideAnimation()
            : base(Resource.String.anim_slide)
        {
            Transformer = new SlideTransformer();
        }

        public class SlideTransformer : ICanvasTransformer
        {
            public void TransformCanvas(Canvas canvas, float percentOpen)
            {
                canvas.Translate(0, canvas.Height * (1 - interp.GetInterpolation(percentOpen)));
            }
        }
    }
}