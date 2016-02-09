using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitMobile.Droid.UI
{
    interface IGesturable
    {
        GestureType SupportedGesture { get; }
    }
}