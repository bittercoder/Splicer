using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class TransitionFixture
    {
        [Test]
        public void AddTransitionSetsAppropriateContainerAndGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                TransitionDefinition def = StandardTransitions.CreateIris();

                IGroup group = timeline.AddAudioGroup();
                ITransition groupTransition = group.AddTransition(0, 0, def);
                Assert.AreSame(group, groupTransition.Container);
                Assert.AreSame(group, groupTransition.Group);

                ITrack track = group.AddTrack();
                ITransition trackTransition = track.AddTransition(0, 0, def);
                Assert.AreSame(track, trackTransition.Container);
                Assert.AreSame(group, trackTransition.Group);

                IComposition composition = group.AddComposition();
                ITransition compositionTransition = composition.AddTransition(0, 0, def);
                Assert.AreSame(composition, compositionTransition.Container);
                Assert.AreSame(group, compositionTransition.Group);
            }
        }

        [Test]
        public void AddTransitionSetsSwappedInputsProperly()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                TransitionDefinition def = StandardTransitions.CreateIris();

                IGroup group = timeline.AddAudioGroup();
                ITransition groupTransition1 = group.AddTransition(0, 5, def, false);
                Assert.IsFalse(groupTransition1.SwapInputs);

                ITransition groupTransition2 = group.AddTransition(5, 5, def, true);
                Assert.IsTrue(groupTransition2.SwapInputs);
            }
        }

        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "Propsective transition overlaps with an existing transition at index: 0")]
        public void AddingTransitionsChecksForOverlap()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                TransitionDefinition def = StandardTransitions.CreateIris();

                IGroup group = timeline.AddAudioGroup();
                ITransition groupTransition1 = group.AddTransition(0, 5, def, false);
                Assert.IsFalse(groupTransition1.SwapInputs);

                ITransition groupTransition2 = group.AddTransition(1, 7, def, true);
                Assert.IsTrue(groupTransition2.SwapInputs);
            }
        }
    }
}