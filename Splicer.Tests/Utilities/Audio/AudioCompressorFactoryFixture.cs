//using NUnit.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Splicer.Utilities.Audio.Tests
{
    [TestClass]
    public class AudioCompressorFactoryFixture
    {
        [TestMethod]
        public void Create()
        {
            using (
                AudioCompressor compressor =
                    AudioCompressorFactory.Create(AudioFormat.CompactDiscQualityStereoPcm))
            {
                Assert.IsNotNull(compressor.Filter);
                Assert.IsNotNull(compressor.MediaType);
            }
        }

        [TestMethod]
        [ExpectedException(typeof (SplicerException))]
        public void CreateForInvalidBitrate()
        {
            var format = new AudioFormat(AudioFormat.PcmFriendlyName, true, 99, 88);
            AudioCompressorFactory.Create(format);
        }
    }
}