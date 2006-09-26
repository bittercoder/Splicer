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

using System;
using System.Collections.Generic;
using DirectShowLib;
using Permissions_SecurityPermission=System.Security.Permissions.SecurityPermission;

namespace Splicer.Utils.Audio
{
    public class WavFormatInfoCollection : List<WavFormatInfo>, IDisposable
    {
        public WavFormatInfoCollection(IBaseFilter filter)
            : this(filter, PinDirection.Output)
        {
        }

        public WavFormatInfoCollection(IBaseFilter filter, PinDirection direction)
        {
            IEnumerator<WavFormatInfo> enumerator = WavFormatInfoUtils.EnumerateFormatsForDirection(filter, direction);
            while (enumerator.MoveNext()) Add(enumerator.Current);
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (WavFormatInfo info in this) info.Dispose();
        }

        #endregion
    }
}