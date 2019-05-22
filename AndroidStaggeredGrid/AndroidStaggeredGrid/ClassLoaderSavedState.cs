using Android.OS;

using Java.Interop;
using Java.Lang;

using System;

namespace AndroidStaggeredGrid
{
	/// <summary>
	/// A <seealso cref="android.os.Parcelable"/> implementation that should be used by inheritance
	/// hierarchies to ensure the state of all classes along the chain is saved.
	/// </summary>
	public class ClassLoaderSavedState : Java.Lang.Object, IParcelable
	{
        public static ClassLoaderSavedState instance = null;
        private static ClassLoaderSavedState Instance
		{
            get
            {
                if (instance == null)
                {
                    instance = new ClassLoaderSavedState();
                }
                return instance;
            }
        }

		private IParcelable mSuperState = Instance;
		private ClassLoader mClassLoader;

		/// <summary>
		/// Constructor used to make the EMPTY_STATE singleton
		/// </summary>
        public ClassLoaderSavedState()
        {
            mSuperState = null;
            mClassLoader = null;
        }

		/// <summary>
		/// Constructor called by derived classes when creating their ListSavedState objects
		/// </summary>
		/// <param name="superState"> The state of the superclass of this view </param>
        public ClassLoaderSavedState(IParcelable superState, ClassLoader classLoader)
		{
			mClassLoader = classLoader;
			if (superState == null)
			{
				throw new System.ArgumentException("superState must not be null");
			}
			else
			{
				mSuperState = superState != Instance ? superState : null;
			}
		}

		/// <summary>
		/// Constructor used when reading from a parcel. Reads the state of the superclass.
		/// </summary>
		/// <param name="source"> </param>
        public ClassLoaderSavedState(Parcel source)
		{
			// ETSY : we're using the passed super class loader unlike AbsSavedState
			IParcelable superState = (IParcelable)source.ReadParcelable(mClassLoader);
			mSuperState = superState != null ? superState : Instance;
		}

		public IParcelable SuperState
		{
			get
			{
				return mSuperState;
			}
		}

		public int DescribeContents()
		{
			return 0;
		}

		public virtual void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
		{
			dest.WriteParcelable(mSuperState, flags);
		}

        [ExportField("CREATOR")]
        public static ClassLoaderParcelableCreator ClassLoaderInitializeCreator()
        {
            Console.WriteLine("ClassLoaderParcelableCreator.ClassLoaderInitializeCreator");
            return new ClassLoaderParcelableCreator();
        }
        public class ClassLoaderParcelableCreator : Java.Lang.Object, IParcelableCreator
		{
			public Java.Lang.Object CreateFromParcel(Parcel source)
			{
				IParcelable superState = (IParcelable)source.ReadParcelable(null);
				if (superState != null)
				{
					throw new IllegalStateException("superState must be null");
				}
				return Instance;
			}

            public Java.Lang.Object[] NewArray(int size)
			{
				return new ClassLoaderSavedState[size];
			}
		}
	}
}