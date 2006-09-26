using NUnit.Framework;

namespace Splicer.WindowsMedia.Tests
{
    [TestFixture]
    public class BundledProflesFixture
    {
        [Test]
        public void CanReadAll()
        {
            Assert.IsFalse(string.IsNullOrEmpty(WindowsMediaProfiles.HighQualityVideo));
            Assert.IsFalse(string.IsNullOrEmpty(WindowsMediaProfiles.LowQualityAudio));
            Assert.IsFalse(string.IsNullOrEmpty(WindowsMediaProfiles.LowQualityVideo));
            Assert.IsFalse(string.IsNullOrEmpty(WindowsMediaProfiles.MediumQualityAudio));
        }
    }
}