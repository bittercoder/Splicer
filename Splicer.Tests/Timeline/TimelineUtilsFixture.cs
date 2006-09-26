using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class TimelineUtilsFixture
    {
        [Test]
        public void ToSeconds()
        {
            Assert.AreEqual(1, TimelineUtils.ToSeconds(10000000));
            Assert.AreEqual(-1, TimelineUtils.ToSeconds(-1));
        }

        [Test]
        public void ToUnits()
        {
            Assert.AreEqual(10000000, TimelineUtils.ToUnits(1));
            Assert.AreEqual(-1, TimelineUtils.ToUnits(-1));
        }
    }
}