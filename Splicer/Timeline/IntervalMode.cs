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

namespace Splicer.Timeline
{
    /// <summary>
    /// Interval mode, defines the type of interval for this parameter - this is equivalent to
    /// Dexterf in the DirectShowLib (though we avoid using that, so there isn't any required for
    /// DirectShowLib when dealing with serialized configurations).
    /// </summary>
    public enum IntervalMode
    {
        /// <summary>
        /// Comes out as a "linear" tag from the DES XML
        /// </summary>
        Interpolate,

        /// <summary>
        /// Comes out as an "at" tag from the DES XML
        /// </summary>
        Jump
    }
}