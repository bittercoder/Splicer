using System;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class AfterAddedToContainerEventArgsFixture
    {
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullItem()
        {
            AfterClipAddedEventArgs args = new AfterClipAddedEventArgs(null, null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullContainer()
        {
            AfterClipAddedEventArgs args = new AfterClipAddedEventArgs(new MockClip(0, 10, 0), null);
        }

        [Test]
        public void Construct()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();

                AfterGroupAddedEventArgs args = new AfterGroupAddedEventArgs(group, timeline);
                Assert.AreSame(group, args.Item);
                Assert.AreSame(timeline, args.Container);
            }
        }
    }
}