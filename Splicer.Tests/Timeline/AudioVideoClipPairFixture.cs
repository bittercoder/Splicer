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
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class AudioVideoClipPairFixture
    {
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullAudioClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip videoClip = timeline.AddVideoGroup(24, 100, 100).AddTrack().AddVideo("1sec.wmv");
                AudioVideoClipPair pair = new AudioVideoClipPair(null, videoClip);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullVideoClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip audioClip = timeline.AddAudioGroup().AddTrack().AddAudio("1sec.wav");
                AudioVideoClipPair pair = new AudioVideoClipPair(audioClip, null);
            }
        }
    }
}