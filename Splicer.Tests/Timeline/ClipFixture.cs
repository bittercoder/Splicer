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
using DirectShowLib.DES;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Splicer.Timeline.Tests
{
    [TestClass]
    public class ClipFixture : AbstractFixture
    {
        [TestMethod]
        [
            ExpectedException(typeof (SplicerException),
                "Missing Exception for: You can not add audio clips to a track which exists within a non-audio group")]
        public void AddAudioClipToVideoGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddVideoGroup(24, 64, 64).AddTrack();
                track.AddClip("wav file", "..\\..\\1sec.wav", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, -1);
            }
        }

        [TestMethod]
        public void AddClip()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, -1);
                Assert.AreSame(track, clip.Container);
                Assert.AreSame(group, clip.Group);
                Assert.AreEqual(1, track.Clips.Count);
                Assert.AreSame(track.Clips[0], clip);
                Assert.IsNull(clip.Name);
            }
        }

        [TestMethod]
        public void AddClipIgnoresUnrequiredAssistant()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                var assistant = new MockMediaFileAssistant(false);
                timeline.InstallAssistant(assistant);

                Assert.AreEqual(0, assistant.ExecutionCount);

                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddAudio("..\\..\\1sec.wav");

                Assert.AreEqual(0, assistant.ExecutionCount);
            }
        }

        [TestMethod]
        public void AddClipResolvesDuration()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, -1);
                Assert.AreEqual(7.999, clip.Duration);
            }
        }

        [TestMethod]
        public void AddClipUsesAssistant()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                var assistant = new MockMediaFileAssistant(true);
                timeline.InstallAssistant(assistant);

                Assert.AreEqual(0, assistant.ExecutionCount);

                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddAudio("..\\..\\1sec.wav");

                Assert.AreEqual(1, assistant.ExecutionCount);
            }
        }

        [TestMethod]
        public void AddClipWithName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip =
                    track.AddClip("clock animation", "..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute,
                                  0, 0,
                                  -1);
                Assert.AreEqual(1, track.Clips.Count);
                Assert.AreSame(track.Clips[0], clip);
                Assert.AreEqual("clock animation", clip.Name);
                Assert.AreEqual(7.999, clip.Duration);
                Assert.AreEqual(0, clip.Offset);
                Assert.AreEqual("..\\..\\transitions.wmv", clip.File.FileName);
                Assert.AreEqual(0, clip.Effects.Count);
            }
        }

        [TestMethod]
        public void AddEffectToClip()
        {
            bool beforeFired = false;
            bool afterFired = false;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack();
                IClip clip = track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, -1);

                clip.AddingEffect += delegate { beforeFired = true; };

                clip.AddedEffect += delegate { afterFired = true; };

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
			<clip start=""0"" stop=""7.9990000"" src=""..\..\transitions.wmv"" mstart=""0"">
				<effect start=""0"" stop=""7.9990000"" clsid=""{7312498D-E87A-11D1-81E0-0000F87557DB}"" username=""blur"">
					<param name=""PixelRadius"" value=""2"">
						<linear time=""7.9990000"" value=""20"" />
					</param>
				</effect>
			</clip>
		</track>
	</group>
</timeline>");
            }
        }

        [TestMethod, ExpectedException(typeof (SplicerException),
            "Missing exception for: You can not add video or image clips to a track which exists within a non-video group")]
        public void AddImageClipToAudioGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddClip("image file", "..\\..\\image1.jpg", GroupMediaType.Image, InsertPosition.Absolute, 0, 0, -1);
            }
        }

        [TestMethod, ExpectedException(typeof (SplicerException),
            "Missing exception for: You can not add video or image clips to a track which exists within a non-video group")]
        public void AddVideoClipToAudioGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                ITrack track = timeline.AddAudioGroup().AddTrack();
                track.AddClip("wav file", "..\\..\\1sec.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, -1);
            }
        }

        [TestMethod]
        public void AlterStretchMode()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 64, 64);
                ITrack track = group.AddTrack("root", -1);
                IClip clip = track.AddClip("..\\..\\transitions.wmv", GroupMediaType.Video, InsertPosition.Absolute, 0, 0, -1);

                Assert.AreEqual(ResizeFlags.Stretch, clip.StretchMode);
                clip.StretchMode = ResizeFlags.PreserveAspectRatio;
                Assert.AreEqual(ResizeFlags.PreserveAspectRatio, clip.StretchMode);
            }
        }

        [TestMethod]
        public void RemoveClipEventHandlers()
        {
            int count = 0;

            EventHandler incrementBefore = delegate { count++; };

            EventHandler<AddedEffectEventArgs> incrementAfter =
                delegate { count++; };

            using (ITimeline timeline = new DefaultTimeline())
            {
                IClip clip =
                    timeline.AddAudioGroup().AddTrack().AddClip("..\\..\\testinput.wav", GroupMediaType.Audio,
                                                                InsertPosition.Absolute, 0, 0, -1);
                clip.AddedEffect += incrementAfter;
                clip.AddingEffect += incrementBefore;

                clip.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                Assert.AreEqual(2, count);

                count = 0;
                clip.AddedEffect -= incrementAfter;
                clip.AddingEffect -= incrementBefore;

                clip.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());

                Assert.AreEqual(0, count);
            }
        }
    }
}