using System;
using System.Collections.Generic;
using System.IO;

namespace BitMobile.Common.IO
{
    public interface IRemoteProvider
    {
        void SaveFile(string relativePath, Stream source);
        void DeleteFile(string relativePath);
        void LoadFile(string relativePath, Action<Stream> action);
        List<IFile> Files { get; }
        bool FileExists(string relativePath);
        IFile FindFile(string relativePath);
    }
}