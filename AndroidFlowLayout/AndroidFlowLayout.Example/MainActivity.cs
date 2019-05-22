using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace AndroidFlowLayout.Example
{
    [Activity(MainLauncher = true)]
    public class MyActivity : Activity
    {
        private FlowLayout layout;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Main);

            layout = (FlowLayout)this.FindViewById(Resource.Id.flowLayout);
            
            Button buttonOrientation = new Button(this);
            buttonOrientation.LayoutParameters = new FlowLayout.LayoutParams(100, 100);
            buttonOrientation.TextSize = 8;
            buttonOrientation.Text = "Switch Orientation (Current: Horizontal)";
            buttonOrientation.Click += buttonOrientation_Click;
            layout.AddView(buttonOrientation, 0);

            Button buttonGravity = new Button(this);
            buttonGravity.LayoutParameters = new FlowLayout.LayoutParams(100, 100);
            buttonGravity.TextSize = 8;
            buttonGravity.Text = "Switch Gravity (Current: FILL)";
            buttonGravity.Click += buttonGravity_Click;
            layout.AddView(buttonGravity, 0);

            Button buttonLayoutDirection = new Button(this);
            buttonLayoutDirection.LayoutParameters = new FlowLayout.LayoutParams(100, 100);
            buttonLayoutDirection.TextSize = 8;
            buttonLayoutDirection.Text = "Switch LayoutDirection (Current: LTR)";
            buttonLayoutDirection.Click += buttonLayoutDirection_Click;
            layout.AddView(buttonLayoutDirection, 0);

            Button buttonDebug = new Button(this);
            buttonDebug.LayoutParameters = new FlowLayout.LayoutParams(100, 100);
            buttonDebug.TextSize = 8;
            buttonDebug.Text = "Switch Debug (Current: true)";
            buttonDebug.Click += buttonDebug_Click;
            layout.AddView(buttonDebug, 0);
        }

        void buttonOrientation_Click(object sender, System.EventArgs e)
        {
            layout.Orientation = 1 - layout.Orientation;
            ((Button)sender).Text = layout.Orientation == FlowLayout.HORIZONTAL ? "Switch Orientation (Current: Horizontal)" : "Switch Orientation (Current: Vertical)";
        }

        void buttonGravity_Click(object sender, System.EventArgs e)
        {
            switch (layout.Gravity)
            {
                case GravityFlags.Left | GravityFlags.Top:
                    layout.Gravity = GravityFlags.Fill;
                    ((Button)sender).Text = "Switch Gravity (Current: FILL)";
                    break;
                case GravityFlags.Fill:
                    layout.Gravity = GravityFlags.Center;
                    ((Button)sender).Text = "Switch Gravity (Current: CENTER)";
                    break;
                case GravityFlags.Center:
                    layout.Gravity = GravityFlags.Right | GravityFlags.Bottom;
                    ((Button)sender).Text = "Switch Gravity (Current: RIGHT | BOTTOM)";
                    break;
                case GravityFlags.Right | GravityFlags.Bottom:
                    layout.Gravity = GravityFlags.Left | GravityFlags.Top;
                    ((Button)sender).Text = "Switch Gravity (Current: LEFT | TOP)";
                    break;
            }
        }

        void buttonLayoutDirection_Click(object sender, System.EventArgs e)
        {
            layout.LayoutDirection = 1 - layout.LayoutDirection;
            ((Button)sender).Text = layout.LayoutDirection == FlowLayout.LAYOUT_DIRECTION_LTR ? "Switch LayoutDirection (Current: LTR)" : "Switch LayoutDirection (Current: RTL)";
        }

        void buttonDebug_Click(object sender, System.EventArgs e)
        {
            layout.DebugDraw = !layout.DebugDraw;
            ((Button)sender).Text = layout.DebugDraw ? "Switch LayoutDirection (Current: true)" : "Switch LayoutDirection (Current: false)";
        }
    }
}