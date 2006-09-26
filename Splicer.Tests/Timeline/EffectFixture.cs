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

using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class EffectFixture
    {
        [Test]
        public void AddEffectSetsApropriateContainer()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                EffectDefinition def = StandardEffects.CreateDefaultBlur();

                IGroup group = timeline.AddVideoGroup(24, 100, 100);
                IEffect groupEffect = group.AddEffect(0, 10, def);
                Assert.AreSame(group, groupEffect.Group);
                Assert.AreSame(group, groupEffect.Container);

                ITrack track = group.AddTrack();
                IEffect trackEffect = track.AddEffect(0, 10, def);
                Assert.AreSame(group, trackEffect.Group);
                Assert.AreSame(track, trackEffect.Container);

                IComposition composition = group.AddComposition();
                IEffect compositionEffect = composition.AddEffect(0, 10, def);
                Assert.AreSame(group, compositionEffect.Group);
                Assert.AreSame(composition, compositionEffect.Container);

                IClip clip = track.AddClip("image1.jpg", GroupMediaType.Image, InsertPosition.Absoloute, 0, 0, 10);
                IEffect clipEffect = clip.AddEffect(0, 10, def);
                Assert.AreSame(group, clip.Group);
                Assert.AreSame(clip, clipEffect.Container);
            }
        }
    }
}