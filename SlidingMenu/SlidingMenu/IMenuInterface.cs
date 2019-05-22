using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;

namespace SlidingMenuLibrary
{
	public interface IMenuInterface
	{
		void ScrollBehindTo(int x, int y, CustomViewBehind cvb, float scrollScale);
		int GetMenuLeft(CustomViewBehind cvb, View content);
		int GetAbsLeftBound(CustomViewBehind cvb, View content);
		int GetAbsRightBound(CustomViewBehind cvb, View content);
		bool MarginTouchAllowed(View content, int x, int threshold);
		bool MenuOpenTouchAllowed(View content, int currPage, int x);
		bool MenuTouchInQuickReturn(View content, int currPage, int x);
		bool MenuClosedSlideAllowed(int x);
		bool MenuOpenSlideAllowed(int x);
		void DrawShadow(Canvas canvas, Drawable shadow, int width);
		void DrawFade(Canvas canvas, int alpha, CustomViewBehind cvb, View content);
		void DrawSelector(View content, Canvas canvas, float percentOpen);
	}
}