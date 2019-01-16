using System;
using System.Runtime.InteropServices;

namespace Splicer.Renderer
{
    public static class NativeMethods
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        internal static extern void CopyMemory(IntPtr Destination, IntPtr Source,
                                               [MarshalAs(UnmanagedType.U4)] uint Length);
    }
}