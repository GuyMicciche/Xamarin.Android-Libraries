using Android.Database;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace AndroidStaggeredGrid
{
	/// <summary>
	/// ListAdapter used when a ListView has header views. This ListAdapter
	/// wraps another one and also keeps track of the header views and their
	/// associated data objects.
	/// <p>This is intended as a base class; you will probably not need to
	/// use this class directly in your own code.
	/// </summary>
	public class HeaderViewListAdapter : Java.Lang.Object, IWrapperListAdapter, IFilterable
	{
		private readonly IListAdapter mAdapter;

		// These two ArrayList are assumed to NOT be null.
		// They are indeed created when declared in ListView and then shared.
        public List<StaggeredGridView.FixedViewInfo> mHeaderViewInfos;
        public List<StaggeredGridView.FixedViewInfo> mFooterViewInfos;

		// Used as a placeholder in case the provided info views are indeed null.
		// Currently only used by some CTS tests, which may be removed.
        public static readonly List<StaggeredGridView.FixedViewInfo> EMPTY_INFO_LIST = new List<StaggeredGridView.FixedViewInfo>();

        public bool mAreAllFixedViewsSelectable;

		private readonly bool mIsFilterable;

		public HeaderViewListAdapter(List<StaggeredGridView.FixedViewInfo> headerViewInfos, List<StaggeredGridView.FixedViewInfo> footerViewInfos, IListAdapter adapter)
		{
			mAdapter = adapter;
			mIsFilterable = adapter is IFilterable;

			if (headerViewInfos == null)
			{
				mHeaderViewInfos = EMPTY_INFO_LIST;
			}
			else
			{
				mHeaderViewInfos = headerViewInfos;
			}

			if (footerViewInfos == null)
			{
				mFooterViewInfos = EMPTY_INFO_LIST;
			}
			else
			{
				mFooterViewInfos = footerViewInfos;
			}

			mAreAllFixedViewsSelectable = AreAllListInfosSelectable(mHeaderViewInfos) && AreAllListInfosSelectable(mFooterViewInfos);
		}

		public int HeadersCount
		{
			get
			{
				return mHeaderViewInfos.Count;
			}
		}

		public int FootersCount
		{
			get
			{
				return mFooterViewInfos.Count;
			}
		}

		public bool IsEmpty
		{
			get
			{
				return mAdapter == null || mAdapter.IsEmpty;
			}
		}

		private bool AreAllListInfosSelectable(List<StaggeredGridView.FixedViewInfo> infos)
		{
			if (infos != null)
			{
				foreach (StaggeredGridView.FixedViewInfo info in infos)
				{
					if (!info.IsSelectable)
					{
						return false;
					}
				}
			}
			return true;
		}

		public bool RemoveHeader(View v)
		{
			for (int i = 0; i < mHeaderViewInfos.Count; i++)
			{
				StaggeredGridView.FixedViewInfo info = mHeaderViewInfos[i];
				if (info.view == v)
				{
					mHeaderViewInfos.RemoveAt(i);

					mAreAllFixedViewsSelectable = AreAllListInfosSelectable(mHeaderViewInfos) && AreAllListInfosSelectable(mFooterViewInfos);

					return true;
				}
			}

			return false;
		}

		public bool RemoveFooter(View v)
		{
			for (int i = 0; i < mFooterViewInfos.Count; i++)
			{
				StaggeredGridView.FixedViewInfo info = mFooterViewInfos[i];
				if (info.view == v)
				{
					mFooterViewInfos.RemoveAt(i);

					mAreAllFixedViewsSelectable = AreAllListInfosSelectable(mHeaderViewInfos) && AreAllListInfosSelectable(mFooterViewInfos);

					return true;
				}
			}

			return false;
		}

		public int Count
		{
			get
			{
				if (mAdapter != null)
				{
					return FootersCount + HeadersCount + mAdapter.Count;
				}
				else
				{
					return FootersCount + HeadersCount;
				}
			}
		}

		public bool AreAllItemsEnabled()
		{
			if (mAdapter != null)
			{
				return mAreAllFixedViewsSelectable && mAdapter.AreAllItemsEnabled();
			}
			else
			{
				return true;
			}
		}

		public bool IsEnabled(int position)
		{
			// Header (negative positions will throw an ArrayIndexOutOfBoundsException)
			int numHeaders = HeadersCount;
			if (position < numHeaders)
			{
				return mHeaderViewInfos[position].IsSelectable;
			}

			// Adapter
			int adjPosition = position - numHeaders;
			int adapterCount = 0;
			if (mAdapter != null)
			{
				adapterCount = mAdapter.Count;
				if (adjPosition < adapterCount)
				{
                    return mAdapter.IsEnabled(adjPosition);
				}
			}

			// Footer (off-limits positions will throw an ArrayIndexOutOfBoundsException)
			return mFooterViewInfos[adjPosition - adapterCount].IsSelectable;
		}

		public Java.Lang.Object GetItem(int position)
		{
			// Header (negative positions will throw an ArrayIndexOutOfBoundsException)
			int numHeaders = HeadersCount;
			if (position < numHeaders)
			{
				return mHeaderViewInfos[position].Data;
			}

			// Adapter
			int adjPosition = position - numHeaders;
			int adapterCount = 0;
			if (mAdapter != null)
			{
				adapterCount = mAdapter.Count;
				if (adjPosition < adapterCount)
				{
					return mAdapter.GetItem(adjPosition);
				}
			}

			// Footer (off-limits positions will throw an ArrayIndexOutOfBoundsException)
			return mFooterViewInfos[adjPosition - adapterCount].Data;
		}

		public long GetItemId(int position)
		{
			int numHeaders = HeadersCount;
			if (mAdapter != null && position >= numHeaders)
			{
				int adjPosition = position - numHeaders;
				int adapterCount = mAdapter.Count;
				if (adjPosition < adapterCount)
				{
					return mAdapter.GetItemId(adjPosition);
				}
			}
			return -1;
		}

		public bool HasStableIds
		{
            get
            {
                if (mAdapter != null)
                {
                    return mAdapter.HasStableIds;
                }
                return false;
            }
		}

		public View GetView(int position, View convertView, ViewGroup parent)
		{
			// Header (negative positions will throw an ArrayIndexOutOfBoundsException)
			int numHeaders = HeadersCount;
			if (position < numHeaders)
			{
				return mHeaderViewInfos[position].view;
			}

			// Adapter
			int adjPosition = position - numHeaders;
			int adapterCount = 0;
			if (mAdapter != null)
			{
				adapterCount = mAdapter.Count;
				if (adjPosition < adapterCount)
				{
					return mAdapter.GetView(adjPosition, convertView, parent);
				}
			}

			// Footer (off-limits positions will throw an ArrayIndexOutOfBoundsException)
			return mFooterViewInfos[adjPosition - adapterCount].view;
		}

		public int GetItemViewType(int position)
		{
			int numHeaders = HeadersCount;
			if (mAdapter != null && position >= numHeaders)
			{
				int adjPosition = position - numHeaders;
				int adapterCount = mAdapter.Count;
				if (adjPosition < adapterCount)
				{
					return mAdapter.GetItemViewType(adjPosition);
				}
			}

			return AdapterView.ItemViewTypeHeaderOrFooter;
		}

		public int ViewTypeCount
		{
			get
			{
				if (mAdapter != null)
				{
					return mAdapter.ViewTypeCount;
				}
				return 1;
			}
		}

		public void RegisterDataSetObserver(DataSetObserver observer)
		{
			if (mAdapter != null)
			{
				mAdapter.RegisterDataSetObserver(observer);
			}
		}

		public void UnregisterDataSetObserver(DataSetObserver observer)
		{
			if (mAdapter != null)
			{
				mAdapter.UnregisterDataSetObserver(observer);
			}
		}

		public Filter Filter
		{
			get
			{
				if (mIsFilterable)
				{
					return ((IFilterable) mAdapter).Filter;
				}
				return null;
			}
		}

		public IListAdapter WrappedAdapter
		{
			get
			{
				return mAdapter;
			}
		}
	}
}