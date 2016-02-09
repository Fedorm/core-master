using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Common.DataAccessLayer
{
    public interface ITranslator
    {
        string TranslateByKey(string key);
    }
}