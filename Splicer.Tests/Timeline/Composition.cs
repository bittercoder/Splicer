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
    public class CompositionFixture : AbstractFixture
    {
        [TestMethod]
        public void AddComposition()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition composition = group.AddComposition();
                Assert.AreSame(group, composition.Container);
                Assert.AreSame(group, composition.Group);

                bool firedBefore = false;
                bool firedAfter = false;

                composition.AddingComposition += delegate { firedBefore = true; };

                composition.AddedComposition += delegate { firedAfter = true; };

                IComposition childComposition = composition.AddComposition();
                Assert.AreSame(composition, childComposition.Container);
                Assert.AreSame(group, childComposition.Group);
                Assert.AreEqual(1, composition.Compositions.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [TestMethod]
        public void AddCompositionWithName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition composition = group.AddComposition("named", -1);
                Assert.AreEqual("named", composition.Name);
            }
        }

        [TestMethod]
        public void AddEffect()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.AddingEffect += delegate { firedBefore = true; };

                composition.AddedEffect += delegate { firedAfter = true; };

                IEffect effect = composition.AddEffect("test", -1, 1, 2, StandardEffects.CreateBlurEffect(2, 2, 10));
                Assert.AreEqual("test", effect.Name);
                Assert.AreEqual(1, effect.Offset);
                Assert.AreEqual(2, effect.Duration);
                Assert.AreEqual(1, composition.Effects.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [TestMethod]
        public void AddTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.AddingTrack += delegate { firedBefore = true; };

                composition.AddedTrack += delegate { firedAfter = true; };

                ITrack track = composition.AddTrack();
                Assert.AreEqual(1, composition.Tracks.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [TestMethod]
        public void AddTransition()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.AddingTransition += delegate { firedBefore = true; };

                composition.AddedTransition += delegate { firedAfter = true; };

                ITransition transition =
                    composition.AddTransition("test", 0, 2, StandardTransitions.CreateFade(), false);
                Assert.AreEqual(1, composition.Transitions.Count);
                Assert.AreEqual("test", transition.Name);
                Assert.AreEqual(0, transition.Offset);
                Assert.AreEqual(2, transition.Duration);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [TestMethod]
        public void CompositionPriorities()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition first = group.AddComposition("first", -1);
                IComposition second = group.AddComposition("second", 0);
                IComposition third = group.AddComposition("third", 1);
                IComposition fourth = group.AddComposition("fourth", -1);
                IComposition fifth = group.AddComposition("fifth", 2);
                Assert.AreEqual(3, first.Priority);
                Assert.AreEqual(0, second.Priority);
                Assert.AreEqual(1, third.Priority);
                Assert.AreEqual(4, fourth.Priority);
                Assert.AreEqual(2, fifth.Priority);
            }
        }

        [TestMethod]
        public void EffectPrioirities()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                EffectDefinition definition = StandardEffects.CreateBlurEffect(2, 10, 2);

                IGroup group = timeline.AddAudioGroup();
                IEffect first = group.AddEffect("first", -1, 0, 10, definition);
                IEffect second = group.AddEffect("second", 0, 0, 10, definition);
                IEffect third = group.AddEffect("third", 1, 0, 10, definition);
                IEffect fourth = group.AddEffect("fourth", -1, 0, 10, definition);
                IEffect fifth = group.AddEffect("fifth", 2, 0, 10, definition);
                Assert.AreEqual(3, first.Priority);
                Assert.AreEqual(0, second.Priority);
                Assert.AreEqual(1, third.Priority);
                Assert.AreEqual(4, fourth.Priority);
                Assert.AreEqual(2, fifth.Priority);
            }
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

            using (ITimeline timeline = new DefaultTimeline())
            {
                IComposition composition = timeline.AddAudioGroup().AddComposition();
                composition.AddedComposition += incrementForAfterCompositionAdded;
                composition.AddedEffect += incrementForAfterEffectAdded;
                composition.AddedTrack += incrementForAfterTrackAdded;
                composition.AddedTransition += incrementForAfterTransitionAdded;
                composition.AddedClip += incrementForAfterClipAdded;
                composition.AddingComposition += increment;
                composition.AddingEffect += increment;
                composition.AddingTrack += increment;
                composition.AddingTransition += increment;
                composition.AddingClip += increment;

                composition.AddComposition();
                composition.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                composition.AddTrack().AddClip("..\\..\\testinput.mp3", GroupMediaType.Audio, InsertPosition.Absolute, 0, 0, 1);
                composition.AddTransition(0, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(10, count);
                count = 0;

                composition.AddedComposition -= incrementForAfterCompositionAdded;
                composition.AddedEffect -= incrementForAfterEffectAdded;
                composition.AddedTrack -= incrementForAfterTrackAdded;
                composition.AddedTransition -= incrementForAfterTransitionAdded;
                composition.AddedClip -= incrementForAfterClipAdded;
                composition.AddingComposition -= increment;
                composition.AddingEffect -= increment;
                composition.AddingTrack -= increment;
                composition.AddingTransition -= increment;
                composition.AddingClip -= increment;

                composition.AddComposition();
                composition.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                composition.AddTrack();
                composition.AddTransition(2, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(0, count);
            }
        }

        [TestMethod]
        public void TrackAddEffectBubblesToComposition()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                composition.AddingEffect += delegate { beforeCount++; };

                composition.AddedEffect += delegate { afterCount++; };

                ITrack track = composition.AddTrack();
                track.AddEffect("test", -1, 1, 2, StandardEffects.CreateBlurEffect(2, 2, 10));

                Assert.AreEqual(1, beforeCount);
                Assert.AreEqual(1, afterCount);
            }
        }

        [TestMethod]
        public void TrackPrioirities()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ITrack first = group.AddTrack("first", -1);
                ITrack second = group.AddTrack("second", 0);
                ITrack third = group.AddTrack("third", 1);
                ITrack fourth = group.AddTrack("fourth", -1);
                ITrack fifth = group.AddTrack("fifth", 2);
                Assert.AreEqual(3, first.Priority);
                Assert.AreEqual(0, second.Priority);
                Assert.AreEqual(1, third.Priority);
                Assert.AreEqual(4, fourth.Priority);
                Assert.AreEqual(2, fifth.Priority);
            }
        }
    }
}