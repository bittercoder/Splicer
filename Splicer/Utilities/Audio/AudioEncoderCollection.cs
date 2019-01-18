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
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;

namespace Splicer.Utilities.Audio
{
    public class AudioEncoderCollection : IEnumerable<AudioEncoder>, IDisposable
    {
        private const string FriendlyNameParameter = "FriendlyName";

        private List<AudioEncoder> _encoders = new List<AudioEncoder>();

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public AudioEncoderCollection()
        {
            IEnumerator<AudioEncoder> encoders = AudioEncoder.EnumerateEncoders();
            while (encoders.MoveNext())
            {
                _encoders.Add(encoders.Current);
            }
        }

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
            get { return _encoders.Find(delegate(AudioEncoder encoder) { return encoder.FriendlyName == friendlyName; }); }
        }

        #region IDisposable Members

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerable<AudioEncoder> Members

        public IEnumerator<AudioEncoder> GetEnumerator()
        {
            return _encoders.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _encoders).GetEnumerator();
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~AudioEncoderCollection()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (AudioEncoder encoder in _encoders)
                {
                    encoder.Dispose();
                }

                _encoders = null;
            }
        }
    }
}