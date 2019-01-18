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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace Splicer.Utilities.Audio
{
    public static class WavFormatInfoTools
    {
        public static WavFormatInfo FindFormat(IBaseFilter filter, PinDirection direction, AudioFormat format)
        {
            IEnumerator<WavFormatInfo> enumerator = EnumerateFormatsForDirection(filter, direction);

            while (enumerator.MoveNext())
            {
                if ((enumerator.Current.IsMono == format.IsMono) && (enumerator.Current.Khz == format.Khz) &&
                    (enumerator.Current.Kbps == format.Kbps))
                {
                    return enumerator.Current;
                }

                enumerator.Current.Dispose();
            }

            return null;
        }

        public static IEnumerator<WavFormatInfo> EnumerateFormatsForDirection(IBaseFilter filter, PinDirection direction)
        {
            if (filter == null) throw new ArgumentNullException("filter");

            IEnumPins pinsEnum = null;
            try
            {
                int hr = filter.EnumPins(out pinsEnum);
                DsError.ThrowExceptionForHR(hr);

                if (pinsEnum == null) throw new InvalidOperationException("pinsEnum is null");

                var pins = new IPin[1];

                while (true)
                {
                    try
                    {
                        int fetched = 0;
                        IntPtr pcFetched = Marshal.AllocCoTaskMem(4);
                        try
                        {
                            hr = pinsEnum.Next(pins.Length, pins, pcFetched);
                            DsError.ThrowExceptionForHR(hr);

                            fetched = Marshal.ReadInt32(pcFetched);
                        }
                        finally
                        {
                            Marshal.FreeCoTaskMem(pcFetched);
                        }

                        if (fetched == 1)
                        {
                            // we have something
                            IPin pin = pins[0];

                            string queryId;
                            hr = pin.QueryId(out queryId);
                            DsError.ThrowExceptionForHR(hr);

                            PinInfo pinInfo;
                            hr = pin.QueryPinInfo(out pinInfo);
                            DsError.ThrowExceptionForHR(hr);

                            if (pinInfo.dir != direction) continue;

                            IEnumMediaTypes mediaTypesEnum = null;

                            try
                            {
                                hr = pin.EnumMediaTypes(out mediaTypesEnum);
                                DsError.ThrowExceptionForHR(hr);

                                var mediaTypes = new AMMediaType[1];


                                while (true)
                                {
                                    IntPtr mtFetched = Marshal.AllocCoTaskMem(4);
                                    try
                                    {
                                        hr = mediaTypesEnum.Next(1, mediaTypes, mtFetched);
                                        DsError.ThrowExceptionForHR(hr);
                                        fetched = Marshal.ReadInt32(mtFetched);
                                    }
                                    finally
                                    {
                                        Marshal.FreeCoTaskMem(mtFetched);
                                    }

                                    if (fetched == 1)
                                    {
                                        if (mediaTypes[0].formatType == FormatType.WaveEx)
                                        {
                                            yield return new WavFormatInfo(mediaTypes[0]);
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                            finally
                            {
                                if (mediaTypesEnum != null) Marshal.ReleaseComObject(mediaTypesEnum);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    finally
                    {
                        if (pins[0] != null) Marshal.ReleaseComObject(pins[0]);
                        pins[0] = null;
                    }
                }
            }
            finally
            {
                if (pinsEnum != null) Marshal.ReleaseComObject(pinsEnum);
            }
        }
    }
}