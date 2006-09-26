// Copyright 2004-2006 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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