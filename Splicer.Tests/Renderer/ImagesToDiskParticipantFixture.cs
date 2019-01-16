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
using System.IO;
//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Splicer.Renderer;
using Splicer.Timeline;

namespace Splicer.Tests.Renderer
{
    [TestClass]
    public class ImagesToDiskParticipantFixture
    {
        #region Setup/Teardown

        [TestInitialize]
        public void SetUp()
        {
            for (int i = 0; i < 6; i++)
            {
                File.Delete(string.Format("frame{0}.jpg", i));
            }
        }

        #endregion

        [TestMethod]
        public void WriteSomeImages()
        {
            using (var timeline = new DefaultTimeline())
            {
                timeline.AddVideoGroup(24, 320, 240).AddTrack(); // we want 320x240 sized images
                timeline.AddVideo("..\\..\\transitions.wmv");

                var participant = new ImagesToDiskParticipant(24, 320, 240, Environment.CurrentDirectory, 1, 2, 3, 4, 5,
                                                              6, 7);

                using (var render = new NullRenderer(timeline, null, new ICallbackParticipant[] {participant}))
                {
                    render.Render();
                }

                for (int i = 0; i < 6; i++)
                {
                    Assert.IsTrue(File.Exists(string.Format("frame{0}.jpg", i)));
                }
            }
        }
    }
}