using Android.Content;
using Android.Util;

using Xamarin.NineOldAndroids.Animations;
using Xamarin.NineOldAndroids.Views;

namespace MaterialDesignLibrary
{
	public class ProgressBarIndeterminateDeterminate : ProgressBarDeterminate
	{

		internal bool firstProgress = true;
		internal bool runAnimation = true;
		internal new ObjectAnimator animation;
		
        public ProgressBarIndeterminateDeterminate(Context context, IAttributeSet attrs) 
            : base(context, attrs)
		{
            Post(() =>
                {
                    Progress = 60;
                    ViewHelper.SetX(progressView, Width + progressView.Width / 2);
                    animation = ObjectAnimator.OfFloat(progressView, "x", -progressView.Width / 2);
                    animation.SetDuration(1200);
                    animation.AddListener(new MyAnimatorListener(this));
                    animation.Start();
                });
		}

		private class MyAnimatorListener : Java.Lang.Object, Animator.IAnimatorListener
		{
			private readonly ProgressBarIndeterminateDeterminate progress;

            public MyAnimatorListener(ProgressBarIndeterminateDeterminate progress)
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
				if (progress.runAnimation)
				{
				    ViewHelper.SetX(progress.progressView, progress.Width + progress.progressView.Width / 2);
				    cont += suma;
				    progress.animation = ObjectAnimator.OfFloat(progress.progressView, "x", -progress.progressView.Width / 2);
				    progress.animation.SetDuration(duration / cont);
				    progress.animation.AddListener(this);
				    progress.animation.Start();
				    if (cont == 3 || cont == 1)
				    {
					    suma *= -1;
				    }
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

		public override int Progress
		{
			set
			{
				if (firstProgress)
				{
					firstProgress = false;
				}
				else
				{
					StopIndeterminate();
				}
				base.Progress = value;
			}
		}

		/// <summary>
		/// Stop indeterminate animation to convert view in determinate progress bar
		/// </summary>
		private void StopIndeterminate()
		{
			animation.Cancel();
			ViewHelper.SetX(progressView, 0);
			runAnimation = false;
		}
	}
}