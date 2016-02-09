using System;
using System.IO;

namespace Microsoft.Synchronization.Services.Formatters
{
    public class ProgressStream : Stream
    {
        static readonly int predictedLength = 100000;
        int contentLength = 0;
        int bytesRead = 0;
        Action<int, int> statusCallback;
        Stream _baseStream;

        public ProgressStream(Stream s, int contentLength, Action<int, int> onStatus = null)
        {
            this._baseStream = s;
            this.contentLength = contentLength > 0 ? contentLength : predictedLength;
            this.statusCallback = onStatus;
        }

        public override bool CanRead
        {
            get { return _baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            _baseStream.Flush();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int rc = _baseStream.Read(buffer, offset, count);
            bytesRead += rc;

            if (statusCallback != null)
                statusCallback(contentLength, bytesRead);

            return rc;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            statusCallback = null;

            if (_baseStream != null)
            {
                _baseStream.Dispose();
                _baseStream = null;
            }
            base.Dispose(disposing);
        }
    }
}

