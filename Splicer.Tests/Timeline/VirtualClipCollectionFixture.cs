using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class VirtualClipCollectionFixture
    {
        [Test]
        public void AddOneClips()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(1, 5, 2));

            Assert.AreEqual(1, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(2, collection[0].MediaStart);
        }

        [Test]
        public void AddTwoClips()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 5, 0));
            collection.AddVirtualClip(new MockClip(5, 5, 0));

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            Assert.AreEqual(5, collection[1].Offset);
            Assert.AreEqual(5, collection[1].Duration);
            Assert.AreEqual(0, collection[1].MediaStart);
        }

        [Test]
        public void Scenario1()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
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

        [Test]
        public void Scenario2()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(0, 7, 0));
            collection.AddVirtualClip(new MockClip(5, 10, 0));

            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(5, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);

            Assert.AreEqual(5, collection[1].Offset);
            Assert.AreEqual(10, collection[1].Duration);
            Assert.AreEqual(0, collection[1].MediaStart);
        }

        [Test]
        public void Scenario3()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
            collection.AddVirtualClip(new MockClip(2, 2, 0));
            collection.AddVirtualClip(new MockClip(0, 10, 0));

            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(0, collection[0].Offset);
            Assert.AreEqual(10, collection[0].Duration);
            Assert.AreEqual(0, collection[0].MediaStart);
        }

        [Test]
        public void Scenario4()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
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

        [Test]
        public void StronglyTypedEnumerate()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
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

        [Test]
        public void UntypedEnumerate()
        {
            VirtualClipCollection collection = new VirtualClipCollection();
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