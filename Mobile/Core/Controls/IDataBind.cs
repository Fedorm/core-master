using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Controls
{
    public interface IDataBind
    {
        DataBinder Value { get; set; }
        void DataBind();
    }
}