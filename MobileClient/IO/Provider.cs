using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BitMobile.Common.IO;

namespace BitMobile.IO
{
    // ReSharper disable UnusedMemberInSuper.Global
    abstract class Provider
    {
        public const string PrivateDirectory = "private";
        public const string SharedDirectory = "shared";
        public const string LogDirectory = "log";

        // ReSharper disable once PublicConstructorInAbstractClass
        public Provider()
        {
            Files = new List<IFile>();
        }

        public List<IFile> Files { get; private set; }

        public abstract void SaveFile(string relativePath, Stream source);

        public abstract void DeleteFile(string relativePath);

        public bool FileExists(string relativePath)
        {
            return Files.Exists(val => val.RelativePath.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
        }

        public IFile FindFile(string relativePath)
        {
            return Files.Find(val => val.RelativePath.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
        }

        protected static RelativeFile ParseItems(string message)
        {
            string[] splitted = message.Split('|');
            var item = new RelativeFile();
            string[] fileDirectories = splitted[0].Split('\\');
            item.RelativePath = Path.Combine(fileDirectories);
            string dt = splitted[1];
            DateTime time = DateTime.ParseExact(dt, @"yyyy\.MM\.dd hh:mm:ss", CultureInfo.GetCultureInfo("ru-RU"));
            item.Time = time;
            if (splitted.Length > 2)
            {
                long size;
                if (long.TryParse(splitted[2], out size))
                    item.Size = size;
            }
            return item;
        }
    }
}