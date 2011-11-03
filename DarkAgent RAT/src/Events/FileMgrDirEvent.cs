using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void FileMgrDirHandler(object o, FileMgrDirEventArgs e);

    public class FileMgrDirEventArgs : EventArgs
    {
        public readonly FileMgrDirInfo _FileMgrDir;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public FileMgrDirEventArgs(FileMgrDirInfo FilemgrDir, string ip)
        {
            lock(locky)
            {
                this._FileMgrDir = FilemgrDir;
                this.RemoteIP = ip;
            }
        }
    }

    public class FileMgrDirEvent
    {
        public static event FileMgrDirHandler FileMgrDir;
        private static Object locky = new Object();
        public static void OnFileMgrDir(FileMgrDirEventArgs e)
        {
            lock(locky)
            {
                if (FileMgrDir != null)
                    FileMgrDir(new object(), e);
            }
        }
    }
}
