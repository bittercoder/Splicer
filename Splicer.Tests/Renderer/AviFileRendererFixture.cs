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
using Splicer.Renderer;
using Splicer.Timeline;

namespace Splicer.Tests.Renderer
{
    [TestFixture]
    public class AviFileRendererFixture : AbstractFixture
    {
        [Test]
        [ExpectedException(typeof (SplicerException), "Can not render to AVI when no video group exists")]
        public void RenderWithoutVideo()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();

                using (IRenderer renderer = new AviFileRenderer(timeline, null))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Output file name cannot be null")]
        public void RenderWithNoFilename()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 80);
                ITrack track = group.AddTrack();
                track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, 2);

                using (IRenderer renderer = new AviFileRenderer(timeline, null))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        public void RenderVideoOnly()
        {
            string outputFile = "RenderVideoOnly.avi";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 80);
                ITrack track = group.AddTrack();
                track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, 2);

                using (IRenderer renderer = new AviFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [Test]
        public void RenderVideoAndAudio()
        {
            string outputFile = "RenderVideoAndAudio.avi";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 100, 80);
                ITrack videoTrack = videoGroup.AddTrack();
                videoTrack.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, 2);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                using (IRenderer renderer = new AviFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }

                AssertLengths(timeline, 2, outputFile);
            }
        }
    }
}