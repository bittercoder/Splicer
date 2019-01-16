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

namespace Splicer.Timeline.Tests
{
    [TestClass]
    public class DefaultTimelineFixture : AbstractFixture
    {
        [TestMethod]
        public void AddAudio()
        {
            // test all the overloads for AddAudio

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack();

                IClip clip1 = timeline.AddAudio("..\\..\\1sec.wav");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddAudio("..\\..\\1sec.wav", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddAudio("..\\..\\1sec.wav", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddAudio("..\\..\\1sec.wav", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddAudio("..\\..\\1sec.wav", InsertPosition.Absolute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddAudio("myclip", "..\\..\\1sec.wav", InsertPosition.Absolute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [TestMethod]
        public void AddAudioAndVideo()
        {
            // test all the overloads for AddVideoWithAudio

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();
                timeline.AddAudioGroup().AddTrack();

                IAudioVideoClipPair clip1 = timeline.AddVideoWithAudio("..\\..\\1sec.wmv");
                Assert.AreEqual(0, clip1.AudioClip.Offset);
                Assert.AreEqual(1, clip1.AudioClip.Duration);
                Assert.AreEqual(0, clip1.VideoClip.Offset);
                Assert.AreEqual(1, clip1.VideoClip.Duration);

                IAudioVideoClipPair clip2 = timeline.AddVideoWithAudio("..\\..\\1sec.wmv", 1);
                Assert.AreEqual(2, clip2.AudioClip.Offset);
                Assert.AreEqual(1, clip2.AudioClip.Duration);
                Assert.AreEqual(2, clip2.VideoClip.Offset);
                Assert.AreEqual(1, clip2.VideoClip.Duration);

                IAudioVideoClipPair clip3 = timeline.AddVideoWithAudio("..\\..\\1sec.wmv", 0, 0.5);
                Assert.AreEqual(3, clip3.AudioClip.Offset);
                Assert.AreEqual(0.5, clip3.AudioClip.Duration);
                Assert.AreEqual(3, clip3.VideoClip.Offset);
                Assert.AreEqual(0.5, clip3.VideoClip.Duration);

                IAudioVideoClipPair clip4 = timeline.AddVideoWithAudio("..\\..\\1sec.wmv", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.AudioClip.Offset);
                Assert.AreEqual(0.5, clip4.AudioClip.Duration);
                Assert.AreEqual(0.5, clip4.AudioClip.MediaStart);
                Assert.AreEqual(3.5, clip4.VideoClip.Offset);
                Assert.AreEqual(0.5, clip4.VideoClip.Duration);
                Assert.AreEqual(0.5, clip4.VideoClip.MediaStart);

                IAudioVideoClipPair clip5 = timeline.AddVideoWithAudio("..\\..\\1sec.wmv", InsertPosition.Absolute, 6, 0, -1);
                Assert.AreEqual(6, clip5.AudioClip.Offset);
                Assert.AreEqual(1, clip5.AudioClip.Duration);
                Assert.AreEqual(6, clip5.VideoClip.Offset);
                Assert.AreEqual(1, clip5.VideoClip.Duration);

                IAudioVideoClipPair clip6 =
                    timeline.AddVideoWithAudio("myclip", "..\\..\\1sec.wmv", InsertPosition.Absolute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.AudioClip.Offset);
                Assert.AreEqual(0, clip6.AudioClip.MediaStart);
                Assert.AreEqual(0.5, clip6.AudioClip.Duration);
                Assert.AreEqual("myclip", clip6.AudioClip.Name);
                Assert.AreEqual(8, clip6.VideoClip.Offset);
                Assert.AreEqual(0, clip6.VideoClip.MediaStart);
                Assert.AreEqual(0.5, clip6.VideoClip.Duration);
                Assert.AreEqual("myclip", clip6.VideoClip.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "No group found supporting a clip of type \"Audio\"")]
        public void AddAudioClipWhenNoSupportingGroupExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudio("..\\..\\1sec.wav");
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "No tracks found in the first group of type \"Audio\"")]
        public void AddAudioClipWhenNoSupportingTrackExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();
                timeline.AddAudio("..\\..\\1sec.wav");
            }
        }

        [TestMethod]
        public void AddAudioGroupWithDefaultFPS()
        {
            bool beforeFired = false;
            bool afterFried = false;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingGroup += delegate { beforeFired = true; };

                timeline.AddedGroup += delegate { afterFried = true; };

                IGroup group = timeline.AddAudioGroup();
                Assert.AreSame(timeline, group.Timeline);
                Assert.AreEqual(timeline.Fps, group.Fps);
                Assert.IsTrue(beforeFired);
                Assert.IsTrue(afterFried);
            }
        }

        [TestMethod]
        public void AddAudioGroupWithName()
        {
            using (ITimeline timeline = new DefaultTimeline(12))
            {
                IGroup group = timeline.AddAudioGroup("my audio");
                Assert.AreEqual(group.Name, "my audio");
                Assert.AreSame(timeline, group.Timeline);

                PrepareToExecute(timeline,
                                 @"<timeline framerate=""12.0000000""><group username=""my audio"" type=""audio"" framerate=""12.0000000"" previewmode=""0"" /></timeline>");
            }
        }

        [TestMethod]
        public void AddClipInChildBubblestoTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingClip += delegate { beforeCount++; };

                timeline.AddedClip += delegate { afterCount++; };

                IGroup group = timeline.AddAudioGroup();
                group.AddTrack().AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 5);
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [TestMethod]
        public void AddImage()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = timeline.AddImage("..\\..\\image1.jpg");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddImage("..\\..\\image1.jpg", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddImage("..\\..\\image1.jpg", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddImage("..\\..\\image1.jpg", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddImage("..\\..\\image1.jpg", InsertPosition.Absolute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddImage("myclip", "..\\..\\image1.jpg", InsertPosition.Absolute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [TestMethod]
        public void AddVideo()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = timeline.AddVideo("..\\..\\1sec.wmv");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddVideo("..\\..\\1sec.wmv", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddVideo("..\\..\\1sec.wmv", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddVideo("..\\..\\1sec.wmv", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddVideo("..\\..\\1sec.wmv", InsertPosition.Absolute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddVideo("myclip", "..\\..\\1sec.wmv", InsertPosition.Absolute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "No group found supporting a clip of type \"Video\"")]
        public void AddVideoClipWhenNoSupportingGroupExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideo("1sec.wmv");
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException),
            "Missing Exception for: No tracks found in the first group of type \"Video\"")]
        public void AddVideoClipWhenNoSupportingTrackExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 100, 100);
                timeline.AddVideo("..\\..\\1sec.wmv");
            }
        }

        [TestMethod]
        public void AddVideoGroupWithDefaultFPS()
        {
            bool beforeFired = false;
            bool afterFried = false;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingGroup += delegate { beforeFired = true; };

                timeline.AddedGroup += delegate { afterFried = true; };

                IGroup group = timeline.AddVideoGroup(32, 100, 100);
                Assert.AreSame(timeline, group.Timeline);
                Assert.AreEqual(timeline.Fps, group.Fps);
                Assert.IsTrue(beforeFired);
                Assert.IsTrue(afterFried);
            }
        }

        [TestMethod]
        public void AddVideoGroupWithName()
        {
            using (ITimeline timeline = new DefaultTimeline(12))
            {
                IGroup group = timeline.AddVideoGroup("some video", 32, 100, 100);
                Assert.AreEqual(group.Name, "some video");
                Assert.AreSame(timeline, group.Timeline);
            }
        }

        [TestMethod]
        public void Constructor()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                Assert.AreEqual(DefaultTimeline.DefaultFps, timeline.Fps);
                Assert.AreEqual(0, timeline.Groups.Count);
                Assert.AreEqual(0, timeline.Duration);
                Assert.AreEqual(0, timeline.LengthInUnits);
            }
        }

        [TestMethod]
        public void Constructor2()
        {
            using (ITimeline timeline = new DefaultTimeline(15))
            {
                Assert.AreEqual(15, timeline.Fps);
            }
        }

        [TestMethod]
        public void GroupAddCompositionBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingComposition += delegate { beforeCount++; };

                timeline.AddedComposition += delegate { afterCount++; };

                IGroup group = timeline.AddAudioGroup();
                group.AddComposition();
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [TestMethod]
        public void GroupAddEffectBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingEffect += delegate { beforeCount++; };

                timeline.AddedEffect += delegate { afterCount++; };

                IGroup group = timeline.AddAudioGroup();
                group.AddEffect(0, 1, StandardEffects.CreateDefaultBlur());
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [TestMethod]
        public void GroupAddTrackBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingTrack += delegate { beforeCount++; };

                timeline.AddedTrack += delegate { afterCount++; };

                IGroup group = timeline.AddAudioGroup();
                group.AddTrack();
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [TestMethod]
        public void GroupAddTransitionBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.AddingTransition += delegate { beforeCount++; };

                timeline.AddedTransition += delegate { afterCount++; };

                IGroup group = timeline.AddAudioGroup();
                group.AddTransition(0, 0, StandardTransitions.CreateFade());
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [TestMethod]
        public void RemoveEvents()
        {
            int count = 0;

            EventHandler increment = delegate { count++; };

            EventHandler<AddedCompositionEventArgs> incrementForAfterCompositionAdded =
                delegate { count++; };

            EventHandler<AddedEffectEventArgs> incrementForAfterEffectAdded =
                delegate { count++; };

            EventHandler<AddedTrackEventArgs> incrementForAfterTrackAdded =
                delegate { count++; };

            EventHandler<AddedTransitionEventArgs> incrementForAfterTransitionAdded =
                delegate { count++; };

            EventHandler<AddedClipEventArgs> incrementForAfterClipAdded =
                delegate { count++; };

            EventHandler<AddedGroupEventArgs> incrementForAfterGroupAdded =
                delegate { count++; };

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddedComposition += incrementForAfterCompositionAdded;
                timeline.AddedEffect += incrementForAfterEffectAdded;
                timeline.AddedTrack += incrementForAfterTrackAdded;
                timeline.AddedTransition += incrementForAfterTransitionAdded;
                timeline.AddedClip += incrementForAfterClipAdded;
                timeline.AddedGroup += incrementForAfterGroupAdded;

                timeline.AddingComposition += increment;
                timeline.AddingEffect += increment;
                timeline.AddingTrack += increment;
                timeline.AddingTransition += increment;
                timeline.AddingClip += increment;
                timeline.AddingGroup += increment;

                IGroup group = timeline.AddAudioGroup();
                group.AddComposition();
                group.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                group.AddTrack().AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 1);
                group.AddTransition(0, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(12, count);
                count = 0;

                timeline.AddedComposition -= incrementForAfterCompositionAdded;
                timeline.AddedEffect -= incrementForAfterEffectAdded;
                timeline.AddedTrack -= incrementForAfterTrackAdded;
                timeline.AddedTransition -= incrementForAfterTransitionAdded;
                timeline.AddedClip -= incrementForAfterClipAdded;
                timeline.AddedGroup -= incrementForAfterGroupAdded;

                timeline.AddingComposition -= increment;
                timeline.AddingEffect -= increment;
                timeline.AddingTrack -= increment;
                timeline.AddingTransition -= increment;
                timeline.AddingClip -= increment;
                timeline.AddingGroup -= increment;

                IGroup group2 = timeline.AddAudioGroup();
                group2.AddComposition();
                group2.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                group2.AddTrack().AddClip("..\\..\\testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 1);
                group2.AddTransition(2, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(0, count);
            }
        }

        [TestMethod]
        public void RemoveGroupAddedHandlers()
        {
            int count = 0;

            EventHandler incrementBefore = delegate { count++; };
            EventHandler<AddedGroupEventArgs> incrementAfter = delegate { count++; };

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddingGroup += incrementBefore;
                timeline.AddedGroup += incrementAfter;

                timeline.AddAudioGroup();

                Assert.AreEqual(2, count);

                count = 0;
                timeline.AddingGroup -= incrementBefore;
                timeline.AddedGroup -= incrementAfter;

                timeline.AddAudioGroup();

                Assert.AreEqual(0, count);
            }
        }

        [TestMethod]
        public void ShadowCopyAudio()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack();
                timeline.AddVideoGroup(24, 320, 200).AddTrack();
                IAudioVideoClipPair pair = timeline.AddVideoWithAudio("..\\..\\1sec.wmv", true);
                Assert.AreEqual("..\\..\\1sec.wmv", pair.VideoClip.File.FileName);
                Assert.AreNotEqual("..\\..\\1sec.wmv", pair.AudioClip.File.FileName);
                Assert.IsTrue(pair.AudioClip.File.FileName.EndsWith(".wmv"));
            }
        }
    }
}