using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace DarkAgent_RAT.src.Events
{
    public delegate void RemoteControlHandler(object o, RemoteControlEventArgs e);

    public class RemoteControlEventArgs : EventArgs
    {
        public readonly string RemoteIP;
        public Image ScreenImage;
        private Object locky = new Object();
        public RemoteControlEventArgs(string RemoteIP, Image ScreenImage)
        {
            lock(locky)
            {
                this.RemoteIP = RemoteIP;
                this.ScreenImage = ScreenImage;
            }
        }
    }

    public class RemoteControlEvent
    {
        public static event RemoteControlHandler RemoteControl;
        private static Object locky = new Object();
        public static void OnRemoteControl(RemoteControlEventArgs e)
        {
            lock(locky)
            {
                if (RemoteControl != null)
                    RemoteControl(new object(), e);
            }
        }
    }
}
