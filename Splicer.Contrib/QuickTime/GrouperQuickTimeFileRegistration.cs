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
using System.Text;
using Splicer.Utilities;

namespace Splicer.Contrib.QuickTime
{
    /// <summary>
    /// Derived from MediaFileRegistration, takes care of registering all the media extensions required.
    /// </summary>
    public sealed class GrouperQuickTimeFileRegistration : MediaFileRegistration
    {
        private static readonly string[] _audioExtensions = new[] {".aac", ".amr", ".m4a"};

        private static readonly string[] _videoExtensions =
            new[] {".qt", ".3g2", ".3gp", ".3gpp", ".3gpp2", ".m4v", ".mov", ".mp4"};

        private static readonly Guid GVQuickTimeFilterId = new Guid("CB83D662-BEFE-4dbf-830C-25E52627C1C3");

        public GrouperQuickTimeFileRegistration()
        {
            if (GrouperQuickTimeWrapper.IsGVQuickTimeRegistered)
            {
                foreach (string videoExtension in _videoExtensions)
                {
                    Add(new RegisteredMediaExtension(GVQuickTimeFilterId, videoExtension));
                }

                foreach (string audioExtension in _audioExtensions)
                {
                    Add(new RegisteredMediaExtension(GVQuickTimeFilterId, audioExtension));
                }
            }
        }

        public static string GetAudioExtensionsAsString(string separator)
        {
            if (separator == null) throw new ArgumentNullException("separator");

            var output = new StringBuilder();
            foreach (string extension in _audioExtensions)
            {
                if (output.Length > 0)
                {
                    output.Append(separator);
                }
                output.Append(extension);
            }
            return output.ToString();
        }

        public static string GetVideoExtensionsAsString(string separator)
        {
            if (separator == null) throw new ArgumentNullException("separator");

            var output = new StringBuilder();
            foreach (string extension in _videoExtensions)
            {
                if (output.Length > 0)
                {
                    output.Append(separator);
                }
                output.Append(extension);
            }
            return output.ToString();
        }

        public static bool IsQTAudioExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException("extension");
            return (Array.BinarySearch(_audioExtensions, extension.ToLower(CultureInfo.InvariantCulture)) >= 0);
        }

        public static bool IsQTVideoExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException("extension");
            return (Array.BinarySearch(_videoExtensions, extension.ToLower(CultureInfo.InvariantCulture)) >= 0);
        }

        public static string[] GetQTVideoExtensions()
        {
            return _videoExtensions;
        }

        public static string[] GetQTAudioExtensions()
        {
            return _audioExtensions;
        }
    }
}