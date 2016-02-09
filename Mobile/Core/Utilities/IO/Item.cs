using System;

namespace BitMobile.Utilities.IO
{
    public class Item
    {
        public Item()
        {
            Size = long.MaxValue;
        }

        public DateTime Time { get; set; }

        public string RelativePath { get; set; }    

        public long Size { get; set; }
    }
}
