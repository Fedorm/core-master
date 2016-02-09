using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService
{
    public class AsyncDeployMetadataThread
    {
        const int progressStep = 200;

        RequestHandlerProxy proxy;
        Common.Solution solution;
        Stream messageBody;
        bool checkTabularSectionKey;
        Exception exception;

        public AsyncDeployMetadataThread(RequestHandlerProxy proxy, Common.Solution solution, Stream messageBody, bool checkTabularSectionKey)
        {
            this.proxy = proxy;
            this.solution = solution;
            this.messageBody = messageBody;
            this.checkTabularSectionKey = checkTabularSectionKey;
        }

        public void Start()
        {
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
                    proxy.UploadMetadataInternal(solution, messageBody, checkTabularSectionKey);
                }
                catch (Exception e)
                {
                    Common.Solution.Log(solution.Name, "admin", e.Message);
                }
            }
            finally
            {
            }
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

    }
}
