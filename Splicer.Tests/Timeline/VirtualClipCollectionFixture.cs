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

using System.Collections;
using System.Collections.Generic;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Splicer.Timeline.Tests
{
    [TestClass]
    public class VirtualClipCollectionFixture
    {
        [TestMethod]
        public void AddOneClips()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(1, 5, 2));

            Assert.AreEqual(1, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(2, collection[0].MediaStart);
        }

        [TestMethod]
        public void AddTwoClips()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 5, 0));
            collection.AddVirtualClip(new MockClip(5, 5, 0));

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            Assert.AreEqual(5, collection[1].Offset);
            Assert.AreEqual(5, collection[1].Duration);
            Assert.AreEqual(0, collection[1].MediaStart);
        }

        [TestMethod]
        public void Scenario1()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(5, 10, 0));
            collection.AddVirtualClip(new MockClip(0, 7, 0));

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(7, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            // this should be reduced in length 2 seconds, and moved 2 seconds forward
            Assert.AreEqual(7, collection[1].Offset);
            Assert.AreEqual(8, collection[1].Duration);
            Assert.AreEqual(2, collection[1].MediaStart);
        }

        [TestMethod]
        public void Scenario2()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 7, 0));
            collection.AddVirtualClip(new MockClip(5, 10, 0));

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            Assert.AreEqual(5, collection[1].Offset);
            Assert.AreEqual(10, collection[1].Duration);
            Assert.AreEqual(0, collection[1].MediaStart);
        }

        [TestMethod]
        public void Scenario3()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(2, 2, 0));
            collection.AddVirtualClip(new MockClip(0, 10, 0));

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(10, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);
        }

        [TestMethod]
        public void Scenario4()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 10, 0));
            collection.AddVirtualClip(new MockClip(2, 2, 0));

            Assert.AreEqual(3, collection.Count);

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(2, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            Assert.AreEqual(2, collection[1].Offset);
            Assert.AreEqual(2, collection[1].Duration);
            Assert.AreEqual(0, collection[1].MediaStart);

            Assert.AreEqual(4, collection[2].Offset);
            Assert.AreEqual(6, collection[2].Duration);
            Assert.AreEqual(4, collection[2].MediaStart);
        }

        [TestMethod]
        public void StronglyTypedEnumerate()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 10, 0));
            collection.AddVirtualClip(new MockClip(2, 2, 0));

            // test the strongly typed enumerator

            int index = 0;

            IEnumerator<IVirtualClip> genericEnumerator = collection.GetEnumerator();
            while (genericEnumerator.MoveNext())
            {
                Assert.AreSame(genericEnumerator.Current, collection[index++]);
            }

            Assert.AreEqual(3, index);
        }

        [TestMethod]
        public void UntypedEnumerate()
        {
            var collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 10, 0));
            collection.AddVirtualClip(new MockClip(2, 2, 0));

            // and the untyped enumerator

            int index = 0;

            IEnumerator enumerator = ((IEnumerable) collection).GetEnumerator();
            while (enumerator.MoveNext())
            {
                Assert.AreSame(enumerator.Current, collection[index++]);
            }

            Assert.AreEqual(3, index);
        }
    }
}