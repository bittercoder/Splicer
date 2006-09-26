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

using NUnit.Framework;
using Splicer.Timeline;

namespace Splicer.Renderer.Tests
{
    [TestFixture]
    public class ConsoleProgressCallbackFixture : AbstractFixture
    {
        [Test]
        public void UseInRender()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                using (NullRenderer renderer = new NullRenderer(timeline, new ConsoleProgressCallback(), null))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""testinput.wav"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }
            }
        }
    }
}