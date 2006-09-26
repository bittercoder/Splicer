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