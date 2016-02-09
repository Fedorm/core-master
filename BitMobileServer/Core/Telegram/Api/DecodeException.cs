using System;

namespace Telegram
{
    class DecodeException : Exception
    {
        public DecodeException(string message)
            : base(message)
        {

        }
    }
}
