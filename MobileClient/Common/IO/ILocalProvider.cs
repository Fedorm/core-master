using System;
using System.Collections.Generic;
using System.IO;

namespace BitMobile.Common.IO
{
    public interface ILocalProvider
    {
        void SaveFile(string relativePath, Stream source);
        void DeleteFile(string relativePath);
        Stream GetStream(string relativePath);
        void DeleteDirectory(string path);
        List<IFile> Files { get; }
        bool FileExists(string relativePath);
        IFile FindFile(string relativePath);
    }
}
