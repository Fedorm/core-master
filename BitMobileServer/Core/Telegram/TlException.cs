using System;
using Jint;
using Jint.Native;

namespace Telegram
{
    public class TlException : JsException
    {
        public TlException(Exception innerException)
            : base(new JsClr(JintEngine.CurrentVisitor, innerException), innerException)
        {
        }
    }
}
