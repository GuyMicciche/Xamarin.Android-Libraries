using Android.OS;
using Android.Util;
using Java.Interop;
using System;

namespace AndroidStyledDialogs
{
	public class SparseBooleanArrayParcelable : SparseBooleanArray, IParcelable
	{
        [ExportField("CREATOR")]
        public static MyParcelableCreator InitializeCreator()
        {
            Console.WriteLine("SparseBooleanArrayParcelable.InitializeCreator");
            return new MyParcelableCreator();
        }
        
		public SparseBooleanArrayParcelable()
		{

		}

		public SparseBooleanArrayParcelable(SparseBooleanArray sparseBooleanArray)
		{
			for (int i = 0; i < sparseBooleanArray.Size(); i++)
			{
				this.Put(sparseBooleanArray.KeyAt(i), sparseBooleanArray.ValueAt(i));
			}
		}

		public int DescribeContents()
		{
            Console.WriteLine("SparseBooleanArrayParcelable.DescribeContents");
			return 0;
		}

		public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
		{
            Console.WriteLine("SparseBooleanArrayParcelable.WriteToParcel");
            int[] keys = new int[Size()];
            bool[] values = new bool[Size()];

            for (int i = 0; i < Size(); i++)
			{
                keys[i] = KeyAt(i);
                values[i] = ValueAt(i);
			}

			dest.WriteInt(Size());
			dest.WriteIntArray(keys);
			dest.WriteBooleanArray(values);
		}
	}

    public class MyParcelableCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            Console.WriteLine("MyParcelableCreator.CreateFromParcel");
            SparseBooleanArrayParcelable read = new SparseBooleanArrayParcelable();
            int size = source.ReadInt();

            int[] keys = new int[size];
            bool[] values = new bool[size];

            source.ReadIntArray(keys);
            source.ReadBooleanArray(values);

            for (int i = 0; i < size; i++)
            {
                read.Put(keys[i], values[i]);
            }

            return read;
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            Console.WriteLine("MyParcelableCreator.NewArray");
            return new SparseBooleanArrayParcelable[size];
        }
    }
}