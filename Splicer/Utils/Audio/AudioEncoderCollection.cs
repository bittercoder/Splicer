using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DirectShowLib;

namespace Splicer.Utils.Audio
{
    public class AudioEncoderCollection : IEnumerable<AudioEncoder>
    {
        private List<AudioEncoder> _encoders = new List<AudioEncoder>();

        public AudioEncoderCollection()
        {
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

                            object bag;

                            Guid id = FilterGraphTools.IID_IPropertyBag;

                            if (monikers[0] == null) break;

                            monikers[0].BindToStorage(null, null, ref id, out bag);

                            IPropertyBag propertyBag = (IPropertyBag) bag;

                            Marshal.ReleaseComObject(propertyBag);

                            object variantName;
                            hr = propertyBag.Read("FriendlyName", out variantName, null);
                            DsError.ThrowExceptionForHR(hr);

                            string friendlyName = Convert.ToString(variantName);

                            Guid id2 = FilterGraphTools.IID_IBaseFilter;

                            object filter;

                            monikers[0].BindToObject(null, null, ref id2, out filter);

                            if (filter != null)
                            {
                                _encoders.Add(new AudioEncoder(friendlyName, (IBaseFilter) filter));
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

        #region IEnumerable<AudioEncoder> Members

        public IEnumerator<AudioEncoder> GetEnumerator()
        {
            return _encoders.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _encoders).GetEnumerator();
        }

        #endregion

        public int Count
        {
            get { return _encoders.Count; }
        }

        public AudioEncoder this[int index]
        {
            get { return _encoders[index]; }
        }

        public AudioEncoder this[string friendlyName]
        {
            get
            {
                return _encoders.Find(delegate(AudioEncoder encoder)
                    {
                        return encoder.FriendlyName == friendlyName;
                    });
            }
        }
    }
}