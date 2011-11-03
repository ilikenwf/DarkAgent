using System;
using System.Collections.Generic;
using System.Text;
using DarkAgent_Client.src.Utils;
using System.Threading;

namespace DarkAgent_Client.src.Features
{
    public class SysMutex
    {
        public static bool CheckMutex()
        {
            if (Settings.Mutex_Enable)
            {
                Mutex mutex;
                try
                {
                    mutex = Mutex.OpenExisting(Settings.Mutex_MUTEX);
                    return false;
                }
                catch
                {
                    mutex = new Mutex(true, Settings.Mutex_MUTEX);
                    return true;
                }
            }
            return false;
        }
    }
}
