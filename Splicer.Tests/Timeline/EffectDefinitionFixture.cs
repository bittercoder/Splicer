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