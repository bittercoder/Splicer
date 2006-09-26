using System;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class CompositionFixture : AbstractFixture
    {
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

            using (ITimeline timeline = new DefaultTimeline())
            {
                IComposition composition = timeline.AddAudioGroup().AddComposition();
                composition.AfterCompositionAdded += incrementForAfterCompositionAdded;
                composition.AfterEffectAdded += incrementForAfterEffectAdded;
                composition.AfterTrackAdded += incrementForAfterTrackAdded;
                composition.AfterTransitionAdded += incrementForAfterTransitionAdded;
                composition.AfterClipAdded += incrementForAfterClipAdded;
                composition.BeforeCompositionAdded += increment;
                composition.BeforeEffectAdded += increment;
                composition.BeforeTrackAdded += increment;
                composition.BeforeTransitionAdded += increment;
                composition.BeforeClipAdded += increment;

                composition.AddComposition();
                composition.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                composition.AddTrack().AddClip("testinput.mp3", GroupMediaType.Audio, InsertPosition.Absoloute, 0, 0, 1);
                composition.AddTransition(0, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(10, count);
                count = 0;

                composition.AfterCompositionAdded -= incrementForAfterCompositionAdded;
                composition.AfterEffectAdded -= incrementForAfterEffectAdded;
                composition.AfterTrackAdded -= incrementForAfterTrackAdded;
                composition.AfterTransitionAdded -= incrementForAfterTransitionAdded;
                composition.AfterClipAdded -= incrementForAfterClipAdded;
                composition.BeforeCompositionAdded -= increment;
                composition.BeforeEffectAdded -= increment;
                composition.BeforeTrackAdded -= increment;
                composition.BeforeTransitionAdded -= increment;
                composition.BeforeClipAdded -= increment;

                composition.AddComposition();
                composition.AddEffect(0, 2, StandardEffects.CreateDefaultBlur());
                composition.AddTrack();
                composition.AddTransition(2, 2, StandardTransitions.CreateFade());

                Assert.AreEqual(0, count);
            }
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
        public void AddCompositionWithName()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition composition = group.AddComposition("named", -1);
                Assert.AreEqual("named", composition.Name);
            }
        }

        [Test]
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

                composition.BeforeCompositionAdded += new EventHandler(delegate
                    {
                        firedBefore = true;
                    });

                composition.AfterCompositionAdded += new EventHandler<AfterCompositionAddedEventArgs>(delegate
                    {
                        firedAfter = true;
                    });

                IComposition childComposition = composition.AddComposition();
                Assert.AreSame(composition, childComposition.Container);
                Assert.AreSame(group, childComposition.Group);
                Assert.AreEqual(1, composition.Compositions.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [Test]
        public void AddTrack()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.BeforeTrackAdded += new EventHandler(delegate
                    {
                        firedBefore = true;
                    });

                composition.AfterTrackAdded += new EventHandler<AfterTrackAddedEventArgs>(delegate
                    {
                        firedAfter = true;
                    });

                ITrack track = composition.AddTrack();
                Assert.AreEqual(1, composition.Tracks.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [Test]
        public void AddTransition()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.BeforeTransitionAdded += new EventHandler(delegate
                    {
                        firedBefore = true;
                    });

                composition.AfterTransitionAdded += new EventHandler<AfterTransitionAddedEventArgs>(delegate
                    {
                        firedAfter = true;
                    });

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

        [Test]
        public void AddEffect()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                bool firedBefore = false;
                bool firedAfter = false;

                composition.BeforeEffectAdded += new EventHandler(delegate
                    {
                        firedBefore = true;
                    });

                composition.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        firedAfter = true;
                    });

                IEffect effect = composition.AddEffect("test", -1, 1, 2, StandardEffects.CreateBlurEffect(2, 2, 10));
                Assert.AreEqual("test", effect.Name);
                Assert.AreEqual(1, effect.Offset);
                Assert.AreEqual(2, effect.Duration);
                Assert.AreEqual(1, composition.Effects.Count);
                Assert.IsTrue(firedBefore);
                Assert.IsTrue(firedAfter);
            }
        }

        [Test]
        public void TrackAddEffectBubblesToComposition()
        {
            int beforeCount = 0;
            int afterCount = 0;

            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IComposition composition = group.AddComposition();

                composition.BeforeEffectAdded += new EventHandler(delegate
                    {
                        beforeCount++;
                    });

                composition.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(delegate
                    {
                        afterCount++;
                    });

                ITrack track = composition.AddTrack();
                track.AddEffect("test", -1, 1, 2, StandardEffects.CreateBlurEffect(2, 2, 10));

                Assert.AreEqual(1, beforeCount);
                Assert.AreEqual(1, afterCount);
            }
        }
    }
}