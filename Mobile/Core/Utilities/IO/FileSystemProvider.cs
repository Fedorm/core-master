using System;
using System.IO;

namespace BitMobile.Utilities.IO
{
    public class FileSystemProvider : Provider
    {
        readonly string _localStorage;
        readonly string _root;

        public FileSystemProvider(string localStorage, string root)
        {
            _localStorage = localStorage;
            _root = root;
            FillItems(string.Empty);
        }

        public override void SaveFile(string relativePath, Stream source)
        {
            string path = Path.Combine(_localStorage, _root, relativePath);
            string dir = Path.GetDirectoryName(path);
            if (dir == null)
                throw new NullReferenceException("Cannot combine url");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                source.CopyTo(stream);

            var item = new Item
            {
                RelativePath = relativePath,
                Time = File.GetLastWriteTimeUtc(path)
            };
            Items.Add(item);
        }

        public override void DeleteFile(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            File.Delete(Path.Combine(_localStorage, _root, relativePath));
            Items.RemoveAll(val => val.RelativePath == relativePath);
        }

        public Stream GetStream(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            string path = Path.Combine(_localStorage, _root, relativePath);
            return new FileStream(path, FileMode.Open);
        }

        public static string TranslatePath(string localstorage, string input)
        {
            string path;
            if (input.StartsWith(string.Format("/{0}/", PrivateDirectory))
                || input.StartsWith(string.Format("\\{0}\\", PrivateDirectory)))
            {
                path = Path.Combine(localstorage
                    , PrivateDirectory
                    , input.Substring(PrivateDirectory.Length + 2));
            }
            else if (input.StartsWith(string.Format("/{0}/", SharedDirectory))
                || input.StartsWith(string.Format("\\{0}\\", SharedDirectory)))
            {
                path = Path.Combine(localstorage
                    , SharedDirectory
                    , input.Substring(SharedDirectory.Length + 2));
            }
            else
                throw new Exception("Incorrect path: " + input);

            return path;
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                foreach (var dir in Directory.GetDirectories(path))
                    DeleteDirectory(dir);

                foreach (var file in Directory.GetFiles(path))
                    File.Delete(file);

                Directory.Delete(path);
            }
        }

        public static string GetSolutionName(Uri uri)
        {
            string solutionName = uri.Segments[uri.Segments.Length - 1];

            solutionName = solutionName.Replace("/", "");
            solutionName = solutionName.Replace("\\", "");

            return solutionName;
        }

        void FillItems(string relativePath)
        {
            string fullPath = Path.Combine(_localStorage, _root, relativePath);

            if (Directory.Exists(fullPath))
            {
                foreach (string path in Directory.GetDirectories(fullPath))
                {
                    string name = Path.GetFileName(path);
                    if (name != null)
                        FillItems(Path.Combine(relativePath, name));
                }

                foreach (string path in Directory.GetFiles(fullPath))
                {
                    string name = Path.GetFileName(path);
                    if (name != null)
                    {
                        var item = new Item
                        {
                            RelativePath = Path.Combine(relativePath, name),
                            Time = File.GetLastWriteTimeUtc(path),
                            Size = new FileInfo(path).Length
                        };
                        Items.Add(item);
                    }
                }
            }
            else
                Directory.CreateDirectory(fullPath); // To create /shared/ and /private/ directories
        }
    }
}
