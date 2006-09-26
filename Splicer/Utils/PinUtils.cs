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