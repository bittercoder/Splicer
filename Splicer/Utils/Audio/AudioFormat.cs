namespace Splicer.Utils.Audio
{
    public class AudioFormat
    {
        private string _audioCompressor;
        private bool _isMono;
        private int _khz;
        private int _kbps;
        private bool _useDefaults;

        public AudioFormat()
        {
        }

        public AudioFormat(string audioCompressor)
        {
            _audioCompressor = audioCompressor;
            _useDefaults = true;
        }

        public AudioFormat(string audioCompressor, bool isMono, int khz, int kbps)
        {
            _audioCompressor = audioCompressor;
            _isMono = isMono;
            _khz = khz;
            _kbps = kbps;
        }

        public string AudioCompressor
        {
            get { return _audioCompressor; }
            set { _audioCompressor = value; }
        }

        public bool IsMono
        {
            get { return _isMono; }
            set { _isMono = value; }
        }

        public int Khz
        {
            get { return _khz; }
            set { _khz = value; }
        }

        public int Kbps
        {
            get { return _kbps; }
            set { _kbps = value; }
        }

        public bool UseDefaults
        {
            get { return _useDefaults; }
            set { _useDefaults = value; }
        }
    }
}