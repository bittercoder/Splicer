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

using System.Globalization;
using System.IO;

namespace Splicer.WindowsMedia
{
    public static class WindowsMediaProfiles
    {
        public static string HighQualityVideo
        {
            get { return ReadStream("HighQualityVideo.prx"); }
        }

        public static string LowQualityAudio
        {
            get { return ReadStream("LowQualityAudio.prx"); }
        }

        public static string LowQualityVideo
        {
            get { return ReadStream("LowQualityVideo.prx"); }
        }

        public static string MediumQualityAudio
        {
            get { return ReadStream("MediumQualityAudio.prx"); }
        }


        private static string ReadStream(string profileName)
        {
            using (
                Stream stream =
                    typeof (WindowsMediaProfiles).Assembly.GetManifestResourceStream(
                        string.Format(CultureInfo.InvariantCulture, "Splicer.WindowsMedia.{0}", profileName)))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}