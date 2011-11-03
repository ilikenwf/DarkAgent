using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void CpuInfoHandler(object o, CpuInfoEventArgs e);

    public class CpuInfoEventArgs : EventArgs
    {
        public readonly string RemoteIP;
        public readonly CpuInfo cpuInfo;
        private Object locky = new Object();
        public CpuInfoEventArgs(string RemoteIP, CpuInfo info)
        {
            lock(locky)
            {
                this.RemoteIP = RemoteIP;
                this.cpuInfo = info;
            }
        }
    }

    public class CpuInfoEvent
    {
        public static event CpuInfoHandler CpuInfo;
        private static Object locky = new Object();
        public static void OnCpuInfo(CpuInfoEventArgs e)
        {
            lock(locky)
            {
                if (CpuInfo != null)
                    CpuInfo(new object(), e);
            }
        }
    }
}
