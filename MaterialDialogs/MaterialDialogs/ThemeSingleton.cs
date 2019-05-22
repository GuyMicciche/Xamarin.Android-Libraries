using Android.Graphics;
using Android.Graphics.Drawables;

namespace MaterialDialogs
{
	/// <summary>
	/// Use of this is discouraged for now; for internal use only. See the Global Theming section of the README.
	/// </summary>
	public class ThemeSingleton
	{
		private static ThemeSingleton singleton;

		public static ThemeSingleton Get(bool createIfNull)
		{
			if (singleton == null && createIfNull)
			{
				singleton = new ThemeSingleton();
			}
			return singleton;
		}

		public static ThemeSingleton Get()
		{
			return Get(true);
		}

		public bool DarkTheme = false;
        public Color TitleColor = Color.Black;
        public Color ContentColor = Color.Black;
        public Color PositiveColor = Color.Black;
        public Color NeutralColor = Color.Black;
        public Color NegativeColor = Color.Black;
        public Color ItemColor = Color.Black;
		public Drawable Icon = null;
        public Color BackgroundColor = Color.Black;
		public Color DividerColor = Color.Black;

		public int ListSelector = 0;
		public int BtnSelectorStacked = 0;
		public int BtnSelectorPositive = 0;
		public int BtnSelectorNeutral = 0;
		public int BtnSelectorNegative = 0;
	}
}