using System;
using System.Collections.Generic;
using System.Text;

namespace DarkAgent_RAT.src.Events
{
    public delegate void ClipboardHandler(object o, ClipboardEventArgs e);

    public class ClipboardEventArgs : EventArgs
    {
        public readonly string RemoteIP;
        public readonly string data;
        private Object locky = new Object();
        public ClipboardEventArgs(string RemoteIP, string data)
        {
            lock(locky)
            {
                this.RemoteIP = RemoteIP;
                this.data = data;
            }
        }
    }

    public class ClipboardEvent
    {
        public static event ClipboardHandler Clipboard;
        private static Object locky = new Object();
        public static void OnClipboard(ClipboardEventArgs e)
        {
            lock(locky)
            {
                if (Clipboard != null)
                    Clipboard(new object(), e);
            }
        }
    }
}