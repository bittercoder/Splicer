// Copyright 2006-2008 Splicer Project - http://www.codeplex.com/splicer/
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using DirectShowLib;

namespace Splicer.Utilities.Audio
{
    public class AudioEncoder : IDisposable
    {
        private const string FriendlyNameParameter = "friendlyName";
        private readonly string _friendlyName;
        private IBaseFilter _filter;
        private WavFormatInfoCollection _formats;

        public AudioEncoder(string friendlyName, IBaseFilter filter)
        {
            _friendlyName = friendlyName;
            _filter = filter;
        }

        public string FriendlyName
        {
            get { return _friendlyName; }
        }

        public IBaseFilter Filter
        {
            get { return _filter; }
        }

        public WavFormatInfoCollection Formats
        {
            get
            {
                if (_formats == null)
                {
                    _formats = new WavFormatInfoCollection(_filter);
                }

                return _formats;
            }
        }

        #region IDisposable Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static AudioEncoder FindByFriendlyName(string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName)) throw new ArgumentNullException(FriendlyNameParameter);

            IEnumerator<AudioEncoder> enumerator = EnumerateEncoders();
            while (enumerator.MoveNext())
            {
                if (string.Compare(friendlyName, enumerator.Current.FriendlyName, true, CultureInfo.InvariantCulture) ==
                    0)
                {
                    return enumerator.Current;
                }
                else
                {
                    enumerator.Current.Dispose();
                }
            }

            return null;
        }

        public static IEnumerator<AudioEncoder> EnumerateEncoders()
        {
            ICreateDevEnum deviceEnumerator = null;
            try
            {
                deviceEnumerator = (ICreateDevEnum) new CreateDevEnum();

                IEnumMoniker monikerEnum = null;

                try
                {
                    int hr =
                        deviceEnumerator.CreateClassEnumerator(FilterGraphTools.AudioCompressorCategoryClassId,
                                                               out monikerEnum, CDef.None);
                    DsError.ThrowExceptionForHR(hr);

                    var monikers = new IMoniker[1];

                    while (true)
                    {
                        try
                        {
                            hr = monikerEnum.Next(1, monikers, IntPtr.Zero);

                            DsError.ThrowExceptionForHR(hr);

                            object bag;

                            Guid id = FilterGraphTools.IPropertyBagInterfaceId;

                            if (monikers[0] == null) break;

                            monikers[0].BindToStorage(null, null, ref id, out bag);

                            var propertyBag = (IPropertyBag) bag;

                            Marshal.ReleaseComObject(propertyBag);

                            object variantName;
                            hr = propertyBag.Read(FriendlyNameParameter, out variantName, null);
                            DsError.ThrowExceptionForHR(hr);

                            string friendlyName = Convert.ToString(variantName, CultureInfo.InvariantCulture);

                            Guid id2 = FilterGraphTools.IBaseFilterInterfaceId;

                            object filter;

                            monikers[0].BindToObject(null, null, ref id2, out filter);

                            if (filter != null)
                            {
                                yield return new AudioEncoder(friendlyName, (IBaseFilter) filter);
                            }
                        }
                        finally
                        {
                            if (monikers[0] != null) Marshal.ReleaseComObject(monikers[0]);
                        }
                    }
                }
                finally
                {
                    if (monikerEnum != null) Marshal.ReleaseComObject(monikerEnum);
                }
            }
            finally
            {
                if (deviceEnumerator != null) Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~AudioEncoder()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_formats != null)
                {
                    _formats.Dispose();
                    _formats = null;
                }
            }

            if (_filter != null)
            {
                Marshal.ReleaseComObject(_filter);
                _filter = null;
            }
        }
    }
}