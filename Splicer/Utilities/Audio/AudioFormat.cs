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

namespace Splicer.Utilities.Audio
{
    public class AudioFormat
    {
        public const string Ac3AcmFriendlyName = "AC-3 ACM Codec";
        public const string MpegLayer3FriendlyName = "MPEG Layer-3";
        public const string OggMode3PlusFriendlyName = "Ogg Vorbis (mode3+)";
        public const string PcmFriendlyName = "PCM";

        public AudioFormat()
        {
        }

        public AudioFormat(string audioCompressor)
        {
            AudioCompressor = audioCompressor;
            UseDefaults = true;
        }

        public AudioFormat(string audioCompressor, bool isMono, int khz, int kbps)
        {
            AudioCompressor = audioCompressor;
            IsMono = isMono;
            Khz = khz;
            Kbps = kbps;
        }

        /// <summary>
        /// Low quality Ac3 encoding (mono, 32khz, 32kbps)
        /// </summary>
        public static AudioFormat LowQualityAc3Acm
        {
            get { return new AudioFormat(Ac3AcmFriendlyName, true, 32, 32); }
        }

        /// <summary>
        /// High quality Ac3 encoding (stereo, 44khz, 640kbps)
        /// </summary>
        public static AudioFormat HighQualityAc3Acm
        {
            get { return new AudioFormat(Ac3AcmFriendlyName, false, 44, 640); }
        }

        /// <summary>
        /// Low quality ogg mode 3+ encodign (mono, 22khz, 64kbps)
        /// </summary>
        public static AudioFormat LowQualityMonoOggMode3Plus
        {
            get { return new AudioFormat(OggMode3PlusFriendlyName, true, 22, 64); }
        }

        /// <summary>
        /// the default ogg mode 3+ encoding
        /// </summary>
        public static AudioFormat DefaultOggMode3Plus
        {
            get { return new AudioFormat(OggMode3PlusFriendlyName); }
        }

        /// <summary>
        /// Low quality mono PCM encoding (mono, 32khz, 256kpbs)
        /// </summary>
        public static AudioFormat LowQualityMonoPcm
        {
            get { return new AudioFormat(PcmFriendlyName, true, 32, 256); }
        }

        /// <summary>
        /// Compact disc quality stereo PCM encoding (stereo, 44khz, 1411kbps)
        /// </summary>
        public static AudioFormat CompactDiscQualityStereoPcm
        {
            get { return new AudioFormat(PcmFriendlyName, false, 44, 1411); }
        }

        public string AudioCompressor { get; set; }

        public bool IsMono { get; set; }

        public int Khz { get; set; }

        public int Kbps { get; set; }

        public bool UseDefaults { get; set; }
    }
}