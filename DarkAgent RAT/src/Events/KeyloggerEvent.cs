using System;
using System.Collections.Generic;
using System.Text;

namespace DarkAgent_RAT.src.Events
{
    public delegate void KeyloggerHandler(object o, KeyloggerEventArgs e);

    public class KeyloggerEventArgs : EventArgs
    {
        public readonly string KeyStrokes;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public KeyloggerEventArgs(string KeyStrokes, string ip)
        {
            lock(locky)
            {
                this.KeyStrokes = KeyStrokes;
                this.RemoteIP = ip;
            }
        }
    }

    public class KeyloggerEvent
    {
        public static event KeyloggerHandler KeyStrokes;
        private static Object locky = new Object();
        public static void OnKeyStrokes(KeyloggerEventArgs e)
        {
            lock(locky)
            {
                if (KeyStrokes != null)
                    KeyStrokes(new object(), e);
            }
        }
    }
}
