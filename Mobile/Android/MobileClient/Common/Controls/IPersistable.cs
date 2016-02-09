using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Controls
{
    public interface IPersistable
    {
        object GetState();
        void SetState(object state);
    }
}