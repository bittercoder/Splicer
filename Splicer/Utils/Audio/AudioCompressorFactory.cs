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

using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public static class AudioCompressorFactory
    {
        public static AudioCompressor Create(AudioFormat format)
        {
            AudioEncoder encoder = AudioEncoder.FindByFriendlyName(format.AudioCompressor);
            if (encoder == null)
                throw new SplicerException(
                    string.Format("Can not resolve audio encoder \"{0}\"", format.AudioCompressor));

            if (!format.UseDefaults)
            {
                WavFormatInfo formatInfo = WavFormatInfoUtils.FindFormat(encoder.Filter, PinDirection.Output, format);
                if (formatInfo == null)
                    throw new SplicerException(
                        string.Format("Can not resolve media type for encoder ({0} khz {1}, kbps, mono? {2})",
                                      format.Khz, format.Kbps, format.IsMono));

                return new AudioCompressor(encoder.Filter, formatInfo.MediaType);
            }
            else
            {
                return new AudioCompressor(encoder.Filter, null);
            }
        }
    }
}