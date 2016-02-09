using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService
{
    public class AsyncUploadThread
    {
        const int progressStep = 200;

        RequestHandlerProxy proxy;
        Guid sessionGuid;
        Common.Solution solution;
        Stream messageBody;
        bool checkExisting;
        bool zippedStream;
        String contentEncoding;
        String filepath;
        Exception exception;
        System.Threading.AutoResetEvent startedEvent;

        public AsyncUploadThread(RequestHandlerProxy proxy, Guid sessionGuid, Common.Solution solution, Stream messageBody, bool checkExisting, bool zippedStream = true, String contentEncoding = null, String filepath = null)
        {
            this.proxy = proxy;
            this.sessionGuid = sessionGuid;
            this.solution = solution;
            this.messageBody = messageBody;
            this.checkExisting = checkExisting;
            this.zippedStream = zippedStream;
            this.contentEncoding = contentEncoding;
            this.filepath = filepath;
        }

        public void Start()
        {
            startedEvent = new AutoResetEvent(false);
            Thread t = new Thread(Execute);
            t.Start();
        }

        public Exception GetException
        {
            get
            {
                return exception;
            }
        }

        public void Execute()
        {
            exception = null;
            try
            {
                try
                {
                    proxy.UploadDataInternal(solution, null, checkExisting, !String.IsNullOrEmpty(contentEncoding), contentEncoding, filepath, ProgressCallback, progressStep);
                    proxy.CommitAsyncUploadRecord(solution, sessionGuid);
                }
                catch (Exception e)
                {
                    proxy.CommitAsyncUploadRecord(solution, sessionGuid, MakeExceptionString(e));
                }
            }
            finally
            {
                try
                {
                    startedEvent.Dispose();
                    System.IO.File.Delete(filepath);
                }
                catch
                { 
                }
            }
        }

        private void ProgressCallback(int cnt)
        {
            proxy.CommitAsyncUploadRecord(solution, sessionGuid, String.Format("{0} completed..", cnt.ToString()));
            startedEvent.Set();
        }

        private string MakeExceptionString(Exception e)
        {
            String text = e.Message;
            while (e.InnerException != null)
            {
                text = text + "; " + e.InnerException.Message;
                e = e.InnerException;
            }
            return text;
        }

        public bool IsStarted()
        {
            return startedEvent.WaitOne(5000);
        }

        public void Terminate()
        {
            this.Terminate();
        }

    }
}
