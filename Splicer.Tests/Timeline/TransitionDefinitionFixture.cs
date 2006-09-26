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