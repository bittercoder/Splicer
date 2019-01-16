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
    public class VirtualClipFixture
    {
        [TestMethod]
        public void CompareTo()
        {
            var clip = new VirtualClip(0, 10, 5, new MockClip(0, 10, 5));
            Assert.AreEqual(-1, clip.CompareTo((object) null));
            Assert.AreEqual(-1, clip.CompareTo((IClip) null));
        }

        [TestMethod]
        public void RetrieveNameFromUnderlyingClip()
        {
            IClip clip = new MockClip("some clip", 0, 10, 5);
            var virtualClip = new VirtualClip(0, 10, 5, clip);
            Assert.AreEqual(clip.Name, virtualClip.Name);
        }
    }
}