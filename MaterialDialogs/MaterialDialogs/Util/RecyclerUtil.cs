using Android.Support.V7.Widget;
using Android.Views;
namespace MaterialDialogs
{
	public class RecyclerUtil
	{
		public static bool CanRecyclerViewScroll(View view)
		{
			RecyclerView rv = (RecyclerView) view;

			RecyclerView.LayoutManager lm = rv.GetLayoutManager();
			int count = rv.GetAdapter().ItemCount;
			int lastVisible = -1;

			if (lm is LinearLayoutManager)
			{
				LinearLayoutManager llm = (LinearLayoutManager) lm;
				lastVisible = llm.FindLastVisibleItemPosition();
			}
			else if (lm is GridLayoutManager)
			{
				GridLayoutManager glm = (GridLayoutManager) lm;
				lastVisible = glm.FindLastVisibleItemPosition();
			}
			else
			{
				//throw new MaterialDialog.NotImplementedException("Material Dialogs currently only supports LinearLayoutManager and GridLayoutManager. Please report any new layout managers.");
                throw new System.Exception("Material Dialogs currently only supports LinearLayoutManager and GridLayoutManager. Please report any new layout managers.");
			}

			if (lastVisible == -1)
			{
				return false;
			}
			/* We scroll if the last item is not visible */
			bool lastItemVisible = lastVisible == count - 1;
			return !lastItemVisible || rv.GetChildAt(rv.ChildCount - 1).Bottom > rv.Height - rv.PaddingBottom;
		}

		public static bool IsRecyclerView(View view)
		{
			return view is RecyclerView;
		}
	}
}