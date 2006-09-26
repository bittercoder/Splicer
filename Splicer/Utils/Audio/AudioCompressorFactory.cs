using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public static class AudioCompressorFactory
    {
        public static AudioCompressor Create(AudioFormat format)
        {
            AudioEncoder encoder = AudioEncoder.FindByFriendlyName(format.AudioCompressor);
            if (encoder == null)
                throw new SplicerException(
                    string.Format("Can not resolve audio encoder \"{0}\"", format.AudioCompressor));

            if (!format.UseDefaults)
            {
                WavFormatInfo formatInfo = WavFormatInfoUtils.FindFormat(encoder.Filter, PinDirection.Output, format);
                if (formatInfo == null)
                    throw new SplicerException(
                        string.Format("Can not resolve media type for encoder ({0} khz {1}, kbps, mono? {2})",
                                      format.Khz, format.Kbps, format.IsMono));

                return new AudioCompressor(encoder.Filter, formatInfo.MediaType);
            }
            else
            {
                return new AudioCompressor(encoder.Filter, null);
            }
        }
    }
}