using System;
using Android.Views;

namespace BitMobile.Droid.UI
{
    static class ControlExtensions
    {
        public static void Execute(this EventHandler<View.TouchEventArgs> handler
            , object sender
            , MotionEvent e)
        {
            if (handler != null)
            {
                var args = new View.TouchEventArgs(true, e);
                handler(sender, args);
            }
        }
    }
}