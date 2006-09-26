using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public class AudioEncoder : IDisposable
    {
        private string _friendlyName;
        private IBaseFilter _filter;
        private WavFormatInfoCollection _formats;

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public static AudioEncoder FindByFriendlyName(string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName)) throw new ArgumentNullException("friendlyName");

            ICreateDevEnum deviceEnumerator = null;
            try
            {
                deviceEnumerator = (ICreateDevEnum) new CreateDevEnum();

                IEnumMoniker monikerEnum = null;

                try
                {
                    int hr =
                        deviceEnumerator.CreateClassEnumerator(FilterGraphTools.CLSID_AudioCompressorCategory,
                                                               out monikerEnum, CDef.None);
                    DsError.ThrowExceptionForHR(hr);

                    IMoniker[] monikers = new IMoniker[1];

                    while (true)
                    {
                        try
                        {
                            hr = monikerEnum.Next(1, monikers, IntPtr.Zero);

                            DsError.ThrowExceptionForHR(hr);


                            Guid id = FilterGraphTools.IID_IPropertyBag;

                            if (monikers[0] == null) break;

                            string discoveredFriendlyName;
                            object bag = null;

                            try
                            {
                                monikers[0].BindToStorage(null, null, ref id, out bag);

                                IPropertyBag propertyBag = (IPropertyBag) bag;

                                object variantName;
                                hr = propertyBag.Read("FriendlyName", out variantName, null);
                                DsError.ThrowExceptionForHR(hr);

                                discoveredFriendlyName = Convert.ToString(variantName);
                            }
                            finally
                            {
                                if (bag != null) Marshal.ReleaseComObject(bag);
                            }

                            if (friendlyName.ToLowerInvariant() != discoveredFriendlyName.ToLowerInvariant())
                            {
                                continue;
                            }

                            Guid id2 = FilterGraphTools.IID_IBaseFilter;

                            object filter;

                            monikers[0].BindToObject(null, null, ref id2, out filter);

                            if (filter != null)
                            {
                                return new AudioEncoder(friendlyName, (IBaseFilter) filter);
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

            return null;
        }

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

        public void Dispose()
        {
            if (_filter != null)
            {
                Marshal.ReleaseComObject(_filter);
                _filter = null;
            }

            if (_formats != null)
            {
                _formats.Dispose();
                _formats = null;
            }
        }

        #endregion
    }
}