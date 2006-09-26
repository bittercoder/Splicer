using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class IntervalFixture
    {
        [Test]
        public void Construct1()
        {
            Interval interval = new Interval();
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(0, interval.Time);
            Assert.IsNull(interval.Value);
        }

        [Test]
        public void Construct2()
        {
            Interval interval = new Interval(IntervalMode.Interpolate, 1, "0.2");
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(1, interval.Time);
            Assert.AreEqual("0.2", interval.Value);
        }

        [Test]
        public void Construct3()
        {
            Interval interval = new Interval(1, "0.2");
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(1, interval.Time);
            Assert.AreEqual("0.2", interval.Value);
        }

        [Test]
        public void SetValues()
        {
            Interval interval = new Interval();

            interval.Mode = IntervalMode.Jump;
            Assert.AreEqual(IntervalMode.Jump, interval.Mode);

            interval.Time = 2;
            Assert.AreEqual(2, interval.Time);

            interval.Value = "3.0";
            Assert.AreEqual("3.0", interval.Value);
        }
    }
}