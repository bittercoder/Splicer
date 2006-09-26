using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class StandardTransitionsFixture
    {
        [Test]
        public void CreateRgbDxtKey()
        {
            TransitionDefinition definition =
                StandardTransitions.CreateDxtKey(DxtKeyTypes.RGB, null, null, null, 0x00FF00, 5);
        }

        [Test]
        public void CreateAlphaDxtKey()
        {
            TransitionDefinition definition =
                StandardTransitions.CreateDxtKey(DxtKeyTypes.Alpha, null, null, null, null, null);
        }
    }
}