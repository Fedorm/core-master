using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BitMobile.Application.Exceptions;
using BitMobile.Application.Translator;
using BitMobile.Common.IO;

namespace BitMobile.IO
{
    class FileSystemProvider : Provider, ILocalProvider
    {
        private readonly IOContext _context;
        readonly string _localStorage;
        readonly string _root;

        public FileSystemProvider(IOContext context, string localStorage, string root)
        {
            _context = context;
            _localStorage = localStorage;
            _root = root;
            FillItems(string.Empty);
        }

        public override void SaveFile(string relativePath, Stream source)
        {
            var path = GetFullPath(relativePath);
            string dir = Path.GetDirectoryName(path);
            if (dir == null)
                throw new NullReferenceException("Cannot combine url");

            _context.CreateDirectory(dir);

            using (var stream = _context.FileStream(path, FileMode.OpenOrCreate))
                source.CopyTo(stream);

            var item = new RelativeFile
            {
                RelativePath = relativePath,
                Time = File.GetLastWriteTimeUtc(path)
            };
            Files.Add(item);
        }

        public override void DeleteFile(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            _context.Delete(GetFullPath(relativePath));
            Files.RemoveAll(val => val.RelativePath.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase));
        }

        public Stream GetStream(string relativePath)
        {
            if (!FileExists(relativePath))
                throw new Exception(relativePath + " not exist");

            string path = GetFullPath(relativePath);
            return _context.FileStream(path, FileMode.Open);
        }

        public void DeleteDirectory(string path)
        {
            _context.Delete(path, FileSystemItem.Directory);
        }

        public static string TranslatePath(string localstorage, string input)
        {
            input = FilterInvalidCharacters(input).Trim();
            string[] split = input.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length > 0 && (split[0].Equals(PrivateDirectory, StringComparison.InvariantCulture)
                || split[0].Equals(SharedDirectory, StringComparison.InvariantCulture)))
            {
                var result = new StringBuilder(localstorage);
                foreach (string part in split)
                {
                    result.Append(Path.DirectorySeparatorChar);
                    result.Append(part);
                }
                return result.ToString();
            }
            throw new NonFatalException(D.INVALID_PATH + ": " + input);
        }
        
        public static string FilterInvalidCharacters(string path)
        {
            var builder = new StringBuilder();

            string[] split = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++)
            {
                string s = split[i];

                const string invalidChars = @"^/ ? < > \ : * | """;
                var regex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
                s = regex.Replace(s, "");

                if (string.IsNullOrWhiteSpace(s))
                    throw new NonFatalException(D.INVALID_PATH + ": " + path);

                const string invalidNames =
                    "com1|com2|com3|com4|com5|com6|com7|com8|com9|lpt1|lpt2|lpt3|lpt4|lpt5|lpt6|lpt7|lpt8|lpt9|con|nul|prn";
                regex = new Regex(string.Format(@"^ *({0}) *(\..+)? *$", invalidNames)); // For example: root/ com1 .exe 
                if (regex.IsMatch(s))
                    throw new NonFatalException(D.INVALID_PATH + ": " + path);

                regex = new Regex(@"^ *\..+ *$"); // For example: root/.exe
                if (regex.IsMatch(s))
                    throw new NonFatalException(D.INVALID_PATH + ": " + path);

                s = s.Trim().ToLower();

                builder.Append(s);
                if (i < split.Length - 1)
                    builder.Append(Path.DirectorySeparatorChar);
            }

            return builder.ToString();
        }

        void FillItems(string relativePath)
        {
            string fullPath = GetFullPath(relativePath);

            if (_context.Exists(fullPath))
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
                        var item = new RelativeFile
                        {
                            RelativePath = Path.Combine(relativePath, name),
                            Time = File.GetLastWriteTimeUtc(path),
                            Size = new FileInfo(path).Length
                        };
                        Files.Add(item);
                    }
                }
            }
            else
                _context.CreateDirectory(fullPath); // To create /shared/ and /private/ directories
        }

        private string GetFullPath(string relativePath)
        {
			string path = FilterInvalidCharacters(relativePath);
			string result = Path.Combine(_localStorage, _root, path);
			return result;
        }
    }
}
