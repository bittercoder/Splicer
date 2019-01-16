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
using Splicer.Timeline;

namespace Splicer.Renderer.Tests
{
    [TestClass]
    public class NullRendererFixture : AbstractFixture
    {
        [TestMethod]
        public void AddAndRemoveHandler()
        {
            bool eventTriggered = false;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack track = audioGroup.AddTrack();
                track.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, -1);

                using (var renderer = new NullRenderer(timeline))
                {
                    EventHandler handler = delegate { eventTriggered = true; };

                    renderer.RenderCompleted += handler;
                    renderer.RenderCompleted -= handler;

                    renderer.BeginRender(null, null);
                    renderer.Cancel();

                    Assert.IsFalse(eventTriggered);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException), "Missing exception: Graph not yet started")]
        public void CancelBeforeStart()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack track = audioGroup.AddTrack();
                track.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, -1);

                using (var renderer = new NullRenderer(timeline))
                {
                    renderer.Cancel();
                }
            }
        }

        [TestMethod]
        public void CancelRender()
        {
            bool eventTriggered = false;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack track = audioGroup.AddTrack();
                track.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, -1);

                using (var renderer = new NullRenderer(timeline))
                {
                    renderer.RenderCompleted += delegate { eventTriggered = true; };

                    renderer.BeginRender(null, null);
                    renderer.Cancel();

                    Assert.AreEqual(RendererState.Canceled, renderer.State);
                    Assert.IsTrue(eventTriggered);
                }
            }
        }

        [TestMethod]
        public void CanRenderAudioVideoAndImages()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                IGroup videoGroup = timeline.AddVideoGroup(24, 160, 100);
                ITrack videoTrack = videoGroup.AddTrack();
                videoTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Relative, 0, 0, 1);
                videoTrack.AddClip("..\\..\\image1.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 1);

                using (var renderer = new NullRenderer(timeline))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""..\..\testinput.mp3"" mstart=""0"" />
		</track>
	</group>
	<group type=""video"" bitdepth=""24"" width=""160"" height=""100"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""1"" src=""..\..\transitions.wmv"" mstart=""0"" />
			<clip start=""1"" stop=""2"" src=""..\..\image1.jpg"" />
		</track>
	</group>
</timeline>");
                }
            }
        }

        [TestMethod]
        public void RenderAudio()
        {
            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                // render the timeline
                using (var renderer = new NullRenderer(timeline))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""..\..\testinput.wav"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }
            }
        }

        [TestMethod]
        public void RenderAudioAndVideo()
        {
            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 320, 240);
                ITrack videoTrack = videoGroup.AddTrack();
                videoTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Relative, 0, 0, 2);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                // render the timeline
                using (var renderer = new NullRenderer(timeline))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
	<group type=""video"" bitdepth=""24"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""..\..\transitions.wmv"" mstart=""0"" />
		</track>
	</group>
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""..\..\testinput.mp3"" mstart=""0"" />
		</track>
	</group>
</timeline>");
                }
            }
        }

        [TestMethod]
        public void RenderToCompletion()
        {
            bool eventTriggered = false;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack track = audioGroup.AddTrack();
                track.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 1);

                using (var renderer = new NullRenderer(timeline))
                {
                    renderer.RenderCompleted += delegate { eventTriggered = true; };

                    renderer.Render();

                    Assert.AreEqual(RendererState.GraphCompleted, renderer.State);
                    Assert.IsTrue(eventTriggered);
                }
            }
        }

        [TestMethod]
        public void RenderVideo()
        {
            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 320, 240);
                ITrack rootTrack = videoGroup.AddTrack();
                rootTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Relative, 5, 0, 2);

                // render the timeline
                using (var renderer = new NullRenderer(timeline))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""video"" bitdepth=""24"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""5"" stop=""7"" src=""..\..\transitions.wmv"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }
            }
        }
    }
}