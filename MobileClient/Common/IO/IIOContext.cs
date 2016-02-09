using System;
using System.IO;

namespace BitMobile.Common.IO
{
    // ReSharper disable once InconsistentNaming
    public interface IIOContext
    {
        string PrivateDirectory { get; }
        string SharedDirectory { get; }
        string LogDirectory { get; }
        ILocalProvider CreateLocalProvider(string root);
        IRemoteProvider CreateRemoteProvider(string root);
        IRemoteProvider CreateWebDavProvider(string root);
        bool Delete(string path, FileSystemItem type = FileSystemItem.Both);
        bool Exists(string path, FileSystemItem type = FileSystemItem.Both);
        bool CreateDirectory(string path);
        void Copy(string source, string dest, bool overwrite = false, FileSystemItem type = FileSystemItem.Both);
        string TranslateLocalPath(string path);
        string GetSolutionName(Uri uri);
        FileStream FileStream(string path, FileMode fileMode);
    }
}
