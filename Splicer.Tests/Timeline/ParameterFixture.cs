using System.Collections.Generic;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class ParameterFixture
    {
        [Test]
        public void Construct1()
        {
            Parameter parameter = new Parameter("param1", (long) 5, 10, (long) 20);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [Test]
        public void Construct2()
        {
            Parameter parameter = new Parameter("param1", (double) 5.0, 10, (double) 20);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [Test]
        public void Construct3()
        {
            Parameter parameter = new Parameter("param1", "5", 10, "20");
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [Test]
        public void Construct4()
        {
            Parameter parameter = new Parameter("param1", (long) 5);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
        }

        [Test]
        public void Construct5()
        {
            Parameter parameter = new Parameter("param1", (double) 5.0);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
        }

        [Test]
        public void Construct6()
        {
            Parameter parameter = new Parameter("param1", true);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("True", parameter.Value);
        }

        [Test]
        public void Construct7()
        {
            Parameter parameter = new Parameter("param1", "text");
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("text", parameter.Value);
        }

        [Test]
        public void SetValues()
        {
            Parameter parameter = new Parameter();
            parameter.DispId = 10;
            Assert.AreEqual(10, parameter.DispId);

            List<Interval> intervals = new List<Interval>();
            parameter.Intervals = intervals;
            Assert.AreSame(intervals, parameter.Intervals);

            parameter.Name = "name";
            Assert.AreEqual("name", parameter.Name);

            parameter.Value = "value";
            Assert.AreEqual("value", parameter.Value);
        }
    }
}