// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
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