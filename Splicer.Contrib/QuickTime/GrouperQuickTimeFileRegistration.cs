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
using System.Text;
using Splicer.Utils;

namespace Splicer.Contrib.QuickTime
{
    /// <summary>
    /// Derived from MediaFileRegistration, takes care of registering all the media extensions required.
    /// </summary>
    public class GrouperQuickTimeFileRegistration : MediaFileRegistration
    {
        private static readonly string[] _videoExtensions;
        private static readonly string[] _audioExtensions;
        private static readonly Guid GVQuickTimeFilterId = new Guid("CB83D662-BEFE-4dbf-830C-25E52627C1C3");

        static GrouperQuickTimeFileRegistration()
        {
            _videoExtensions = new string[] {".mov", ".mp4", ".m4v", ".qt", ".3gp", ".3gpp", ".3g2", ".3gpp2"};
            _audioExtensions = new string[] {".aac", ".m4a", ".amr"};
            Array.Sort(_videoExtensions);
            Array.Sort(_audioExtensions);
        }

        public GrouperQuickTimeFileRegistration()
        {
            if (GrouperQuickTimeWrapper.IsGVQuickTimeRegistered)
            {
                foreach (string videoExtension in _videoExtensions)
                {
                    Add(new RegsiteredMediaExtension(GVQuickTimeFilterId, videoExtension));
                }

                foreach (string audioExtension in _audioExtensions)
                {
                    Add(new RegsiteredMediaExtension(GVQuickTimeFilterId, audioExtension));
                }
            }
        }

        public static string GetAudioExtensionsAsString(string separator)
        {
            StringBuilder output = new StringBuilder();
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
            StringBuilder output = new StringBuilder();
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

        public static bool IsQTAudioExtension(string ext)
        {
            return (Array.BinarySearch<string>(_audioExtensions, ext.ToLower()) >= 0);
        }

        public static bool IsQTVideoExtension(string ext)
        {
            return (Array.BinarySearch<string>(_videoExtensions, ext.ToLower()) >= 0);
        }

        public static string[] QTVideoExtensions
        {
            get { return _videoExtensions; }
        }

        public static string[] QTAudioExtensions
        {
            get { return _audioExtensions; }
        }
    }
}