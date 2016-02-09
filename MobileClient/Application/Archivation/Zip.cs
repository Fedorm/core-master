using System.IO;
using System.IO.Compression;

namespace BitMobile.Application.Archivation
{
    public static class GZip
    {
        public static Stream UnzipStream(Stream input)
        {
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(input, CompressionMode.Decompress, true))
                gzip.CopyTo(ms);

            ms.Position = 0;

            return ms;
        }

        public static Stream ZipStream(Stream input)
        {
            var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress, true))
                input.CopyTo(gzip);

            ms.Position = 0;
            return ms;
        }
    }
}
