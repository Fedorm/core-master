using System;
using BitMobile.Common.Application;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    // ReSharper disable IntroduceOptionalParameters.Global
    class BarcodeScanner
    {
        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _context;

        public BarcodeScanner(IScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public void Scan(IJsExecutable handler)
        {
            Scan(handler, null);
        }

        public void Scan(IJsExecutable handler, object value)
        {
            Action<object> callback = result =>
                {
                    if (handler != null)
                        // todo: change api to universal format
                        handler.ExecuteCallback(_scriptEngine.Visitor, result, value);
                };

            _context.ScanBarcode(callback);
        }
    }
}
