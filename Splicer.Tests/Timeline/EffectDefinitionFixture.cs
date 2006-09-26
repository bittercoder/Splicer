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
using Splicer.Timeline;
using Splicer.Utils;

namespace Splicer.Tests.Timeline
{
    [TestFixture]
    public class EffectDefinitionFixture
    {
        [Test]
        public void Constructor1()
        {
            EffectDefinition definition = new EffectDefinition();
            Assert.AreEqual(Guid.Empty, definition.EffectId);
            Assert.AreEqual(0, definition.Parameters.Count);
        }

        [Test]
        public void Constructor2()
        {
            EffectDefinition definition = new EffectDefinition(DxtSubObjects.sIEMatrixFxGuid);
            Assert.AreEqual(DxtSubObjects.sIEMatrixFxGuid, definition.EffectId);
            Assert.AreEqual(0, definition.Parameters.Count);
        }

        [Test]
        public void SetValues()
        {
            EffectDefinition definition = new EffectDefinition();
            List<Parameter> newParams = new List<Parameter>();
            definition.Parameters = newParams;
            Assert.AreSame(newParams, definition.Parameters);

            definition.EffectId = DxtSubObjects.AudioMixer;
            Assert.AreEqual(definition.EffectId, DxtSubObjects.AudioMixer);
        }
    }
}