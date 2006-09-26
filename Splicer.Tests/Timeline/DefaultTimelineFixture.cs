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

using System;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class DefaultTimelineFixture : AbstractFixture
    {
        [Test]
        public void Constructor()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                Assert.AreEqual(DefaultTimeline.DefaultFPS, timeline.FPS);
                Assert.AreEqual(0, timeline.Groups.Count);
                Assert.AreEqual(0, timeline.Duration);
                Assert.AreEqual(0, timeline.LengthInUnits);
            }
        }

        [Test]
        public void Constructor2()
        {
            using (ITimeline timeline = new DefaultTimeline(15))
            {
                Assert.AreEqual(15, timeline.FPS);
            }
        }

        [Test]
        public void RemoveGroupAddedHandlers()
        {
            int count = 0;

            EventHandler incrementBefore = new EventHandler(delegate
                {
                    count++;
                });
            EventHandler<AfterGroupAddedEventArgs> incrementAfter = new EventHandler<AfterGroupAddedEventArgs>(delegate
                {
                    count++;
                });

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.BeforeGroupAdded += incrementBefore;
                timeline.AfterGroupAdded += incrementAfter;

                timeline.AddAudioGroup();

                Assert.AreEqual(2, count);

                count = 0;
                timeline.BeforeGroupAdded -= incrementBefore;
                timeline.AfterGroupAdded -= incrementAfter;

                timeline.AddAudioGroup();

                Assert.AreEqual(0, count);
            }
        }


        [Test]
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

        [Test]
        public void AddVideoGroupWithName()
        {
            using (ITimeline timeline = new DefaultTimeline(12))
            {
                IGroup group = timeline.AddVideoGroup("some video", 32, 100, 100);
                Assert.AreEqual(group.Name, "some video");
                Assert.AreSame(timeline, group.Timeline);
            }
        }

        [Test]
        public void AddVideoGroupWithDefaultFPS()
        {
            bool beforeFired = false;
            bool afterFried = false;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeGroupAdded += new EventHandler(delegate
                    {
                        beforeFired = true;
                    });

                timeline.AfterGroupAdded += new EventHandler<AfterGroupAddedEventArgs>(delegate
                    {
                        afterFried = true;
                    });

                IGroup group = timeline.AddVideoGroup(32, 100, 100);
                Assert.AreSame(timeline, group.Timeline);
                Assert.AreEqual(timeline.FPS, group.FPS);
                Assert.IsTrue(beforeFired);
                Assert.IsTrue(afterFried);
            }
        }

        [Test]
        public void AddAudioGroupWithDefaultFPS()
        {
            bool beforeFired = false;
            bool afterFried = false;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeGroupAdded += new EventHandler(delegate
                    {
                        beforeFired = true;
                    });

                timeline.AfterGroupAdded += new EventHandler<AfterGroupAddedEventArgs>(delegate
                    {
                        afterFried = true;
                    });

                IGroup group = timeline.AddAudioGroup();
                Assert.AreSame(timeline, group.Timeline);
                Assert.AreEqual(timeline.FPS, group.FPS);
                Assert.IsTrue(beforeFired);
                Assert.IsTrue(afterFried);
            }
        }

        [Test]
        public void GroupAddEffectBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeEffectAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                timeline.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IGroup group = timeline.AddAudioGroup();
                group.AddEffect(0, 1, StandardEffects.CreateDefaultBlur());
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [Test]
        public void GroupAddTransitionBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeTransitionAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                timeline.AfterTransitionAdded += new EventHandler<AfterTransitionAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IGroup group = timeline.AddAudioGroup();
                group.AddTransition(0, 0, StandardTransitions.CreateFade());
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [Test]
        public void GroupAddTrackBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeTrackAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                timeline.AfterTrackAdded += new EventHandler<AfterTrackAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IGroup group = timeline.AddAudioGroup();
                group.AddTrack();
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [Test]
        public void GroupAddCompositionBubblesToTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeCompositionAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                timeline.AfterCompositionAdded += new EventHandler<AfterCompositionAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IGroup group = timeline.AddAudioGroup();
                group.AddComposition();
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [Test]
        public void AddClipInChildBubblestoTimeline()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline(12))
            {
                timeline.BeforeClipAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                timeline.AfterClipAdded += new EventHandler<AfterClipAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                IGroup group = timeline.AddAudioGroup();
                group.AddTrack().AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 5);
            }

            Assert.AreEqual(1, beforeCount);
            Assert.AreEqual(1, afterCount);
        }

        [Test]
        public void AddAudio()
        {
            // test all the overloads for AddAudio

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup().AddTrack();

                IClip clip1 = timeline.AddAudio("1sec.wav");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddAudio("1sec.wav", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddAudio("1sec.wav", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddAudio("1sec.wav", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddAudio("1sec.wav", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddAudio("myclip", "1sec.wav", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [Test]
        public void AddVideo()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = timeline.AddVideo("1sec.wmv");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddVideo("1sec.wmv", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddVideo("1sec.wmv", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddVideo("1sec.wmv", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddVideo("1sec.wmv", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddVideo("myclip", "1sec.wmv", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [Test]
        public void AddImage()
        {
            // test all the overloads for AddVideo

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();

                IClip clip1 = timeline.AddImage("image1.jpg");
                Assert.AreEqual(0, clip1.Offset);
                Assert.AreEqual(1, clip1.Duration);

                IClip clip2 = timeline.AddImage("image1.jpg", 1);
                Assert.AreEqual(2, clip2.Offset);
                Assert.AreEqual(1, clip2.Duration);

                IClip clip3 = timeline.AddImage("image1.jpg", 0, 0.5);
                Assert.AreEqual(3, clip3.Offset);
                Assert.AreEqual(0.5, clip3.Duration);

                IClip clip4 = timeline.AddImage("image1.jpg", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.Offset);
                Assert.AreEqual(0.5, clip4.Duration);
                Assert.AreEqual(0.5, clip4.MediaStart);

                IClip clip5 = timeline.AddImage("image1.jpg", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.Offset);
                Assert.AreEqual(1, clip5.Duration);

                IClip clip6 = timeline.AddImage("myclip", "image1.jpg", InsertPosition.Absoloute, 8, 0, 0.5);
                Assert.AreEqual(8, clip6.Offset);
                Assert.AreEqual(0, clip6.MediaStart);
                Assert.AreEqual(0.5, clip6.Duration);
                Assert.AreEqual("myclip", clip6.Name);
            }
        }

        [Test]
        public void AddAudioAndVideo()
        {
            // test all the overloads for AddVideoWithAudio

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack();
                timeline.AddAudioGroup().AddTrack();

                IAudioVideoClipPair clip1 = timeline.AddVideoWithAudio("1sec.wmv");
                Assert.AreEqual(0, clip1.AudioClip.Offset);
                Assert.AreEqual(1, clip1.AudioClip.Duration);
                Assert.AreEqual(0, clip1.VideoClip.Offset);
                Assert.AreEqual(1, clip1.VideoClip.Duration);

                IAudioVideoClipPair clip2 = timeline.AddVideoWithAudio("1sec.wmv", 1);
                Assert.AreEqual(2, clip2.AudioClip.Offset);
                Assert.AreEqual(1, clip2.AudioClip.Duration);
                Assert.AreEqual(2, clip2.VideoClip.Offset);
                Assert.AreEqual(1, clip2.VideoClip.Duration);

                IAudioVideoClipPair clip3 = timeline.AddVideoWithAudio("1sec.wmv", 0, 0.5);
                Assert.AreEqual(3, clip3.AudioClip.Offset);
                Assert.AreEqual(0.5, clip3.AudioClip.Duration);
                Assert.AreEqual(3, clip3.VideoClip.Offset);
                Assert.AreEqual(0.5, clip3.VideoClip.Duration);

                IAudioVideoClipPair clip4 = timeline.AddVideoWithAudio("1sec.wmv", 0, 0.5, 1.0);
                Assert.AreEqual(3.5, clip4.AudioClip.Offset);
                Assert.AreEqual(0.5, clip4.AudioClip.Duration);
                Assert.AreEqual(0.5, clip4.AudioClip.MediaStart);
                Assert.AreEqual(3.5, clip4.VideoClip.Offset);
                Assert.AreEqual(0.5, clip4.VideoClip.Duration);
                Assert.AreEqual(0.5, clip4.VideoClip.MediaStart);

                IAudioVideoClipPair clip5 = timeline.AddVideoWithAudio("1sec.wmv", InsertPosition.Absoloute, 6, 0, -1);
                Assert.AreEqual(6, clip5.AudioClip.Offset);
                Assert.AreEqual(1, clip5.AudioClip.Duration);
                Assert.AreEqual(6, clip5.VideoClip.Offset);
                Assert.AreEqual(1, clip5.VideoClip.Duration);

                IAudioVideoClipPair clip6 =
                    timeline.AddVideoWithAudio("myclip", "1sec.wmv", InsertPosition.Absoloute, 8, 0, 0.5);
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

        [Test]
        public void RemoveEvents()
        {
            int count = 0;

            EventHandler increment = new EventHandler(delegate
                {
                    count++;
                });

            EventHandler<AfterCompositionAddedEventArgs> incrementForAfterCompositionAdded =
                new EventHandler<AfterCompositionAddedEventArgs>(delegate
                    {
                        count++;
                    });

            EventHandler<AfterEffectAddedEventArgs> incrementForAfterEffectAdded =
                new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        count++;
                    });

            EventHandler<AfterTrackAddedEventArgs> incrementForAfterTrackAdded =
                new EventHandler<AfterTrackAddedEventArgs>(delegate
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

            EventHandler<AfterGroupAddedEventArgs> incrementForAfterGroupAdded =
                new EventHandler<AfterGroupAddedEventArgs>(delegate
                    {
                        count++;
                    });

            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AfterCompositionAdded += incrementForAfterCompositionAdded;
                timeline.AfterEffectAdded += incrementForAfterEffectAdded;
                timeline.AfterTrackAdded += incrementForAfterTrackAdded;
                timeline.AfterTransitionAdded += incrementForAfterTransitionAdded;
                timeline.AfterClipAdded += incrementForAfterClipAdded;
                timeline.AfterGroupAdded += incrementForAfterGroupAdded;

                timeline.BeforeCompositionAdded += increment;
                timeline.BeforeEffectAdded += increment;
                timeline.BeforeTrackAdded += increment;
                timeline.BeforeTransitionAdded += increment;
                timeline.BeforeClipAdded += increment;
                timeline.BeforeGroupAdded += increment;

                IGroup group = timeline.AddAudioGroup();
                group.AddComposition();
                group.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                group.AddTrack().AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 1);
                group.AddTransition(0, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(12, count);
                count = 0;

                timeline.AfterCompositionAdded -= incrementForAfterCompositionAdded;
                timeline.AfterEffectAdded -= incrementForAfterEffectAdded;
                timeline.AfterTrackAdded -= incrementForAfterTrackAdded;
                timeline.AfterTransitionAdded -= incrementForAfterTransitionAdded;
                timeline.AfterClipAdded -= incrementForAfterClipAdded;
                timeline.AfterGroupAdded -= incrementForAfterGroupAdded;

                timeline.BeforeCompositionAdded -= increment;
                timeline.BeforeEffectAdded -= increment;
                timeline.BeforeTrackAdded -= increment;
                timeline.BeforeTransitionAdded -= increment;
                timeline.BeforeClipAdded -= increment;
                timeline.BeforeGroupAdded -= increment;

                IGroup group2 = timeline.AddAudioGroup();
                group2.AddComposition();
                group2.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                group2.AddTrack().AddClip("testinput.wav", GroupMediaType.Audio, InsertPosition.Relative, 0, 0, 1);
                group2.AddTransition(2, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(0, count);
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "No group found supporting a clip of type \"Audio\"")]
        public void AddAudioClipWhenNoSupportingGroupExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudio("1sec.wav");
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "No group found supporting a clip of type \"Video\"")]
        public void AddVideoClipWhenNoSupportingGroupExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideo("1sec.wmv");
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "No tracks found in the first group of type \"Audio\"")]
        public void AddAudioClipWhenNoSupportingTrackExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddAudioGroup();
                timeline.AddAudio("1sec.wav");
            }
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "No tracks found in the first group of type \"Video\"")]
        public void AddVideoClipWhenNoSupportingTrackExists()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 100, 100);
                timeline.AddVideo("1sec.wmv");
            }
        }
    }
}