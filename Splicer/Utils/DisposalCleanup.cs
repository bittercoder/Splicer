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
using System.Collections;
using System.Runtime.InteropServices;

namespace Splicer.Utils
{
    /// <summary>Provides for easy cleanup of all COM objects used in its scope.</summary>
    public sealed class DisposalCleanup : IDisposable
    {
        /// <summary>Stores the list of items to be disposed when this instance is disposed.</summary>
        private ArrayList _toDispose = new ArrayList();

        /// <summary>Adds any number of objects to be disposed.</summary>
        /// <param name="toDispose">The list of IDisposable objects, COM objects, or interface IntPtrs to be disposed.</param>
        public void Add(params object[] toDispose)
        {
            if (_toDispose == null) throw new ObjectDisposedException(GetType().Name);
            if (toDispose != null)
            {
                foreach (object obj in toDispose)
                {
                    if (obj != null && (obj is IDisposable || obj.GetType().IsCOMObject || obj is IntPtr) &&
                        (!_toDispose.Contains(obj)))
                    {
                        _toDispose.Add(obj);
                    }
                }
            }
        }

        /// <summary>Disposes of all registered resources.</summary>
        void IDisposable.Dispose()
        {
            if (_toDispose != null)
            {
                foreach (object obj in _toDispose) EnsureCleanup(obj);
                _toDispose = null;
            }
        }

        /// <summary>Disposes of the specified object.</summary>
        /// <param name="toDispose">The object to be disposed.</param>
        private void EnsureCleanup(object toDispose)
        {
            try
            {
                if (toDispose is IDisposable)
                {
                    ((IDisposable) toDispose).Dispose();
                }
                else if (toDispose is IntPtr) // assumes IntPtrs are interface pointers
                {
                    Marshal.Release((IntPtr) toDispose);
                }
                else if (toDispose.GetType().IsCOMObject)
                {
                    while (Marshal.ReleaseComObject(toDispose) > 0) ;
                }
            }
            catch (InvalidComObjectException)
            {
            }
        }
    }
}