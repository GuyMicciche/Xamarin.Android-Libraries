using Android.App;
using Android.OS;

using SlidingMenuLibrary;

namespace Example
{
    [Activity(Label = "Sliding Content", Theme = "@style/ExampleTheme")]
    public class SlidingContent : BaseActivity
    {
        public SlidingContent() 
            : base(Resource.String.title_bar_content)
        { }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.content_frame);
            SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.content_frame, new SampleListFragment())
                .Commit();
            
            SlidingActionBarEnabled = false;
        }
    }
}