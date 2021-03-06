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
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splicer.Renderer;
using Splicer.Timeline;

namespace Splicer.Tests.Renderer
{
    [TestClass]
    public class AviFileRendererFixture : AbstractFixture
    {
        [TestMethod]
        public void RenderVideoAndAudio()
        {
            string outputFile = "RenderVideoAndAudio.avi";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 100, 80);
                ITrack videoTrack = videoGroup.AddTrack();
                videoTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 3, 5);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.wav", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                using (var renderer = new AviFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        public void RenderVideoOnly()
        {
            string outputFile = "RenderVideoOnly.avi";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 80);
                ITrack track = group.AddTrack();
                track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 2, 4);

                using (var renderer = new AviFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RenderWithNoFileName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 80);
                ITrack track = group.AddTrack();
                track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, 2);

                using (var renderer = new AviFileRenderer(timeline, null))
                {
                    renderer.Render();
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void RenderWithoutVideo()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();

                using (var renderer = new AviFileRenderer(timeline, null))
                {
                    renderer.Render();
                }
            }
        }
    }
}