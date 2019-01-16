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

using System;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Splicer.Timeline.Tests
{
    [TestClass]
    public class AfterAddedToContainerEventArgsFixture
    {
        [TestMethod]
        public void Construct()
        {
            using (ITimeline timeline = new DefaultTimeline())
            {
                IGroup group = timeline.AddAudioGroup();

                var args = new AddedGroupEventArgs(group, timeline);
                Assert.AreSame(group, args.Item);
                Assert.AreSame(timeline, args.Container);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullContainer()
        {
            var args = new AddedClipEventArgs(new MockClip(0, 10, 0), null);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ConstructWithNullItem()
        {
            var args = new AddedClipEventArgs(null, null);
        }
    }
}