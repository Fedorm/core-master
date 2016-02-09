using System;
using BitMobile.Common.IO;

namespace BitMobile.IO
{
    class RelativeFile : IFile
    {
        public RelativeFile()
        {
            Size = long.MaxValue;
        }

        public DateTime Time { get; set; }

        public string RelativePath { get; set; }

        public long Size { get; set; }
    }
}
