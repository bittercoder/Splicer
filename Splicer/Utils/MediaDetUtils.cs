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