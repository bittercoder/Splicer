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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;
using Splicer.Properties;

namespace Splicer.Utilities.Audio
{
    public class WavFormatInfo : IDisposable
    {
        private readonly int _averageBytesPerSecond;
        private readonly int _bitsPerSample;
        private readonly int _blockAlign;
        private readonly int _channels;
        private readonly int _formatTag;
        private readonly int _kbps;
        private readonly int _khz;
        private readonly int _samplesPerSecond;
        private readonly int _size;
        private AMMediaType _mediaType;

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public WavFormatInfo(AMMediaType type)
        {
            if (type == null) throw new ArgumentNullException("type");

            _mediaType = type;

            if (type.formatType != FormatType.WaveEx)
            {
                throw new SplicerException(Resources.UnsupportedAMMEdiaType);
            }

            var formatEx = Marshal.PtrToStructure(type.formatPtr, typeof (WaveFormatEx)) as WaveFormatEx;
            _size = formatEx.cbSize;
            _averageBytesPerSecond = formatEx.nAvgBytesPerSec;
            _blockAlign = formatEx.nBlockAlign;
            _channels = formatEx.nChannels;
            _samplesPerSecond = formatEx.nSamplesPerSec;
            _bitsPerSample = formatEx.wBitsPerSample;
            _formatTag = formatEx.wFormatTag;
            _khz = _samplesPerSecond/1000;
            _kbps = (_averageBytesPerSecond*8)/1000;
        }

        public AMMediaType MediaType
        {
            get { return _mediaType; }
        }

        public int Size
        {
            get { return _size; }
        }

        public int AverageBytesPerSec
        {
            get { return _averageBytesPerSecond; }
        }

        public int BlockAlign
        {
            get { return _blockAlign; }
        }

        public int Channels
        {
            get { return _channels; }
        }

        public int SamplesPerSecond
        {
            get { return _samplesPerSecond; }
        }

        public int BitsPerSample
        {
            get { return _bitsPerSample; }
        }

        public int FormatTag
        {
            get { return _formatTag; }
        }

        public bool IsMono
        {
            get { return _channels < 2; }
        }

        public int Khz
        {
            get { return _khz; }
        }

        public int Kbps
        {
            get { return _kbps; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public override string ToString()
        {
            string channelName = null;

            if (IsMono)
            {
                channelName = Resources.Mono;
            }
            else
            {
                channelName = Resources.Stereo;
            }

            return string.Format(CultureInfo.InvariantCulture, Resources.WavFormatInfoTemplate, channelName, Khz, Kbps);
        }

        ~WavFormatInfo()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_mediaType != null)
            {
                DsUtils.FreeAMMediaType(_mediaType);
                _mediaType = null;
            }
        }
    }
}