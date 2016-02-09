using System;
using System.Collections.Generic;
using System.Text;


namespace BitMobile.Controls
{
    public interface IContainer
    {
        void AddChild(object obj);

        object[] Controls { get; }

        object GetControl(int index);
    }
}