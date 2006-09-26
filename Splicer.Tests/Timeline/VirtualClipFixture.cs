using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class VirtualClipFixture
    {
        [Test]
        public void CompareTo()
        {
            VirtualClip clip = new VirtualClip(0, 10, 5, new MockClip(0, 10, 5));
            Assert.AreEqual(-1, clip.CompareTo((object) null));
            Assert.AreEqual(-1, clip.CompareTo((IClip) null));
        }

        [Test]
        public void RetrieveNameFromUnderlyingClip()
        {
            IClip clip = new MockClip("some clip", 0, 10, 5);
            VirtualClip virtualClip = new VirtualClip(0, 10, 5, clip);
            Assert.AreEqual(clip.Name, virtualClip.Name);
        }
    }
}