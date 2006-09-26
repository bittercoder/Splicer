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
using System.Runtime.InteropServices;

namespace Splicer.WindowsMedia
{
    public static class ProfileManager
    {
        public static IWMProfileManager CreateInstance()
        {
            return WMCreateProfileManager();
        }

        [
            DllImport("WMVCore.dll", EntryPoint="WMCreateProfileManager", PreserveSig=false, SetLastError=true,
                ExactSpelling=true)]
        private static extern IWMProfileManager WMCreateProfileManager();
    }

    [ComImport]
    [Guid("D16679F2-6CA0-472D-8D31-2F5D55AEE155")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IWMProfileManager
    {
        IntPtr CreateEmptyProfile([In] uint dwVersion);
        IntPtr LoadProfileByID([In, Out] ref Guid guidProfile);
        IntPtr LoadProfileByData([In] string pwszProfile);
        void SaveProfile([In] IntPtr pProfile, [In] string pwszProfile, [In, Out] ref uint pdwLength);
        uint GetSystemProfileCount();
        IntPtr LoadSystemProfile([In] uint dwProfileIndex);
    }
}