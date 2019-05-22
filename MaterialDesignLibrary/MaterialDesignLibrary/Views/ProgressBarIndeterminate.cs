using Android.Content;
using Android.Util;
using Android.Views.Animations;
using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views;
namespace MaterialDesignLibrary
{
	public class ProgressBarIndeterminate : ProgressBarDeterminate
	{

        public ProgressBarIndeterminate(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Post(() =>
                {
                    Progress = 60;
                    Animation anim = AnimationUtils.LoadAnimation(Context, Resource.Animation.progress_indeterminate_animation);
                    progressView.StartAnimation(anim);
                    ObjectAnimator anim2 = ObjectAnimator.OfFloat(progressView, "x", Width);
                    anim2.SetDuration(1200);
                    anim2.AddListener(new MyAnimatorListener(this));

                    anim2.Start();
                });
        }

		private class MyAnimatorListener : Java.Lang.Object, Animator.IAnimatorListener
		{
			private readonly ProgressBarIndeterminate progress;

            public MyAnimatorListener(ProgressBarIndeterminate progress)
			{
                this.progress = progress;
				cont = 1;
				suma = 1;
				duration = 1200;
			}

			internal int cont;
			internal int suma;
			internal int duration;

			public virtual void OnAnimationEnd(Animator arg0)
			{
				// Repeat animation
				ViewHelper.SetX(progress.progressView,-progress.progressView.Width / 2);
				cont += suma;
                ObjectAnimator anim2Repeat = ObjectAnimator.OfFloat(progress.progressView, "x", progress.Width);
				anim2Repeat.SetDuration(duration / cont);
				anim2Repeat.AddListener(this);
				anim2Repeat.Start();
				if (cont == 3 || cont == 1)
				{
					suma *= -1;
				}
			}

			public virtual void OnAnimationStart(Animator arg0)
			{
			}
			public virtual void OnAnimationRepeat(Animator arg0)
			{
			}
			public virtual void OnAnimationCancel(Animator arg0)
			{
			}
		}
	}
}