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
    public class ParameterFixture
    {
        [TestMethod]
        public void Construct1()
        {
            var parameter = new Parameter("param1", 5, 10, 20);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [TestMethod]
        public void Construct2()
        {
            var parameter = new Parameter("param1", 5.0, 10, 20);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [TestMethod]
        public void Construct3()
        {
            var parameter = new Parameter("param1", "5", 10, "20");
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
            Assert.AreEqual(IntervalMode.Interpolate, parameter.Intervals[0].Mode);
            Assert.AreEqual(10, parameter.Intervals[0].Time);
            Assert.AreEqual("20", parameter.Intervals[0].Value);
        }

        [TestMethod]
        public void Construct4()
        {
            var parameter = new Parameter("param1", 5);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
        }

        [TestMethod]
        public void Construct5()
        {
            var parameter = new Parameter("param1", 5.0);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("5", parameter.Value);
        }

        [TestMethod]
        public void Construct6()
        {
            var parameter = new Parameter("param1", true);
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("True", parameter.Value);
        }

        [TestMethod]
        public void Construct7()
        {
            var parameter = new Parameter("param1", "text");
            Assert.AreEqual("param1", parameter.Name);
            Assert.AreEqual(0, parameter.DispId);
            Assert.AreEqual("text", parameter.Value);
        }
    }
}