namespace Splicer.Utils.Audio
{
    public static class CommonAudioFormats
    {
        public static readonly string MpegLayer3FriendlyName = "MPEG Layer-3";
        public static readonly string PCMFriendlyName = "PCM";
        public static readonly string OggMode3PlusFriendlyName = "Ogg Vorbis (mode3+)";
        public static readonly string AC3ACMFriendlyName = "AC-3 ACM Codec";

        public static AudioFormat LowQualityAC3ACM
        {
            get { return new AudioFormat(AC3ACMFriendlyName, true, 32, 32); }
        }

        public static AudioFormat HighQualityAC3ACM
        {
            get { return new AudioFormat(AC3ACMFriendlyName, false, 44, 640); }
        }

        public static AudioFormat LowQualityMonoOggMode3Plus
        {
            get { return new AudioFormat(OggMode3PlusFriendlyName, true, 22, 64); }
        }

        public static AudioFormat DefaultOggMode3Plus
        {
            get { return new AudioFormat(OggMode3PlusFriendlyName); }
        }

        public static AudioFormat LowQualityMonoPcm
        {
            get { return new AudioFormat(PCMFriendlyName, true, 32, 256); }
        }

        public static AudioFormat CDQualityStereoPcm
        {
            get { return new AudioFormat(PCMFriendlyName, false, 44, 1411); }
        }
    }
}