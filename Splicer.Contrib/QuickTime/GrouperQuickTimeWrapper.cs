// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Splicer.Contrib.QuickTime
{
    /// <summary>
    /// This is a demonstration of how to write a wrapper for a source filter that you want
    /// to use in place of the standard filters in the system.  In this case we are using GVQuickTime.dll
    /// which will be present on your system if you have installed the grouper client (http://www.grouper.com/)
    /// to handle quicktime videos.
    /// 
    /// This is for demonstration purposes only, no permission has been granted by Grouper to use their
    /// GVQuicktime component in this way.
    /// </summary>
    public class GrouperQuickTimeWrapper : IDisposable
    {
        private bool _disposed;
        public const string GVQuicktimeGuid = "{CB83D662-BEFE-4dbf-830C-25E52627C1C3}";


        public GrouperQuickTimeWrapper()
        {
            _disposed = false;
            LoadQuickTime();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                UnloadQuickTime();
            }
        }

        [DllImport("GVQuickTime.dll")]
        private static extern int IsQTInstalled();

        [DllImport("GVQuickTime.dll")]
        private static extern void LoadQuickTime();

        [DllImport("GVQuickTime.dll", EntryPoint="DllRegisterServer")]
        public static extern void RegisterGVQuickTimeDll();

        [DllImport("GVQuickTime.dll")]
        private static extern void UnloadQuickTime();

        [DllImport("GVQuickTime.dll", EntryPoint="DllUnregisterServer")]
        public static extern void UnregisterGVQuickTimeDll();

        public static void RegisterDllIfPresent()
        {
            try
            {
                if (IsGVQuickTimeDllPresent)
                {
                    RegisterGVQuickTimeDll();
                }
            }
            catch
            {
            }
        }

        public static bool IsGVQuickTimeDllPresent
        {
            get
            {
                if (File.Exists("GVQuickTime.dll"))
                {
                    return true;
                }
                return false;
            }
        }

        public static bool IsGVQuickTimeRegistered
        {
            get
            {
                if (Registry.ClassesRoot.OpenSubKey(string.Format(@"CLSID\{0}", GVQuicktimeGuid)) != null)
                {
                    return true;
                }
                return false;
            }
        }

        public static bool IsQuickTime6OrLaterInstalledQuick
        {
            get
            {
                bool flag = false;
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Apple Computer, Inc.\QuickTime");
                if (key != null)
                {
                    object obj1 = key.GetValue("Version", 0);
                    int num1 = (int) obj1;
                    int num2 = (num1 >> 0x18) & 0xff;
                    if (num2 >= 6)
                    {
                        flag = true;
                    }
                }
                return flag;
            }
        }

        public static bool IsQuickTimeInstalledSlow
        {
            get
            {
                try
                {
                    if (IsQTInstalled() == 0)
                    {
                        return false;
                    }
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}