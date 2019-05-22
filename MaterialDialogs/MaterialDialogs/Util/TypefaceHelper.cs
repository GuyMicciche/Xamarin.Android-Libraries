using Android.Content;
using Android.Graphics;
using System.Collections.Generic;

namespace MaterialDialogs
{
	/*
	    Each call to Typeface.createFromAsset will load a new instance of the typeface into memory,
	    and this memory is not consistently get garbage collected
	    http://code.google.com/p/android/issues/detail?id=9904
	    (It states released but even on Lollipop you can see the typefaces accumulate even after
	    multiple GC passes)
	
	    You can detect this by running:
	    adb shell dumpsys meminfo com.your.packagenage
	
	    You will see output like:
	
	     Asset Allocations
	        zip:/data/app/com.your.packagenage-1.apk:/assets/Roboto-Medium.ttf: 125K
	        zip:/data/app/com.your.packagenage-1.apk:/assets/Roboto-Medium.ttf: 125K
	        zip:/data/app/com.your.packagenage-1.apk:/assets/Roboto-Medium.ttf: 125K
	        zip:/data/app/com.your.packagenage-1.apk:/assets/Roboto-Regular.ttf: 123K
	        zip:/data/app/com.your.packagenage-1.apk:/assets/Roboto-Medium.ttf: 125K
	
	*/
	public class TypefaceHelper
	{
        private static readonly Dictionary<string, Typeface> cache = new Dictionary<string, Typeface>();

		public static Typeface Get(Context c, string name)
		{
			lock (cache)
			{
				if (!cache.ContainsKey(name))
				{
					Typeface t = Typeface.CreateFromAsset(c.Assets, string.Format("fonts/{0}.ttf", name));
					cache.Add(name, t);
					return t;
				}
				return cache[name];
			}
		}
	}
}