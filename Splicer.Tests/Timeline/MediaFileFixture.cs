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
    public class MediaFileFixture
    {
        [Test]
        public void Construct()
        {
            MediaFile file = new MediaFile("clock.avi");
            Assert.AreEqual("clock.avi", file.FileName);
            Assert.AreEqual(12, file.Length);
            Assert.AreEqual(TimelineUtils.ToUnits(12), file.LengthInUnits);
            Assert.AreEqual(-1, file.LengthInFrames); // not assigned till later
        }

        [Test]
        public void SetLength()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = TimelineUtils.ToUnits(2);
            Assert.AreEqual(2, file.Length);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Invalid length specified")]
        public void SetLengthTooLong()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = TimelineUtils.ToUnits(20);
        }

        [Test]
        public void SetLengthToZero()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = 0;
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Invalid length specified")]
        public void SetLengthToNegative()
        {
            MediaFile file = new MediaFile("clock.avi");
            file.LengthInUnits = -1;
        }
    }
}