using System;

namespace BitMobile.Common.IO
{
    public interface IFile
    {
        DateTime Time { get; set; }
        string RelativePath { get; set; }
        long Size { get; set; }
    }
}
