using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Translator;
using BitMobile.Common.Application;
using BitMobile.Common.ScriptEngine;

namespace BitMobile.BusinessProcess.ClientModel
{
    public class Email
    {
        private readonly IScriptEngine _scriptEngine;
        private readonly IApplicationContext _applicationContext;

        public Email(IScriptEngine scriptEngine, IApplicationContext applicationContext)
        {
            _scriptEngine = scriptEngine;
            _applicationContext = applicationContext;
        }

        public void Create(object address, string text, string subject)
        {
            Create(address, text, subject, null, null, null);
        }

        public void Create(object address, string text, string subject, object attachments)
        {
            Create(address, text, subject, attachments, null, null);
        }

        public void Create(object address, string text, string subject, object attachments, IJsExecutable handler)
        {
            Create(address, text, subject, attachments, handler, null);
        }

        public async void Create(object address, string text, string subject, object attachments
            , IJsExecutable handler, object state)
        {
            string[] destinations = ObjectToStringArray(address);
            var paths = new List<string>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var item in ObjectToStringArray(attachments))
                if (!string.IsNullOrWhiteSpace(item))
                    try
                    {
                        paths.Add(IOContext.Current.TranslateLocalPath(item));
                    }
                    catch (NonFatalException)
                    {
                    }


            await _applicationContext.EmailProvider.OpenEmailManager(destinations, subject, text, paths.ToArray());
            if (handler != null)
                handler.ExecuteCallback(_scriptEngine.Visitor, state, new Args<object>(null));
        }

        private string[] ObjectToStringArray(object obj)
        {
            if (obj == null)
                return new string[0];

            var str = obj as string;
            if (str != null)
                return new[] { str };

            var arr = obj as ArrayList;
            if (arr != null)
            {
                var result = new List<string>(arr.Count);
                result.AddRange(arr.OfType<string>());
                return result.ToArray();
            }

            throw new NonFatalException(D.INVALID_ARGUMENT_VALUE);
        }
    }
}
