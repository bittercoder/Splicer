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