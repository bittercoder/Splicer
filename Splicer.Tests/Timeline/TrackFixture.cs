using System;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class TrackFixture : AbstractFixture
    {
        [Test]
        public void AddTransitionsToTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 320, 200);
                ITrack track = group.AddTrack();

                TransitionDefinition definition = StandardTransitions.CreateFade();

                ITransition transition = track.AddTransition("test", 1, 3, definition, false);

                Assert.AreEqual(1, track.Transitions.Count);
                Assert.AreSame(transition, track.Transitions[0]);
                Assert.AreEqual("test", transition.Name);
                Assert.AreSame(definition, transition.TransitionDefinition);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""video"" bitdepth=""24"" height=""200"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<transition start=""1"" stop=""4"" clsid=""{16B280C5-EE70-11D1-9066-00C04FD9189D}"" username=""test"" />
		</track>
	</group>
</timeline>");
            }
        }

        [Test]
        public void AddEffectsToTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 320, 200);
                ITrack track = group.AddTrack();
                IEffect effect = track.AddEffect("test", -1, 1, 3, StandardEffects.CreateBlurEffect(2, 2, 15));
                Assert.AreEqual(1, track.Effects.Count);
                Assert.AreSame(effect, track.Effects[0]);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""video"" bitdepth=""24"" height=""200"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<effect clsid=""{7312498D-E87A-11D1-81E0-0000F87557DB}"" username=""test"">
				<param name=""PixelRadius"" value=""2"">
					<linear time=""2"" value=""15"" />
				</param>
			</effect>
		</track>
	</group>
</timeline>");
            }
        }

        [Test]
        public void ClipsAssignedContainer()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 320, 200);
                ITrack track = group.AddTrack();
                IClip clip = track.AddClip("image1.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 2);
                Assert.AreSame(track, clip.Container);
            }
        }

        [Test]
        public void AddClipsToTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 320, 200);
                ITrack track1 = group.AddTrack();
                ITrack track2 = group.AddTrack();

                track1.AddClip("image1.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 2);
                track2.AddClip("image2.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 2);
                track1.AddClip("image3.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 2);
                track2.AddClip("image4.jpg", GroupMediaType.Image, InsertPosition.Relative, 0, 0, 2);

                Assert.AreEqual(2, track1.Clips.Count);
                Assert.AreEqual(2, track2.Clips.Count);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""video"" bitdepth=""24"" height=""200"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""2"" src=""image1.jpg"" />
			<clip start=""2"" stop=""4"" src=""image3.jpg"" />
		</track>
		<track>
			<clip start=""0"" stop=""2"" src=""image2.jpg"" />
			<clip start=""2"" stop=""4"" src=""image4.jpg"" />
		</track>
	</group>
</timeline>");
            }
        }

        [Test]
        public void AddTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack rootTrack = group.AddTrack();
                Assert.AreSame(group, rootTrack.Group);
                Assert.AreSame(group, rootTrack.Container);
                Assert.AreEqual(1, group.Tracks.Count);
                Assert.AreSame(group.Tracks[0], rootTrack);
                ITrack track2 = group.AddTrack();
                Assert.AreEqual(2, group.Tracks.Count);
                Assert.AreEqual(group.Tracks[1], track2);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track />
        <track />
    </group>
</timeline>");
            }
        }

        [Test]
        public void AddTrackWithNames()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track1 = group.AddTrack("track1", -1);
                Assert.AreEqual("track1", track1.Name);
                Assert.AreEqual(1, group.Tracks.Count);
                track1.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, -1);

                Assert.AreSame(group.Tracks[0], track1);
                ITrack track2 = group.AddTrack("track2", -1);
                Assert.AreEqual("track2", track2.Name);
                track2.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, -1);

                Assert.AreEqual(2, group.Tracks.Count);
                Assert.AreEqual(group.Tracks[1], track2);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track username=""track1"">
			<clip start=""0"" stop=""55.1250000"" src=""testinput.wav"" mstart=""0"" />
		</track>
		<track username=""track2"">
			<clip start=""0"" stop=""55.1250000"" src=""testinput.wav"" mstart=""0"" />
		</track>
	</group>
</timeline>");
            }
        }

        [Test]
        public void AddOverlappingClips1()
        {
            // Though we've added 3 clips, the DES track only contains 2 tracks because the third has been occluded.
            // this behaviour is mimicked by the virtual clip collection, which demonstrates which clips are actually
            // visible at run time (only on clip on a track is being rendered at any one time)

            // clip 1 is added, 2 thru 10 secs (8 sec duration)
            // clip 2 is added 1 second before clip 1, and completely occluded it at 56 secs in length - clip 1 is gone
            // clip 3 is added 1 second before clip 1, it will play to completion, so the start position for clip 2 is placed
            // and the end of clip3, and it's media start value is incremented accordingly.

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track = group.AddTrack();
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 2, 0, -1);
                track.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Absoloute, 1, 0, -1);
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, -1);

                Assert.AreEqual(
                    @"<clip start=""0"" stop=""8.051875"" src=""testinput.mp3"" mstart=""0"" />
<clip start=""8.051875"" stop=""56.125"" src=""testinput.wav"" mstart=""7.051875"" />",
                    track.VirtualClips.ToString());

                Console.WriteLine(track.VirtualClips.ToString());

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
    <group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
        <track>
            <clip start=""0"" stop=""8.0518750"" src=""testinput.mp3"" mstart=""0"" />
            <clip start=""8.0518750"" stop=""56.1250000"" src=""testinput.wav"" mstart=""7.0518750"" />
        </track>
    </group>
</timeline>");
            }
        }

        [Test]
        public void AddOverlappingClips2()
        {
            // What's happening here is..

            // clip 1 is added, 2 thru 10 secs (8 sec duration)
            // clip 2 is added 1 second before clip 1, and completely occluded it at 56 secs in length - clip 1 is gone
            // clip 3 is added 1 second before clip 1, it will play to completion, so the start position for clip 2 is placed
            // and the end of clip3, and it's media start value is incremented accordingly.

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track = group.AddTrack();
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, -1);
                track.AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Absoloute, 1, 0, -1);
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 2, 0, -1);

                Assert.AreEqual(
                    @"<clip start=""0"" stop=""1"" src=""testinput.mp3"" mstart=""0"" />
<clip start=""1"" stop=""2"" src=""testinput.wav"" mstart=""0"" />
<clip start=""2"" stop=""10.051875"" src=""testinput.mp3"" mstart=""0"" />
<clip start=""10.051875"" stop=""56.125"" src=""testinput.wav"" mstart=""9.051875"" />",
                    track.VirtualClips.ToString());

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""30.0000000"">
	<group type=""audio"" framerate=""30.0000000"" previewmode=""0"">
		<track>
			<clip start=""0"" stop=""1"" src=""testinput.mp3"" mstart=""0"" />
			<clip start=""1"" stop=""2"" src=""testinput.wav"" mstart=""0"" />
			<clip start=""2"" stop=""10.0518750"" src=""testinput.mp3"" mstart=""0"" />
			<clip start=""10.0518750"" stop=""56.1250000"" src=""testinput.wav"" mstart=""9.0518750"" />
		</track>
	</group>
</timeline>");
            }
        }

        [Test]
        public void EnsureClipBubblesBeforeAndAfterEffectAddedUp()
        {
            int beforeCount = 0;
            int afterCount = 0;
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack track = group.AddTrack();
                track.BeforeEffectAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });
                track.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IClip clip = track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, -1);
                clip.AddEffect(0, 1, StandardEffects.CreateDefaultBlur());

                Assert.AreEqual(1, beforeCount);
                Assert.AreEqual(1, afterCount);
            }
        }

        [Test]
        public void RemoveEvents()
        {
            int count = 0;

            EventHandler increment = new EventHandler(delegate
                {
                    count++;
                });

            EventHandler<AfterEffectAddedEventArgs> incrementForAfterEffectAdded =
                new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        count++;
                    });

            EventHandler<AfterTransitionAddedEventArgs> incrementForAfterTransitionAdded =
                new EventHandler<AfterTransitionAddedEventArgs>(delegate
                    {
                        count++;
                    });

            EventHandler<AfterClipAddedEventArgs> incrementForAfterClipAdded =
                new EventHandler<AfterClipAddedEventArgs>(delegate
                    {
                        count++;
                    });

            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();

                track.AfterEffectAdded += incrementForAfterEffectAdded;
                track.AfterTransitionAdded += incrementForAfterTransitionAdded;
                track.AfterClipAdded += incrementForAfterClipAdded;

                track.BeforeEffectAdded += increment;
                track.BeforeTransitionAdded += increment;
                track.BeforeClipAdded += increment;

                track.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 1);
                track.AddTransition(0, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(6, count);
                count = 0;

                track.AfterEffectAdded -= incrementForAfterEffectAdded;
                track.AfterTransitionAdded -= incrementForAfterTransitionAdded;
                track.AfterClipAdded -= incrementForAfterClipAdded;
                track.BeforeEffectAdded -= increment;
                track.BeforeTransitionAdded -= increment;
                track.BeforeClipAdded -= increment;

                track.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                track.AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 1);
                track.AddTransition(2, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(0, count);
            }
        }

        [Test]
        public void AddAudioOverloads()
        {
            // test all the overloads for AddAudio

            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();

                IClip clip1 = track.AddAudio("1sec.wav");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = track.AddAudio("1sec.wav", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = track.AddAudio("1sec.wav", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = track.AddAudio("1sec.wav", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = track.AddAudio("1sec.wav", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = track.AddAudio("myclip", "1sec.wav", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [Test]
        public void AddVideoOverloads()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = track.AddVideo("1sec.wmv");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = track.AddVideo("1sec.wmv", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = track.AddVideo("1sec.wmv", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = track.AddVideo("1sec.wmv", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = track.AddVideo("1sec.wmv", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = track.AddVideo("myclip", "1sec.wmv", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [Test]
        public void AddImageOverloads()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = track.AddImage("image1.jpg");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = track.AddImage("image1.jpg", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = track.AddImage("image1.jpg", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = track.AddImage("image1.jpg", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = track.AddImage("image1.jpg", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = track.AddImage("myclip", "image1.jpg", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }
    }
}