using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_RAT.src.Objects;

namespace DarkAgent_RAT.src.Events
{
    public delegate void GetPasswordHandler(object o, PasswordEventArgs e);

    public class PasswordEventArgs : EventArgs
    {
        public readonly PasswordInfo passwordInfo;
        public readonly string RemoteIP;
        private Object locky = new Object();
        public PasswordEventArgs(PasswordInfo PasswordInfo, string ip)
        {
            lock(locky)
            {
                this.passwordInfo = PasswordInfo;
                this.RemoteIP = ip;
            }
        }
    }

    public class PasswordEvent
    {
        public static event GetPasswordHandler passwords;
        private static Object locky = new Object();
        public static void OnGetPasswords(PasswordEventArgs e)
        {
            lock(locky)
            {
                if (passwords != null)
                    passwords(new object(), e);
            }
        }
    }
}