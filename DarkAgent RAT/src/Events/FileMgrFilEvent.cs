using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void FileMgrFileHandler(object o, FileMgrFileEventArgs e);

    public class FileMgrFileEventArgs : EventArgs
    {
        public readonly FileMgrFileInfo _FileMgrFile;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public FileMgrFileEventArgs(FileMgrFileInfo FilemgrDir, string ip)
        {
            lock(locky)
            {
                this._FileMgrFile = FilemgrDir;
                this.RemoteIP = ip;
            }
        }
    }

    public class FileMgrFileEvent
    {
        public static event FileMgrFileHandler FileMgrFile;
        private static Object locky = new Object();
        public static void OnFileMgrFile(FileMgrFileEventArgs e)
        {
            lock(locky)
            {
                if (FileMgrFile != null)
                    FileMgrFile(new object(), e);
            }
        }
    }
}
