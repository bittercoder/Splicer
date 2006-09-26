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

using System;
using System.Xml;
using NUnit.Framework;
using Splicer.Renderer;
using Splicer.Timeline;
using Splicer.Utils;

namespace Splicer
{
    public abstract class AbstractFixture
    {
        protected void AssertXml(string expected, string actual)
        {
            XmlDocument expectedDoc = new XmlDocument();
            expectedDoc.LoadXml(expected);
            expected = expectedDoc.OuterXml;

            XmlDocument actualDoc = new XmlDocument();
            actualDoc.LoadXml(actual);
            actual = actualDoc.OuterXml;

            Assert.AreEqual(expected, actual);
        }

        protected void AssertLengths(ITimeline timeline, double expected, string file)
        {
            AssertLengths(timeline.FPS, expected, file);
        }

        protected void AssertLengths(double fps, double expected, string file)
        {
            long length1 = TimelineUtils.ToUnits(expected);
            long length2 = MediaDetUtils.GetLength(file);

            long frameLength = TimelineUtils.ToUnits(1.0/fps)*4; // allow for 4 frames difference

            long difference = Math.Abs(length1 - length2);
            Assert.IsTrue(difference <= frameLength,
                          string.Format("expected {0} +/- {1}, but was {2}", length1, frameLength, length2));
        }

        protected void AssertLengths(ITimeline timeline, string file1, string file2)
        {
            AssertLengths(timeline.FPS, file1, file2);
        }

        protected void AssertLengths(double fps, string file1, string file2)
        {
            long length1 = MediaDetUtils.GetLength(file1);
            long length2 = MediaDetUtils.GetLength(file2);

            long frameLength = TimelineUtils.ToUnits(1.0/fps);

            long difference = Math.Abs(length1 - length2);
            Assert.IsTrue(difference <= frameLength);
        }

        protected void PrepareToExecute(ITimeline timeline, string expectedXml)
        {
            using (NullRenderer renderer = new NullRenderer(timeline))
            {
                PrepareToExecute(renderer, expectedXml);
            }
        }

        protected void PrepareToExecute(IRenderer renderer, string expectedXml)
        {
            string actualXml = renderer.ToXml();
#if DEBUG
            Console.WriteLine(actualXml);
#endif
            AssertXml(expectedXml, actualXml);
#if DEBUG
            renderer.SaveToGraphFile("current.grf");
#endif
        }

        protected void ExecuteRenderer(IRenderer renderer, string expectedXml)
        {
            PrepareToExecute(renderer, expectedXml);
            renderer.Render();
        }
    }
}