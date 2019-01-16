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

using System.IO;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splicer.Timeline;
using Splicer.WindowsMedia;

namespace Splicer.Renderer.Tests
{
    [TestClass]
    public class WindowsMediaRendererFixture : AbstractFixture
    {
        [TestMethod]
        public void ConvertAviToWMV()
        {
            string outputFile = "ConvertAviToWMV.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(0x20, 320, 240);
                ITrack videoTrack = videoGroup.AddTrack();
                IClip clockClip =
                    videoTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, 2);

                Assert.IsTrue(clockClip.Duration > 0);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""video"" bitdepth=""32"" framerate=""30.0000000"" previewmode=""0"">
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

                Assert.IsTrue(File.Exists(outputFile));
                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        public void ConvertMp3ToWMA()
        {
            string outputFile = "ConvertMp3ToWMA.wma";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityAudio))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""..\..\testinput.mp3"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }

                Assert.IsTrue(File.Exists(outputFile));
                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        public void AddSoundTrackToWav()
        {
            string outputFile = "SoundOnWav.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(0x20, 320, 240);
                ITrack videoTrack = videoGroup.AddTrack();
                IClip clockClip =
                    videoTrack.AddVideo("..\\..\\transitions.wmv", 0, 0, 10);

                Assert.IsTrue(clockClip.Duration > 0);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 10);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""video"" bitdepth=""32"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""10"" src=""..\..\transitions.wmv"" mstart=""0"" />
        </track>
    </group>
    <group type = ""audio"" framerate = ""30.0000000"" previewmode = ""0"">
        <track>
            <clip start=""0"" stop=""10"" src=""..\..\testinput.mp3"" mstart=""0"" />
        </track>
    </group>
    </timeline>");
                }

                Assert.IsTrue(File.Exists(outputFile));
                AssertLengths(timeline, 10, outputFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "Missing exception for: Output file name is null or invalid")]
        public void ConvertWithNullFileName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

#pragma warning disable 642
                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, null, WindowsMediaProfiles.LowQualityAudio)) ;
#pragma warning restore 642
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "The selected windows media profile encodes video information, yet no video group exists")]
        public void RenderWithInapropriateProfile1()
        {
            string outputFile = "RenderWithInapropriateProfile2.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
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
        [ExpectedException(typeof (SplicerException),
            "Missing exception for: The selected windows media profile encodes audio information, yet no audio group exists")
        ]
        public void RenderWithInapropriateProfile2()
        {
            string outputFile = "RenderWithInapropriateProfile2.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 100, 100);
                ITrack rootTrack = videoGroup.AddTrack();
                rootTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, 2);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.LowQualityVideo))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""..\..\testinput.mp3"" mstart=""0"" />
		</track>
	</group>
</timeline>");
                }
            }
        }
    }
}