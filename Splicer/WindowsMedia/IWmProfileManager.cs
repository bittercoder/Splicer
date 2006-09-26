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