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

namespace Splicer.Timeline
{
    public class AudioVideoClipPair : IAudioVideoClipPair
    {
        private IClip _videoClip;
        private IClip _audioClip;

        public AudioVideoClipPair(IClip audioClip, IClip videoClip)
        {
            if (audioClip == null) throw new ArgumentNullException("audioClip");
            if (videoClip == null) throw new ArgumentNullException("videoClip");

            _audioClip = audioClip;
            _videoClip = videoClip;
        }

        public IClip AudioClip
        {
            get { return _audioClip; }
        }

        public IClip VideoClip
        {
            get { return _videoClip; }
        }
    }
}