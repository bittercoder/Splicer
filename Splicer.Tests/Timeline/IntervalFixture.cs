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
    public class IntervalFixture
    {
        [TestMethod]
        public void Construct1()
        {
            var interval = new Interval();
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(0, interval.Time);
            Assert.IsNull(interval.Value);
        }

        [TestMethod]
        public void Construct2()
        {
            var interval = new Interval(IntervalMode.Interpolate, 1, "0.2");
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(1, interval.Time);
            Assert.AreEqual("0.2", interval.Value);
        }

        [TestMethod]
        public void Construct3()
        {
            var interval = new Interval(1, "0.2");
            Assert.AreEqual(IntervalMode.Interpolate, interval.Mode);
            Assert.AreEqual(1, interval.Time);
            Assert.AreEqual("0.2", interval.Value);
        }

        [TestMethod]
        public void SetValues()
        {
            var interval = new Interval();

            interval.Mode = IntervalMode.Jump;
            Assert.AreEqual(IntervalMode.Jump, interval.Mode);

            interval.Time = 2;
            Assert.AreEqual(2, interval.Time);

            interval.Value = "3.0";
            Assert.AreEqual("3.0", interval.Value);
        }
    }
}