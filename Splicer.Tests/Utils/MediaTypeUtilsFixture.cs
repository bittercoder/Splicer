using DirectShowLib;
using NUnit.Framework;

namespace Splicer.Utils.Tests
{
    [TestFixture]
    public class MediaTypeUtilsFixture
    {
        [Test]
        public void GetAudioMediaType()
        {
            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetAudioMediaType();
                Assert.IsNotNull(mediaType);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }
        }

        [Test]
        public void GetVideoMediaType()
        {
            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(16, 320, 200);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(24, 172, 160);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(32, 172, 160);
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }
        }
    }
}