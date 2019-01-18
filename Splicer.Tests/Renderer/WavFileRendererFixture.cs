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
using Splicer.Utilities.Audio;

namespace Splicer.Renderer.Tests
{
    [TestClass]
    public class WavFileRendererFixture : AbstractFixture
    {
        [TestInitialize]
        public void SetUp()
        {
            File.Delete("RenderWithCompressor.wav");
            File.Delete("RenderWithoutAudioGroup.wav");
            File.Delete("ConvertWavToWav.wav");
            File.Delete("ConvertMp3ToWavWithCompressor.wav");
        }

        [TestMethod]
        public void ConvertMp3ToWavWithCompressor()
        {
            string outputFile = "ConvertMp3ToWavWithCompressor.wav";

            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                // render the timeline
                using (
                    var renderer =
                        new WavFileRenderer(timeline, outputFile, AudioFormat.LowQualityMonoPcm,
                                            new ICallbackParticipant[] {new ConsoleProgressParticipant()}))
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

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        public void ConvertWavToWav()
        {
            string outputFile = "ConvertWavToWav.wav";

            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                // render the timeline                
                using (var renderer = new WavFileRenderer(timeline, outputFile))
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

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [TestMethod]
        public void RenderWithCompressor()
        {
            string outputFile = "RenderWithCompressor.wav";

            using (
                AudioCompressor compressor =
                    AudioCompressorFactory.Create(AudioFormat.CompactDiscQualityStereoPcm))
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("..\\..\\testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                // render the timeline
                using (
                    var renderer =
                        new WavFileRenderer(timeline, outputFile, compressor.Filter, compressor.MediaType, null))
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

                AssertLengths(timeline.Fps, 2, outputFile);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "Missing exception for: Output file name is null or invalid")]
        public void RenderWithNullFile()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();

                // render the timeline

#pragma warning disable 642
                using (var renderer = new WavFileRenderer(timeline, null)) ;
#pragma warning restore 642
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException), "Missing exception for: No audio group to render")]
        public void RenderWithoutAudioGroup()
        {
            string outputFile = "RenderWithoutAudioGroup.wav";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 200, 200);

                // render the timeline
                using (var renderer = new WavFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }
            }
        }
    }
}