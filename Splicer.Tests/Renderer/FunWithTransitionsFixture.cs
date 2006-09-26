using System;
using NUnit.Framework;
using Splicer.Renderer;
using Splicer.Timeline;
using Splicer.Utils;
using Splicer.WindowsMedia;

namespace Splicer.Tests.Renderer
{
    [TestFixture]
    public class FunWithTransitionsFixture : AbstractFixture
    {
        [Test]
        public void JumpVolume()
        {
            // and audible demonstration of the difference between interpolating
            // parameter values for an effect, and jumping directly to them.

            string outputFile = "JumpVolume.wma";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track = group.AddTrack();
                IClip clip = track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 10);

                EffectDefinition effectDefinition = new EffectDefinition(DxtSubObjects.AudioMixer);

                Parameter volumeParameter = new Parameter("Vol", 0.0, 2, 1.0);
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 2.5, "0.2"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 3.5, "0.8"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 4.5, "0.2"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Jump, 5, "1.0"));
                volumeParameter.Intervals.Add(new Interval(IntervalMode.Interpolate, clip.Duration, "0.0"));

                effectDefinition.Parameters.Add(volumeParameter);

                clip.AddEffect(0, clip.Duration, effectDefinition);

                using (
                    IRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.MediumQualityAudio))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        public void FadeBetweenImages()
        {
            // generates a little slide-show, with audio track and fades between images.

            string outputFile = "FadeBetweenImages.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(32, 160, 100);
                ITrack low = group.AddTrack();
                ITrack hi = group.AddTrack();
                hi.AddClip("image1.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 6);
                hi.AddClip("image2.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 6);
                hi.AddClip("image3.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 6);

                // fade out and back in
                group.AddTransition(5.5, 0.5, StandardTransitions.CreateFade(), true);
                group.AddTransition(6.0, 0.5, StandardTransitions.CreateFade(), false);

                // and again
                group.AddTransition(11.5, 0.5, StandardTransitions.CreateFade(), true);
                group.AddTransition(12.0, 0.5, StandardTransitions.CreateFade(), false);

                ITrack audioTrack = timeline.AddAudioGroup().AddTrack();
                IClip audio =
                    audioTrack.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, hi.Duration);
                audioTrack.AddEffect(0, audio.Duration,
                                     StandardEffects.CreateAudioEnvelope(1.0, 1.0, 1.0, audio.Duration));

                using (
                    IRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        public void PixelateAndIrisBetweenImages()
        {
            string outputFile = "PixelateAndIrisBetweenImages.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack().AddClip("testinput.wav", GroupMediaType.Audio,
                                                            InsertPosition.Relative, 0, 0, 17);

                IGroup group = timeline.AddVideoGroup(32, 160, 100);
                ITrack low = group.AddTrack();
                ITrack hi = group.AddTrack();
                hi.AddClip("image1.jpg", GroupMediaType.Image, InsertPosition.Absoloute, 0, 0, 6);
                low.AddClip("image2.jpg", GroupMediaType.Image, InsertPosition.Absoloute, 5, 0, 8);
                hi.AddClip("image3.jpg", GroupMediaType.Image, InsertPosition.Absoloute, 11, 0, 6);

                // notice that we must apply "in" and "out" of the pixelation effect, to get the
                // desired effect, like the fade
                hi.AddTransition(5.0, 1.0, StandardTransitions.CreatePixelate(), true);
                hi.AddTransition(6.0, 1.0, StandardTransitions.CreatePixelate(), false);

                // the iris transition is a one shot
                hi.AddTransition(11.0, 2.0, StandardTransitions.CreateIris(), false);

                using (
                    IRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    renderer.Render();
                }
            }
        }

        [Test]
        public void WatermarkVideoClip()
        {
            // this demonstrates one way of watermarking a video clip... 

            string outputFile = "Watermark.wmv";

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack().AddClip("testinput.mp3", GroupMediaType.Audio,
                                                            InsertPosition.Relative, 0, 0, 5);

                IGroup group = timeline.AddVideoGroup(32, 320, 240);
                ITrack content = group.AddTrack();
                ITrack watermark = group.AddTrack();
                content.AddClip("testpattern1.gif", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 5);

                IClip watermarkClip =
                    watermark.AddClip("testlogo.gif", GroupMediaType.Image, InsertPosition.Relative, 0, 0,
                                      content.Duration);
                watermarkClip.AddEffect(0, watermarkClip.Duration, StandardEffects.CreateAlphaSetterRamp(0.5));
                watermark.AddTransition(0, content.Duration,
                                        StandardTransitions.CreateDxtKey(DxtKeyTypes.Alpha, null, null, null, null, null),
                                        false);

                using (
                    IRenderer renderer =
                        new WindowsMediaRenderer(timeline, outputFile, WindowsMediaProfiles.HighQualityVideo))
                {
                    Console.WriteLine(renderer.ToXml());
                    renderer.Render();
                }
            }
        }
    }
}