using System.Runtime.InteropServices;
using DirectShowLib.DES;
using Splicer.Timeline;

namespace Splicer.Utils
{
    public class MediaDetUtils
    {
        public static long GetLength(string filename)
        {
            int hr;
            double d;
            long i;

            IMediaDet imd = (IMediaDet) new MediaDet();

            // Set the name
            hr = imd.put_Filename(filename);
            DESError.ThrowExceptionForHR(hr);

            // Read from stream zero
            hr = imd.put_CurrentStream(0);
            DESError.ThrowExceptionForHR(hr);

            // Get the length in seconds
            hr = imd.get_StreamLength(out d);
            DESError.ThrowExceptionForHR(hr);

            Marshal.ReleaseComObject(imd);

            // Convert to UNITS
            i = (long) (d*TimelineUtils.UNITS);
            return i;
        }
    }
}