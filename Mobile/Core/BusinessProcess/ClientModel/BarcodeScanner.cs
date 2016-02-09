using BitMobile.Application;
using BitMobile.Script;
using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.ClientModel
{
    class BarcodeScanner
    {
        ScriptEngine _scriptEngine;
        IApplicationContext _context;

        public BarcodeScanner(ScriptEngine scriptEngine, IApplicationContext context)
        {
            _scriptEngine = scriptEngine;
            _context = context;
        }

        public void Scan(IJSExecutable handler)
        {
            Scan(handler, null);
        }

        public void Scan(IJSExecutable handler, object value)
        {
            Action<object> callback = result =>
                {
                    object[] p = { result, value };
                    if (handler != null)
                        handler.ExecuteStandalone(_scriptEngine.Visitor, p);
                };

            _context.ScanBarcode(callback);
        }
    }
}
