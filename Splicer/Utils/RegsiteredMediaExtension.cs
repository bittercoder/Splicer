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
using Microsoft.Win32;

namespace Splicer.Utils
{
    /// <summary>
    /// This class can be used to override an existing extensions associated source filter, upon
    /// disposal it will restore the previous setting.  It's recomended that it be used for only
    /// very short durations ie. when adding media, so as to reduce the chance of a user concurrently
    /// installing a new filter.
    /// </summary>
    public class RegsiteredMediaExtension : IDisposable
    {
        private string _extension;
        private string _HKCRSubKeyName;
        private object _oldKeyValueOrNull;
        private string _sourceFilterGuidName;
        private bool _dontDispose;

        private const string DirectShowExtensionTableHKCRSubKey = @"Media Type\Extensions\";
        private const string DirectShowSourceFilterValueName = "Source Filter";

        /// <summary>
        /// Construct, setting the extension, and the source file we want to use for this extension.
        /// </summary>
        /// <param name="sourceFilterGuid"></param>
        /// <param name="extension"></param>
        public RegsiteredMediaExtension(Guid sourceFilterGuid, string extension)
        {
            _sourceFilterGuidName = "{" + sourceFilterGuid.ToString() + "}";
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
            else
            {
                _oldKeyValueOrNull = null;
            }

            Registry.ClassesRoot.CreateSubKey(_HKCRSubKeyName).SetValue(DirectShowSourceFilterValueName,
                                                                        _sourceFilterGuidName);
        }

        /// <summary>
        /// clean up the registry
        /// </summary>
        public void Dispose()
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