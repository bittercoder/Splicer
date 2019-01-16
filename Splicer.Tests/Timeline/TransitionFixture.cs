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

namespace Splicer.Timeline.Tests
{
    [TestClass]
    public class TransitionFixture
    {
        [TestMethod]
        [ExpectedException(typeof(SplicerException),
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

        [TestMethod]
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

        [TestMethod]
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
    }
}