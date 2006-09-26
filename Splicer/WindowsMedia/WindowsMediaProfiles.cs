using System.IO;

namespace Splicer.WindowsMedia
{
    public static class WindowsMediaProfiles
    {
        public static string HighQualityVideo
        {
            get { return ReadStream("HighQualityVideo.prx"); }
        }

        public static string LowQualityAudio
        {
            get { return ReadStream("LowQualityAudio.prx"); }
        }

        public static string LowQualityVideo
        {
            get { return ReadStream("LowQualityVideo.prx"); }
        }

        public static string MediumQualityAudio
        {
            get { return ReadStream("MediumQualityAudio.prx"); }
        }


        private static string ReadStream(string profileName)
        {
            using (
                Stream stream =
                    typeof (WindowsMediaProfiles).Assembly.GetManifestResourceStream(
                        string.Format("Splicer.WindowsMedia.{0}", profileName)))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}