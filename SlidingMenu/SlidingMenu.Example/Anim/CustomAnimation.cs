using Android.OS;
using SlidingMenuLibrary;

using ICanvasTransformer = SlidingMenuLibrary.SlidingMenu.ICanvasTransformer;

namespace Example.Anim
{
    public abstract class CustomAnimation : BaseActivity
    {
        public ICanvasTransformer Transformer { get; set; }

        protected CustomAnimation(int titleRes)
            : base(titleRes)
        { }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.content_frame);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, new SampleListFragment())
                .Commit();

            SlidingMenu sm = GetSlidingMenu();
            SlidingActionBarEnabled = true;
            sm.BehindScrollScale = 0.0f;
            sm.BehindCanvasTransformer = Transformer;
        }
    }
}