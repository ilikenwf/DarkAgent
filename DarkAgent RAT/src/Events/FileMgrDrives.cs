using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void FileMgrDrivesHandler(object o, FileMgrDrivesEventArgs e);

    public class FileMgrDrivesEventArgs : EventArgs
    {
        public readonly FileMgrDrives _FileMgrDrives;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public FileMgrDrivesEventArgs(FileMgrDrives FilemgrDrives, string ip)
        {
            lock(locky)
            {
                this._FileMgrDrives = FilemgrDrives;
                this.RemoteIP = ip;
            }
        }
    }

    public class FileMgrDrivesEvent
    {
        public static event FileMgrDrivesHandler FileMgrDrives;
        private static Object locky = new Object();
        public static void OnFileMgrDrives(FileMgrDrivesEventArgs e)
        {
            lock(locky)
            {
                if (FileMgrDrives != null)
                    FileMgrDrives(new object(), e);
            }
        }
    }
}
