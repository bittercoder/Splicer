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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;
using Splicer.Properties;

namespace Splicer.Utilities
{
    public static class MediaTypeTools
    {
        /// <summary>
        /// Gets the media sub type for the supplied bit count
        /// </summary>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public static Guid GetMediaSubTypeForBitCount(short bitCount)
        {
            switch (bitCount)
            {
                case 16:
                    return MediaSubType.RGB555;
                case 24:
                    return MediaSubType.RGB24;
                case 32:
                    return MediaSubType.ARGB32;
                default:
                    throw new SplicerException(Resources.ErrorUnrecognizedBitFormat);
            }
        }

        /// <summary>
        /// Gets the pixel format for the supplied bit count
        /// </summary>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public static PixelFormat GetPixelFormatForBitCount(short bitCount)
        {
            switch (bitCount)
            {
                case 16:
                    return PixelFormat.Format16bppRgb555;
                case 24:
                    return PixelFormat.Format24bppRgb;
                case 32:
                    return PixelFormat.Format32bppArgb;
                default:
                    throw new SplicerException(Resources.ErrorUnrecognizedBitFormat);
            }
        }

        /// <summary>
        /// Create a video media type from a few parameters
        /// </summary>
        /// <param name="bitCount">Bits per pixel (16, 24, 32)</param>
        /// <param name="width">Video width</param>
        /// <param name="height">Video height</param>
        /// <returns>The constructed AMMediaType</returns>
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public static AMMediaType GetVideoMediaType(short bitCount, int width, int height)
        {
            Guid mediaSubType = GetMediaSubTypeForBitCount(bitCount);
            var VideoGroupType = new AMMediaType();

            VideoGroupType.majorType = MediaType.Video;
            VideoGroupType.subType = mediaSubType;
            VideoGroupType.formatType = FormatType.VideoInfo;
            VideoGroupType.fixedSizeSamples = true;

            VideoGroupType.formatSize = Marshal.SizeOf(typeof (VideoInfoHeader));
            var vif = new VideoInfoHeader();
            vif.BmiHeader = new BitmapInfoHeader();

            // The HEADER macro returns the BITMAPINFO within the VIDEOINFOHEADER
            vif.BmiHeader.Size = Marshal.SizeOf(typeof (BitmapInfoHeader));
            vif.BmiHeader.Compression = 0;
            vif.BmiHeader.BitCount = bitCount;
            vif.BmiHeader.Width = width;
            vif.BmiHeader.Height = height;
            vif.BmiHeader.Planes = 1;

            int iSampleSize = vif.BmiHeader.Width*vif.BmiHeader.Height*(vif.BmiHeader.BitCount/8);
            vif.BmiHeader.ImageSize = iSampleSize;
            VideoGroupType.sampleSize = iSampleSize;
            VideoGroupType.formatPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(vif));

            Marshal.StructureToPtr(vif, VideoGroupType.formatPtr, false);

            return VideoGroupType;
        }

        /// <summary>
        /// Create an audio media type
        /// </summary>
        /// <returns>The constructed media type</returns>
        public static AMMediaType GetAudioMediaType()
        {
            var AudioGroupType = new AMMediaType();
            AudioGroupType.majorType = MediaType.Audio;

            return AudioGroupType;
        }
    }
}