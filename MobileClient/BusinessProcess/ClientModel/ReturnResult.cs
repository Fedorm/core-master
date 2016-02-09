using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.BusinessProcess.ClientModel
{
    public class ReturnResult<T>
    {
        public ReturnResult(T result)
        {
            Result = result;
            Success = true;
        }

        public ReturnResult(string error, int code)
        {
            Error = error;
            Code = code;
            Success = false;
        }

        public bool Success { get; private set; }

        public T Result { get; private set; }

        public string Error { get; private set; }

        public int Code { get; private set; }
    }
}
