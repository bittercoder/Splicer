using NUnit.Framework;

namespace Splicer.Timeline.Tests
{
    [TestFixture]
    public class StandardEffectsFixture
    {
        [Test]
        public void CreateAlphaSetterRamp()
        {
            StandardEffects.CreateAlphaSetterRamp(0.5);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "The alpha ramp value must be a percentage between 0 and 1")]
        public void CreateAlphaSetterRampLowerThenZero()
        {
            StandardEffects.CreateAlphaSetterRamp(-1);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "The alpha ramp value must be a percentage between 0 and 1")]
        public void CreateAlphaSetterRampAboveOne()
        {
            StandardEffects.CreateAlphaSetterRamp(2);
        }

        [Test]
        public void CreateAlphaSetter()
        {
            StandardEffects.CreateAlphaSetter(0x99);
        }

        [Test]
        public void CreateAudioEnvelope()
        {
            StandardEffects.CreateAudioEnvelope(0.5, 5);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Volume percentage must be between 0 and 1")]
        public void CreateAudioEnvelopeVolumeTooLow1()
        {
            StandardEffects.CreateAudioEnvelope(-1, 5);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Volume percentage must be between 0 and 1")]
        public void CreateAudioEnvelopeVolumeTooLow2()
        {
            StandardEffects.CreateAudioEnvelope(-1, 5);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Volume percentage must be between 0 and 1")]
        public void CreateAudioEnvelopeVolumeTooHigh1()
        {
            StandardEffects.CreateAudioEnvelope(2, 1, 1, 5);
        }

        [Test]
        [ExpectedException(typeof (SplicerException), "Volume percentage must be between 0 and 1")]
        public void CreateAudioEnvelopeVolumeTooHigh2()
        {
            StandardEffects.CreateAudioEnvelope(2, 1, 1, 5);
        }

        [Test]
        public void CreateAudioEnvelopeWithFades()
        {
            StandardEffects.CreateAudioEnvelope(0.5, 1, 1, 5);
            StandardEffects.CreateAudioEnvelope(0.2, 0, 1, 5);
            StandardEffects.CreateAudioEnvelope(0.7, 1, 0, 5);
            StandardEffects.CreateAudioEnvelope(0.15, 0, 0, 5);
        }

        [Test]
        [
            ExpectedException(typeof (SplicerException),
                "Sum of the fade-in and fade-out durations must not exceed the total duration")]
        public void CreateAudioEnvelopeWithInvalidFadeDurations()
        {
            StandardEffects.CreateAudioEnvelope(0.5, 4, 4, 2);
        }
    }
}