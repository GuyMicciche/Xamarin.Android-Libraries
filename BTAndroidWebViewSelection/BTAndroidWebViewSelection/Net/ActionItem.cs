using Android.Graphics;
using Android.Graphics.Drawables;

namespace BTAndroidWebViewSelection
{
	/// <summary>
	/// Action item, displayed as menu with icon and text.
	/// </summary>
	public class ActionItem
	{
		private Drawable icon;
		private Bitmap thumb;
		private string title;
		private int actionId = -1;
		private bool selected;
		private bool sticky;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actionId">  Action id for case statements </param>
		/// <param name="title">     Title </param>
		/// <param name="icon">      Icon to use </param>
		public ActionItem(int actionId, string title, Drawable icon)
		{
			this.title = title;
			this.icon = icon;
			this.actionId = actionId;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public ActionItem() 
            : this(-1, null, null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actionId">  Action id of the item </param>
		/// <param name="title">     Text to show for the item </param>
		public ActionItem(int actionId, string title) 
            : this(actionId, title, null)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="icon"> <seealso cref="Drawable"/> action icon </param>
		public ActionItem(Drawable icon) 
            : this(-1, null, icon)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="actionId">  Action ID of item </param>
		/// <param name="icon">      <seealso cref="Drawable"/> action icon </param>
		public ActionItem(int actionId, Drawable icon) 
            : this(actionId, null, icon)
		{
		}

		/// <summary>
		/// Set action title
		/// </summary>
		/// <param name="title"> action title </param>
		public string Title
		{
			set
			{
				this.title = value;
			}
			get
			{
				return this.title;
			}
		}

		/// <summary>
		/// Set action icon
		/// </summary>
		/// <param name="icon"> <seealso cref="Drawable"/> action icon </param>
		public Drawable Icon
		{
			set
			{
				this.icon = value;
			}
			get
			{
				return this.icon;
			}
		}

		 /// <summary>
		 /// Set action id
		 /// </summary>
		 /// <param name="actionId">  Action id for this action </param>
		public int ActionId
		{
			set
			{
				this.actionId = value;
			}
			get
			{
				return actionId;
			}
		}

		/// <summary>
		/// Set sticky status of button
		/// </summary>
		/// <param name="sticky">  true for sticky, pop up sends event but does not disappear </param>
		public bool Sticky
		{
			set
			{
				this.sticky = value;
			}
			get
			{
				return sticky;
			}
		}

		/// <summary>
		/// Set selected flag;
		/// </summary>
		/// <param name="selected"> Flag to indicate the item is selected </param>
		public bool Selected
		{
			set
			{
				this.selected = value;
			}
			get
			{
				return this.selected;
			}
		}

		/// <summary>
		/// Set thumb
		/// </summary>
		/// <param name="thumb"> Thumb image </param>
		public Bitmap Thumb
		{
			set
			{
				this.thumb = value;
			}
			get
			{
				return this.thumb;
			}
		}
	}
}