using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void GetProcessesThreadHandler(object o, GetProcessesThreadEventArgs e);

    public class GetProcessesThreadEventArgs : EventArgs
    {
        public readonly ProcessThreadInfo _processThreadInfo;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public GetProcessesThreadEventArgs(ProcessThreadInfo processThreadInfo, string ip)
        {
            lock(locky)
            {
                this._processThreadInfo = processThreadInfo;
                this.RemoteIP = ip;
            }
        }
    }

    public class GetProcessesThreadEvent
    {
        public static event GetProcessesThreadHandler ClientProcessesThread;
        private static Object locky = new Object();
        public static void OnGetProcessesThread(GetProcessesThreadEventArgs e)
        {
            lock(locky)
            {
                if (ClientProcessesThread != null)
                    ClientProcessesThread(new object(), e);
            }
        }
    }
}
