using Android.App;
using Android.Net;
using Android.OS;
using Android.Support.V4.App;
using Android.Text;
using Android.Views;
using Android.Widget;
using System;

namespace ActionsContentView.Example
{
    [Activity(Label = "Effects Example")]
    public class EffectsExampleActivity : FragmentActivity
    {
        private const string STATE_POSITION = "state:layout_id";

        private const string SCHEME = "settings";
        private const string AUTHORITY = "effects";
        public static Android.Net.Uri URI = new Android.Net.Uri.Builder().Scheme(SCHEME).Authority(AUTHORITY).Build();

        private EffectsAdapter mAdapter;

        private ListView viewList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            mAdapter = new EffectsAdapter(this);

            int selectedPosition;
            if (savedInstanceState != null)
            {
                selectedPosition = savedInstanceState.GetInt(STATE_POSITION, 0);
            }
            else
            {
                selectedPosition = 0;
            }

            Init(selectedPosition);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);

            int position = viewList.SelectedItemPosition;
            if (position != ListView.InvalidPosition)
            {
                outState.PutInt(STATE_POSITION, position);
            }
        }

        private void Init(int position)
        {
            int layoutId = mAdapter.GetItemAtPosition(position);
            SetContentView(layoutId);

            string titleText = GetString(Resource.String.action_effects);
            TextView title = FindViewById<TextView>(Android.Resource.Id.Text1);
            title.Text = titleText.ToUpper();

            ActionsContentView viewActionsContentView = FindViewById<ActionsContentView>(Resource.Id.actionsContentView);
            // The Android way
            //viewActionsContentView.OnActionsContentListener = new MyOnActionsContentListener();
            
            // OR
            // I ADDED THIS
            // The .NET way
            viewActionsContentView.ContentStateChanged += viewActionsContentView_ContentStateChanged;

            TextView name = FindViewById<TextView>(Resource.Id.effect_name);
            name.Text = mAdapter.GetEffectTitle(position);

            TextView actions = FindViewById<TextView>(Resource.Id.actions_html);
            string actionsHtml = mAdapter.GetActionsHtml(position);
            if (!TextUtils.IsEmpty(actionsHtml))
            {
                FindViewById(Resource.Id.effect_actions_layout).Visibility = ViewStates.Visible;
                actions.Text = Html.FromHtml(actionsHtml).ToString();
            }

            TextView content = FindViewById<TextView>(Resource.Id.content_html);
            string contentHtml = mAdapter.GetContentHtml(position);
            if (!TextUtils.IsEmpty(contentHtml))
            {
                FindViewById(Resource.Id.effect_content_layout).Visibility = ViewStates.Visible;
                content.Text = Html.FromHtml(contentHtml).ToString();
            }

            viewList = FindViewById<ListView>(Resource.Id.actions);
            viewList.Adapter = mAdapter;
            viewList.ItemClick += (sender, e) =>
                {
                    Init(e.Position);
                };
        }

        void viewActionsContentView_ContentStateChanged(ActionsContentView view, bool isContentShown)
        {
            Console.WriteLine(".NET -> ContentStateChanged");
            view.ContentController.IgnoreTouchEvents = !isContentShown;
        }

        private class MyOnActionsContentListener : Java.Lang.Object, ActionsContentView.IOnActionsContentListener
        {
            public void OnContentStateChanged(ActionsContentView v, bool isContentShown)
            {
                Console.WriteLine("ContentStateChanged");
                v.ContentController.IgnoreTouchEvents = !isContentShown;
            }

            public void OnContentStateInAction(ActionsContentView v, bool isContentShowing)
            {
            }
        }
    }
}