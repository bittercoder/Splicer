// Copyright 2006-2008 Splicer Project - http://www.codeplex.com/splicer/
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
using System.Globalization;
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
    public sealed class GrouperQuickTimeWrapper : IDisposable
    {
        public const string GVQuicktimeGuid = "{CB83D662-BEFE-4dbf-830C-25E52627C1C3}";
        private bool _disposed;

        public GrouperQuickTimeWrapper()
        {
            NativeMethods.LoadQuickTime();
        }

        public static bool IsGVQuickTimeRegistered
        {
            get
            {
                if (
                    Registry.ClassesRoot.OpenSubKey(
                        string.Format(CultureInfo.InvariantCulture, @"CLSID\{0}", GVQuicktimeGuid)) != null)
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
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Apple Computer, Inc.\QuickTime");

                if (key != null)
                {
                    var version = (int) key.GetValue("Version", 0);
                    if (((version >> 0x18) & 0xff) >= 6)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static bool IsQuickTimeInstalledSlow
        {
            get
            {
                try
                {
                    if (NativeMethods.IsQTInstalled() == 0)
                    {
                        return false;
                    }
                    return true;
                }
                catch (COMException)
                {
                    return false;
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~GrouperQuickTimeWrapper()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                NativeMethods.UnloadQuickTime();
                _disposed = true;
            }
        }
    }
}