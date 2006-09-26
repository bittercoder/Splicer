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
using System.Collections.Generic;
using NUnit.Framework;
using Splicer.Utils;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class TransitionDefinitionFixture
    {
        [Test]
        public void Construct1()
        {
            TransitionDefinition definition = new TransitionDefinition();
            Assert.AreEqual(Guid.Empty, definition.TransitionId);
            Assert.AreEqual(0, definition.Parameters.Count);
        }

        [Test]
        public void Construct2()
        {
            TransitionDefinition definition = new TransitionDefinition(DxtSubObjects.FadeTransition);
            Assert.AreEqual(DxtSubObjects.FadeTransition, definition.TransitionId);
            Assert.AreEqual(0, definition.Parameters.Count);
        }

        [Test]
        public void SetValues()
        {
            TransitionDefinition definition = new TransitionDefinition();
            List<Parameter> newParams = new List<Parameter>();
            definition.Parameters = newParams;
            Assert.AreSame(newParams, definition.Parameters);

            definition.TransitionId = DxtSubObjects.FadeTransition;
            Assert.AreEqual(DxtSubObjects.FadeTransition, definition.TransitionId);
        }
    }
}