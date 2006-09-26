using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public static class WavFormatInfoUtils
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
                else
                {
                    enumerator.Current.Dispose();
                }
            }

            return null;
        }

        public static IEnumerator<WavFormatInfo> EnumerateFormatsForDirection(IBaseFilter filter, PinDirection direction)
        {
            if (filter == null) throw new ArgumentNullException("filter");
            int hr;

            IEnumPins pinsEnum = null;
            try
            {
                hr = filter.EnumPins(out pinsEnum);
                DsError.ThrowExceptionForHR(hr);

                if (pinsEnum == null) throw new InvalidOperationException("pinsEnum is null");

                IPin[] pins = new IPin[1];

                while (true)
                {
                    try
                    {
                        int pcFetched;
                        hr = pinsEnum.Next(pins.Length, pins, out pcFetched);
                        DsError.ThrowExceptionForHR(hr);

                        if (pcFetched == 1)
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

                                AMMediaType[] mediaTypes = new AMMediaType[1];

                                int mtFetched;

                                while (true)
                                {
                                    hr = mediaTypesEnum.Next(1, mediaTypes, out mtFetched);
                                    DsError.ThrowExceptionForHR(hr);
                                    if (mtFetched == 1)
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