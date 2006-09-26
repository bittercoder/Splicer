using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class GroupFixture
    {
        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "Groups are top level timeline components and do not support this property")]
        public void GetGroupContainer()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                ICompositionContainer container = group.Container;
            }
        }

        [Test]
        public void GetGroupGroup()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();
                Assert.AreSame(group, group.Group);
            }
        }
    }
}