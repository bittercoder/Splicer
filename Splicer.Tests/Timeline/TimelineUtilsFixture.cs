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
    public class TimelineUtilsFixture
    {
        [TestMethod]
        public void ToSeconds()
        {
            Assert.AreEqual(1, TimelineBuilder.ToSeconds(10000000));
            Assert.AreEqual(-1, TimelineBuilder.ToSeconds(-1));
        }

        [TestMethod]
        public void ToUnits()
        {
            Assert.AreEqual(10000000, TimelineBuilder.ToUnits(1));
            Assert.AreEqual(-1, TimelineBuilder.ToUnits(-1));
        }
    }
}