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

namespace Splicer.Utilities
{
    public class MediaFileRegistration : IDisposable
    {
        private List<RegisteredMediaExtension> _registrations = new List<RegisteredMediaExtension>();

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void Add(RegisteredMediaExtension extension)
        {
            _registrations.Add(extension);
        }

        ~MediaFileRegistration()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_registrations != null)
                {
                    foreach (RegisteredMediaExtension registration in _registrations)
                    {
                        registration.Dispose();
                    }

                    _registrations = null;
                }
            }
        }
    }
}