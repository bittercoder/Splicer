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

using System.Drawing;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splicer.Renderer;
using Splicer.Timeline;
using Splicer.WindowsMedia;

namespace Splicer.Tests.Renderer
{
    [TestClass]
    public class ImageWatermarkParticipantFixture
    {
        [TestMethod]
        public void RenderWmvWithImageWatermark()
        {
            string outputFile = "RenderWmvWithImageWatermark.wmv";

            using (Image waterMarkImage = Image.FromFile("..\\..\\corner_watermark.png"))
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup videoGroup = timeline.AddVideoGroup(32, 320, 240);
                ITrack videoTrack = videoGroup.AddTrack();

                IClip videoClip =
                    videoTrack.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, 2);

                IGroup audioGroup = timeline.AddAudioGroup();
                ITrack audioTrack = audioGroup.AddTrack();
                audioTrack.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 2);

                ICallbackParticipant[] videoParticipants =
                    new ICallbackParticipant[]
                        {new ImageWatermarkParticipant(32, 320, 240, true, waterMarkImage, new Point(200, 0))};

                using (
                    WindowsMediaRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo,
                                                 videoParticipants, null))
                {
                    renderer.Render();
                }
            }
        }
    }
}