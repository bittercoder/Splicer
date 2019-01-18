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
using System.IO;
using Splicer.Timeline;

namespace Splicer.Contrib.QuickTime
{
    public sealed class GrouperMediaFileAssistant : IMediaFileAssistant
    {
        #region IMediaFileAssistant Members

        public bool WillAssist(MediaFile file)
        {
            if (file == null) throw new ArgumentNullException("file");

            string extension = Path.GetExtension(file.FileName);
            if (extension.StartsWith(".")) extension = extension.Substring(1);

            if (GrouperQuickTimeFileRegistration.IsQTAudioExtension(extension) ||
                GrouperQuickTimeFileRegistration.IsQTVideoExtension(extension))
            {
                return true;
            }

            return false;
        }

        public IDisposable Assist(MediaFile file)
        {
            if (file == null) throw new ArgumentNullException("file");

            return new GrouperQuickTimeFileRegistration();
        }

        #endregion
    }
}