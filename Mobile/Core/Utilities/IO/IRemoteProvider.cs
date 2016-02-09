using System;
using System.Collections.Generic;
using System.IO;

namespace BitMobile.Utilities.IO
{
    public interface IRemoteProvider
    {
        void SaveFile(string relativePath, Stream source);
        void DeleteFile(string relativePath);
        void LoadFile(string relativePath, Action<Stream> action);
        List<Item> Items { get; }
        bool FileExists(string relativePath);
        Item FindFile(string relativePath);
    }
}