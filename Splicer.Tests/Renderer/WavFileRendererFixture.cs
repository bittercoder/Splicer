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

using System.IO;
using NUnit.Framework;
using Splicer.Timeline;
using Splicer.Utils.Audio;

namespace Splicer.Renderer.Tests
{
    [TestFixture]
    public class WavFileRendererFixture : AbstractFixture
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            File.Delete("RenderWithCompressor.wav");
            File.Delete("RenderWithoutAudioGroup.wav");
            File.Delete("ConvertWavToWav.wav");
            File.Delete("ConvertMp3ToWavWithCompressor.wav");
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Output file name cannot be null")]
        public void RenderWithNullFile()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();

                // render the timeline
                using (WavFileRenderer renderer = new WavFileRenderer(timeline, null)) ;
            }
        }

        [Test]
        public void RenderWithCompressor()
        {
            string outputFile = "RenderWithCompressor.wav";

            AudioCompressor compressor = null;
            try
            {
                compressor = AudioCompressorFactory.Create(CommonAudioFormats.LowQualityMonoPcm);

                // create the timeline
                using (ITimeline timeline = new DefaultTimeline())
                {
                    IGroup audioGroup = timeline.AddAudioGroup();
                    ITrack rootTrack = audioGroup.AddTrack();
                    rootTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                    // render the timeline
                    using (
                        WavFileRenderer renderer =
                            new WavFileRenderer(timeline, outputFile, compressor.Filter, compressor.MediaType, null))
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

                    AssertLengths(timeline.FPS, 2, outputFile);
                }
            }
            finally
            {
                if (compressor != null)
                {
                    compressor.Release();
                }
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "No audio stream to render")]
        public void RenderWithoutAudioGroup()
        {
            string outputFile = "RenderWithoutAudioGroup.wav";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(24, 200, 200);

                // render the timeline
                using (WavFileRenderer renderer = new WavFileRenderer(timeline, outputFile))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        public void ConvertWavToWav()
        {
            string outputFile = "ConvertWavToWav.wav";

            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 2);

                // render the timeline                
                using (WavFileRenderer renderer = new WavFileRenderer(timeline, outputFile))
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

                AssertLengths(timeline, 2, outputFile);
            }
        }

        [Test]
        public void ConvertMp3ToWavWithCompressor()
        {
            // TODO: the compressor is being added, but the selected media type seems (encoding etc.)
            // seems to be ignored.

            string outputFile = "ConvertMp3ToWavWithCompressor.wav";

            // create the timeline
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack rootTrack = audioGroup.AddTrack();
                rootTrack.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 2);

                // render the timeline
                using (
                    WavFileRenderer renderer =
                        new WavFileRenderer(timeline, outputFile, CommonAudioFormats.LowQualityMonoPcm,
                                            new ConsoleProgressCallback()))
                {
                    ExecuteRenderer(renderer,
                                    @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""2"" src=""testinput.mp3"" mstart=""0""/>
        </track>
    </group>
</timeline>");
                }

                AssertLengths(timeline, 2, outputFile);
            }
        }
    }
}