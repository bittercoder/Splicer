using System.Runtime.InteropServices;
using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public class AudioCompressor
    {
        private IBaseFilter _filter;
        private AMMediaType _mediaType;

        public AudioCompressor(IBaseFilter filter, AMMediaType mediaType)
        {
            _filter = filter;
            _mediaType = mediaType;
        }

        public IBaseFilter Filter
        {
            get { return _filter; }
        }

        public AMMediaType MediaType
        {
            get { return _mediaType; }
        }

        public void Release()
        {
            if (_filter != null)
            {
                Marshal.ReleaseComObject(_filter);
                _filter = null;
            }

            if (_mediaType != null)
            {
                DsUtils.FreeAMMediaType(_mediaType);
                _mediaType = null;
            }
        }
    }
}