using System;
using DirectShowLib.DES;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class ClipFixture : AbstractFixture
    {
        [Test]
        public void RemoveClipEventHandlers()
        {
            int count = 0;

            EventHandler incrementBefore = new EventHandler(
                delegate
                    {
                        count++;
                    }
                );

            EventHandler<AfterEffectAddedEventArgs> incrementAfter =
                new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        count++;
                    });

            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip clip =
                    timeline.AddAudioGroup().AddTrack().AddClip("testinput.wav", GroupMediaType.Audio,
                                                                InsertPosition.Absoloute, 0, 0, -1);
                clip.AfterEffectAdded += incrementAfter;
                clip.BeforeEffectAdded += incrementBefore;

                clip.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                Assert.AreEqual(2, count);

                count = 0;
                clip.AfterEffectAdded -= incrementAfter;
                clip.BeforeEffectAdded -= incrementBefore;

                clip.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());

                Assert.AreEqual(0, count);
            }
        }

        [Test]
        public void AddClipResolvesDuration()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, -1);
                Assert.AreEqual(12, clip.Duration);
            }
        }

        [Test]
        public void AddClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, -1);
                Assert.AreSame(track, clip.Container);
                Assert.AreSame(group, clip.Group);
                Assert.AreEqual(1, track.Clips.Count);
                Assert.AreSame(track.Clips[0], clip);
                Assert.IsNull(clip.Name);
            }
        }

        [Test]
        public void AlterStretchMode()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, -1);

                Assert.AreEqual(ResizeFlags.Stretch, clip.StretchMode);
                clip.StretchMode = ResizeFlags.PreserveAspectRatio;
                Assert.AreEqual(ResizeFlags.PreserveAspectRatio, clip.StretchMode);
            }
        }

        [Test]
        public void AddClipWithName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip =
                    track.AddClip("clock animation", "clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0,
                                  -1);
                Assert.AreEqual(1, track.Clips.Count);
                Assert.AreSame(track.Clips[0], clip);
                Assert.AreEqual("clock animation", clip.Name);
                Assert.AreEqual(12, clip.Duration);
                Assert.AreEqual(0, clip.Offset);
                Assert.AreEqual("clock.avi", clip.File.FileName);
                Assert.AreEqual(0, clip.Effects.Count);
            }
        }

        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "You can not add audio clips to a track which exists within a non-audio group")]
        public void AddAudioClipToVideoGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddVideoGroup(24, 64, 64).AddTrack();
                track.AddClip("wav file", "1sec.wav", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, -1);
            }
        }

        [Test, ExpectedException(typeof (SplicerException),
            "You can not add video or image clips to a track which exists within a non-video group")]
        public void AddVideoClipToAudioGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddClip("wav file", "1sec.wmv", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, -1);
            }
        }

        [Test, ExpectedException(typeof (SplicerException),
            "You can not add video or image clips to a track which exists within a non-video group")]
        public void AddImageClipToAudioGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddClip("image file", "image1.jpg", GroupMediaType.Image, InsertPosition.Absoloute, 0, 0, -1);
            }
        }

        [Test]
        public void AddClipUsesAssistant()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                MockMediaFileAssistant assistant = new MockMediaFileAssistant(true);
                timeline.InstallAssistant(assistant);

                Assert.AreEqual(0, assistant.ExecutionCount);

                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddAudio("1sec.wav");

                Assert.AreEqual(1, assistant.ExecutionCount);
            }
        }

        [Test]
        public void AddClipIgnoresUnrequiredAssistant()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                MockMediaFileAssistant assistant = new MockMediaFileAssistant(false);
                timeline.InstallAssistant(assistant);

                Assert.AreEqual(0, assistant.ExecutionCount);

                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddAudio("1sec.wav");

                Assert.AreEqual(0, assistant.ExecutionCount);
            }
        }

        [Test]
        public void AddEffectToClip()
        {
            bool beforeFired = false;
            bool afterFired = false;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack();
                IClip clip = track.AddClip("clock.avi", GroupMediaType.Video, InsertPosition.Absoloute, 0, 0, -1);

                clip.BeforeEffectAdded += new EventHandler(delegate
                    {
                        beforeFired = true;
                    });

                clip.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        afterFired = true;
                    });

                EffectDefinition defintion = StandardEffects.CreateBlurEffect(2, clip.Duration, 20);

                IEffect effect =
                    clip.AddEffect("blur", -1, 0, clip.Duration, defintion);

                Assert.IsTrue(beforeFired);
                Assert.IsTrue(afterFired);
                Assert.AreEqual("blur", effect.Name);
                Assert.AreEqual(0, effect.Priority);
                Assert.AreEqual(clip.Duration, effect.Duration);
                Assert.AreEqual(0, clip.Offset);
                Assert.AreSame(defintion, effect.EffectDefinition);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""video"" bitdepth=""24"" width=""64"" height=""64"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""12"" src=""clock.avi"" mstart=""0"">
				<effect start=""0"" stop=""12"" clsid=""{7312498D-E87A-11D1-81E0-0000F87557DB}"" username=""blur"">
					<param name=""PixelRadius"" value=""2"">
						<linear time=""12"" value=""20"" />
					</param>
				</effect>
			</clip>
		</track>
	</group>
</timeline>");
            }
        }
    }
}