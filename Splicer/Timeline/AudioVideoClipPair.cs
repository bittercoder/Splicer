using System;

namespace Splicer.Timeline
{
    public class AudioVideoClipPair : IAudioVideoClipPair
    {
        private IClip _videoClip;
        private IClip _audioClip;

        public AudioVideoClipPair(IClip audioClip, IClip videoClip)
        {
            if (audioClip == null) throw new ArgumentNullException("audioClip");
            if (videoClip == null) throw new ArgumentNullException("videoClip");

            _audioClip = audioClip;
            _videoClip = videoClip;
        }

        public IClip AudioClip
        {
            get { return _audioClip; }
        }

        public IClip VideoClip
        {
            get { return _videoClip; }
        }
    }
}