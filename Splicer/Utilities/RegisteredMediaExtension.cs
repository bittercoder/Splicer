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
using Microsoft.Win32;

namespace Splicer.Utilities
{
    /// <summary>
    /// This class can be used to override an existing extensions associated source filter, upon
    /// disposal it will restore the previous setting.  It's recomended that it be used for only
    /// very short durations ie. when adding media, so as to reduce the chance of a user concurrently
    /// installing a new filter.
    /// </summary>
    public class RegisteredMediaExtension : IDisposable
    {
        private const string DirectShowExtensionTableHKCRSubKey = @"Media Type\Extensions\";
        private const string DirectShowSourceFilterValueName = "Source Filter";
        private readonly bool _dontDispose;
        private readonly string _extension;
        private readonly string _HKCRSubKeyName;
        private readonly object _oldKeyValueOrNull;
        private readonly string _sourceFilterGuidName;

        /// <summary>
        /// Construct, setting the extension, and the source file we want to use for this extension.
        /// </summary>
        /// <param name="sourceFilterGuid"></param>
        /// <param name="extension"></param>
        public RegisteredMediaExtension(Guid sourceFilterGuid, string extension)
        {
            _sourceFilterGuidName = "{" + sourceFilterGuid + "}";
            _extension = extension;
            _HKCRSubKeyName = DirectShowExtensionTableHKCRSubKey + _extension;

            RegistryKey key = Registry.ClassesRoot.OpenSubKey(_HKCRSubKeyName);

            if (key != null)
            {
                _oldKeyValueOrNull = key.GetValue(DirectShowSourceFilterValueName);

                // in case of concurrent registrations... 
                if (_sourceFilterGuidName == (string) _oldKeyValueOrNull)
                {
                    _dontDispose = true;
                    return;
                }
            }

            Registry.ClassesRoot.CreateSubKey(_HKCRSubKeyName).SetValue(DirectShowSourceFilterValueName,
                                                                        _sourceFilterGuidName);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~RegisteredMediaExtension()
        {
            Dispose(false);
        }

        /// <summary>
        /// clean up the registry
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_dontDispose) return;

            RegistryKey key = Registry.ClassesRoot.OpenSubKey(_HKCRSubKeyName, true);

            if (key != null)
            {
                if (_oldKeyValueOrNull != null)
                {
                    key.SetValue(DirectShowSourceFilterValueName, _oldKeyValueOrNull);
                }
                else
                {
                    key.DeleteValue(DirectShowSourceFilterValueName, false);
                }
            }
        }
    }
}