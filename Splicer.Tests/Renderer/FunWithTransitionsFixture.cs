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

//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splicer.Renderer;
using Splicer.Timeline;
using Splicer.WindowsMedia;

namespace Splicer.Tests.Renderer
{
    [TestClass]
    public class FunWithTransitionsFixture : AbstractFixture
    {
        [TestMethod]
        public void FadeBetweenImages()
        {
            // generates a little slide-show, with audio track and fades between images.

            string outputFile = "FadeBetweenImages.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(32, 160, 100);

                ITrack videoTrack = group.AddTrack();
                IClip clip1 = videoTrack.AddImage("..\\..\\image1.jpg", 0, 2); // play first image for a little while
                IClip clip2 = videoTrack.AddImage("..\\..\\image2.jpg", 0, 2); // and the next
                IClip clip3 = videoTrack.AddImage("..\\..\\image3.jpg", 0, 2); // and finally the last
                IClip clip4 = videoTrack.AddImage("..\\..\\image4.jpg", 0, 2); // and finally the last

                double halfDuration = 0.5;

                // fade out and back in
                group.AddTransition(clip2.Offset - halfDuration, halfDuration, StandardTransitions.CreateFade(), true);
                group.AddTransition(clip2.Offset, halfDuration, StandardTransitions.CreateFade(), false);

                // again
                group.AddTransition(clip3.Offset - halfDuration, halfDuration, StandardTransitions.CreateFade(), true);
                group.AddTransition(clip3.Offset, halfDuration, StandardTransitions.CreateFade(), false);

                // and again
                group.AddTransition(clip4.Offset - halfDuration, halfDuration, StandardTransitions.CreateFade(), true);
                group.AddTransition(clip4.Offset, halfDuration, StandardTransitions.CreateFade(), false);

                // add some audio
                ITrack audioTrack = timeline.AddAudioGroup().AddTrack();

                IClip audio =
                    audioTrack.AddAudio("..\\..\\testinput.wav", 0, videoTrack.Duration);

                // create an audio envelope effect, this will:
                // fade the audio from 0% to 100% in 1 second.
                // play at full volume until 1 second before the end of the track
                // fade back out to 0% volume
                audioTrack.AddEffect(0, audio.Duration,
                                     StandardEffects.CreateAudioEnvelope(1.0, 1.0, 1.0, audio.Duration));

                // render our slideshow out to a windows media file
                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    renderer.Render();
                }
            }
        }

        [TestMethod]
        public void JumpVolume()
        {
            // and audible demonstration of the difference between interpolating
            // parameter values for an effect, and jumping directly to them.

            string outputFile = "JumpVolume.wma";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track = group.AddTrack();
                IClip clip = track.AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 10);

                var effectDefinition = new EffectDefinition(StandardEffects.AudioMixerEffect);

                var volumeParameter = new Parameter("Vol", 0.0, 2, 1.0);
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 2.5, "0.2"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 3.5, "0.8"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 4.5, "0.2"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 5, "1.0"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Interpolate, clip.Duration, "0.0"));

                effectDefinition.Parameters.Add(volumeParameter);

                clip.AddEffect(0, clip.Duration, effectDefinition);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.MediumQualityAudio))
                {
                    renderer.Render();
                }
            }
        }

        [TestMethod]
        public void PixelateAndIrisBetweenImages()
        {
            string outputFile = "PixelateAndIrisBetweenImages.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack().AddClip("..\\..\\testinput.wav", GroupMediaType.Audio,
                                                            InsertPosition.Relative, 0, 0, 17);

                IGroup group = timeline.AddVideoGroup(32, 160, 100);
                ITrack low = group.AddTrack();
                ITrack hi = group.AddTrack();
                hi.AddClip("..\\..\\image1.jpg", GroupMediaType.Image, InsertPosition.Absolute, 0, 0, 6);
                low.AddClip("..\\..\\image2.jpg", GroupMediaType.Image, InsertPosition.Absolute, 5, 0, 8);
                hi.AddClip("..\\..\\image3.jpg", GroupMediaType.Image, InsertPosition.Absolute, 11, 0, 6);

                // notice that we must apply "in" and "out" of the pixelation effect, to get the
                // desired effect, like the fade
                hi.AddTransition(5.0, 1.0, StandardTransitions.CreatePixelate(), true);
                hi.AddTransition(6.0, 1.0, StandardTransitions.CreatePixelate(), false);

                // the iris transition is a one shot
                hi.AddTransition(11.0, 2.0, StandardTransitions.CreateIris(), false);

                using (
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    renderer.Render();
                }
            }
        }

        [TestMethod]
        public void WatermarkVideoClip()
        {
            // this demonstrates one way of watermarking a video clip... 

            string outputFile = "WatermarkVideoClip.wmv";

            using (ITimeline timeline = new DefaultTimeline(15))
            {
                // greate our default audio track
                timeline.AddAudioGroup().AddTrack();

                // add a video group, 32bpp, 320x240 (32bpp required to allow for an alpha channel)
                IGroup videoGroup = timeline.AddVideoGroup(32, 320, 240);

                // add our default video track
                ITrack videoTrack = videoGroup.AddTrack();

                // add another video track, this will be used to contain our watermark image
                ITrack watermarkTrack = videoGroup.AddTrack();

                // add the video in "transitions.wmv" to the first video track, and the audio in "transitions.wmv"
                // to the first audio track.
                timeline.AddVideoWithAudio("..\\..\\transitions.wmv");

                // add the watermark image in, and apply it for the duration of the videoContent
                // this image will be stretched to fit the video clip, and in this case is a transparent gif.
                IClip watermarkClip = watermarkTrack.AddImage("..\\..\\testlogo.gif", 0, videoTrack.Duration);

                // add a alpha setter effect to the image, this will adjust the alpha of the image to be 0.5
                // of it's previous value - so the watermark is 50% transparent.
                watermarkClip.AddEffect(0, watermarkClip.Duration, StandardEffects.CreateAlphaSetterRamp(0.8));

                // add a transition to the watermark track, this allows the video clip to "shine through" the watermark,
                // base on the values present in the alpha channel of the watermark track.
                watermarkTrack.AddTransition(0, videoTrack.Duration,
                                             StandardTransitions.CreateKey(KeyTransitionType.Alpha, null, null, null,
                                                                           null,
                                                                           null),
                                             false);
                using (
                    // render it to windows media
                    var renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    renderer.Render();
                }
            }
        }
    }
}