using System;


namespace BitMobile.Droid.UI
{
    [Flags]
    enum GestureType
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Any = 3
    }
}