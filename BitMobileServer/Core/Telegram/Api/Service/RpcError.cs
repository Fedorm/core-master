using System;

namespace Telegram.Service
{
    public class RpcError
    {
        public RpcError(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public override string ToString()
        {
            return String.Format("RPC Error! error code: {0}, error message: {1}",
                ErrorCode, ErrorMessage
                );
        }
    }
}