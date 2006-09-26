using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class MediaFileFixture
    {
        [Test]
        public void Construct()
        {
            MediaFile file = new MediaFile("clock.avi");
            Assert.AreEqual("clock.avi", file.FileName);
            Assert.AreEqual(12, file.Length);
            Assert.AreEqual(TimelineUtils.ToUnits(12), file.LengthInUnits);
            Assert.AreEqual(-1, file.LengthInFrames); // not assigned till later
        }

        [Test]
        public void SetLength()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = TimelineUtils.ToUnits(2);
            Assert.AreEqual(2, file.Length);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Invalid length specified")]
        public void SetLengthTooLong()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = TimelineUtils.ToUnits(20);
        }

        [Test]
        public void SetLengthToZero()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = 0;
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Invalid length specified")]
        public void SetLengthToNegative()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = -1;
        }
    }
}