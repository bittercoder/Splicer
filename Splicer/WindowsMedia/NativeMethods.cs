using System.Runtime.InteropServices;

namespace Splicer.WindowsMedia
{
    public static class NativeMethods
    {
        [
            DllImport("WMVCore.dll", EntryPoint = "WMCreateProfileManager", PreserveSig = false, SetLastError = true,
                ExactSpelling = true)]
        internal static extern IWMProfileManager WMCreateProfileManager();
    }
}