using System.Collections.Generic;

namespace AndroidStaggeredGrid.Example
{
    public class SampleData
    {
        public const int SAMPLE_DATA_ITEM_COUNT = 30;

        public static List<string> GenerateSampleData()
        {
            List<string> data = new List<string>(SAMPLE_DATA_ITEM_COUNT);

            for (int i = 0; i < SAMPLE_DATA_ITEM_COUNT; i++)
            {
                data.Add("SAMPLE #");
            }

            return data;
        }
    }
}