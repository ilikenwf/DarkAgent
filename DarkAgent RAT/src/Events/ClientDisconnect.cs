using System;
using System.Collections.Generic;
using System.Text;

namespace DarkAgent_RAT.src.Events
{
    public delegate void ClientDisconnectHandler(object o, ClientDisconnectEventArgs e);

    public class ClientDisconnectEventArgs : EventArgs
    {
        public readonly string RemoteIP;
        private Object locky = new Object();
        public ClientDisconnectEventArgs(string RemoteIP)
        {
            lock(locky)
            {
                this.RemoteIP = RemoteIP;
            }
        }
    }

    public class ClientDisconnectEvent
    {
        public static event ClientDisconnectHandler ClientDisconnect;
        private static Object locky = new Object();
        public static void OnClientDisconnect(ClientDisconnectEventArgs e)
        {
            lock(locky)
            {
                if (ClientDisconnect != null)
                    ClientDisconnect(new object(), e);
            }
        }
    }
}
