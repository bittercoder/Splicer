using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.DES;

namespace Splicer.Utils
{
    public static class PinUtils
    {
        /// <summary>
        /// Determine whether a specified pin is audio or video
        /// </summary>
        /// <param name="pPin">Pin to check</param>
        /// <returns>True if pin is video</returns>
        public static bool IsVideo(IPin pPin)
        {
            int hr;
            bool bRet = false;
            AMMediaType[] pmt = new AMMediaType[1];
            IEnumMediaTypes ppEnum;
            int i;

            // Walk the MediaTypes for the pin
            hr = pPin.EnumMediaTypes(out ppEnum);
            DESError.ThrowExceptionForHR(hr);

            try
            {
                // Just read the first one
                hr = ppEnum.Next(1, pmt, out i);
                DESError.ThrowExceptionForHR(hr);

                bRet = pmt[0].majorType == MediaType.Video;
            }
            finally
            {
                Marshal.ReleaseComObject(ppEnum);
            }
            DsUtils.FreeAMMediaType(pmt[0]);

            return bRet;
        }
    }
}