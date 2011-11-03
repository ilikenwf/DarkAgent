using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Network.DataNetwork.Packets.Receive;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void GetProcessesHandler(object o, ProcessEventArgs e);

    public class ProcessEventArgs : EventArgs
    {
        public readonly ProcessInfo processInfo;
        public readonly string RemoteIP;
        private static Object locky = new Object();
        public ProcessEventArgs(ProcessInfo processInfo, string ip)
        {
            lock(locky)
            {
                this.processInfo = processInfo;
                this.RemoteIP = ip;
            }
        }
    }

    public class ProcessEvent
    {
        public static event GetProcessesHandler ClientProcesses;
        private static Object locky = new Object();
        public static void OnGetProcesses(ProcessEventArgs e)
        {
            lock(locky)
            {
                if (ClientProcesses != null)
                    ClientProcesses(new object(), e);
            }
        }
    }
}