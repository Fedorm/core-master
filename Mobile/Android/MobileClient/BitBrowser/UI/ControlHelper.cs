using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace BitMobile.Droid.UI
{
    static class ControlHelper
    {
        public static void Execute(this EventHandler<View.TouchEventArgs> handler
            , object sender
            , MotionEvent e)
        {
            if (handler != null)
            {
                View.TouchEventArgs args = new View.TouchEventArgs(true, e);
                handler(sender, args);
            }
        }
    }
}