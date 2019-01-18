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
using System.Security.Permissions;
using System.Text;
using DirectShowLib;
using Splicer.Properties;

namespace Splicer.Utilities.Audio
{
    public static class AudioCompressorFactory
    {
        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static AudioCompressor Create(AudioFormat format)
        {
            if (format == null) throw new ArgumentNullException("format");

            AudioEncoder encoder = AudioEncoder.FindByFriendlyName(format.AudioCompressor);
            if (encoder == null)
                throw new SplicerException(
                    string.Format(CultureInfo.CurrentUICulture, Resources.ErrorCanResolveAudioEncoder,
                                  format.AudioCompressor));

            if (!format.UseDefaults)
            {
                WavFormatInfo formatInfo = WavFormatInfoTools.FindFormat(encoder.Filter, PinDirection.Output, format);
                if (formatInfo == null)
                {
                    var builder = new StringBuilder();
                    builder.AppendFormat(CultureInfo.CurrentUICulture, Resources.ErrorCanNotResolveMediaTypeForEncoder,
                                         format.Khz, format.Kbps, format.IsMono);

                    foreach (WavFormatInfo info in encoder.Formats)
                    {
                        builder.Append(Environment.NewLine);
                        builder.Append(info.ToString());
                    }

                    throw new SplicerException(builder.ToString());
                }

                return new AudioCompressor(encoder.Filter, formatInfo.MediaType);
            }
            else
            {
                return new AudioCompressor(encoder.Filter, null);
            }
        }
    }
}