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

using System;
using System.Runtime.InteropServices;
using DirectShowLib;

namespace Splicer.Utils
{
    public static class MediaTypeUtils
    {
        /// <summary>
        /// Create a video media type from a few parameters
        /// </summary>
        /// <param name="BitCount">Bits per pixel (16, 24, 32)</param>
        /// <param name="Width">Video width</param>
        /// <param name="Height">Video height</param>
        /// <returns>The constructed AMMediaType</returns>
        public static AMMediaType GetVideoMediaType(short BitCount, int Width, int Height)
        {
            Guid mediaSubType;
            AMMediaType VideoGroupType = new AMMediaType();

            // Calculate the SubType from the Bit count
            switch (BitCount)
            {
                case 16:
                    mediaSubType = MediaSubType.RGB555;
                    break;
                case 24:
                    mediaSubType = MediaSubType.RGB24;
                    break;
                case 32:
                    mediaSubType = MediaSubType.ARGB32;
                    break;
                default:
                    throw new Exception("Unrecognized bit format");
            }

            VideoGroupType.majorType = MediaType.Video;
            VideoGroupType.subType = mediaSubType;
            VideoGroupType.formatType = FormatType.VideoInfo;
            VideoGroupType.fixedSizeSamples = true;

            VideoGroupType.formatSize = Marshal.SizeOf(typeof (VideoInfoHeader));
            VideoInfoHeader vif = new VideoInfoHeader();
            vif.BmiHeader = new BitmapInfoHeader();

            // The HEADER macro returns the BITMAPINFO within the VIDEOINFOHEADER
            vif.BmiHeader.Size = Marshal.SizeOf(typeof (BitmapInfoHeader));
            vif.BmiHeader.Compression = 0;
            vif.BmiHeader.BitCount = BitCount;
            vif.BmiHeader.Width = Width;
            vif.BmiHeader.Height = Height;
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
            AMMediaType AudioGroupType = new AMMediaType();
            AudioGroupType.majorType = MediaType.Audio;

            return AudioGroupType;
        }
    }
}