using System;
using System.Collections.Generic;
using System.Text;

namespace BitMobile.Controls
{
    public interface IApplicationContextAware
    {
        void SetApplicationContext(object applicationContext);
    }
}