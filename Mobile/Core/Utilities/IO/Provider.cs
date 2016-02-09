using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BitMobile.Utilities.IO
{
    // ReSharper disable UnusedMemberInSuper.Global
    public abstract class Provider
    {
        public const string PrivateDirectory = "private";
        public const string SharedDirectory = "shared";

        // ReSharper disable once PublicConstructorInAbstractClass
        public Provider()
        {
            Items = new List<Item>();
        }

        public List<Item> Items { get; private set; }

        public abstract void SaveFile(string relativePath, Stream source);

        public abstract void DeleteFile(string relativePath);

        public bool FileExists(string relativePath)
        {
            return Items.Exists(val => val.RelativePath == relativePath);
        }

        public Item FindFile(string relativePath)
        {
            return Items.Find(val => val.RelativePath == relativePath);
        }

        protected static Item ParseItems(string message)
        {
            string[] splitted = message.Split('|');
            var item = new Item();
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