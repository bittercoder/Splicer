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
    /// The different key types which can be used with the KeyTransition transition
    /// </summary>
    public enum KeyTransitionType
    {
        /// <summary>
        /// Chroma key
        /// </summary>
        Rgb = 0,
        /// <summary>
        /// Makes blue and green areas transparent
        /// </summary>
        NoRed = 1,
        /// <summary>
        /// Luminance
        /// </summary>        
        Luminance = 2,
        /// <summary>
        /// key by alpha
        /// </summary>
        Alpha = 3,
        /// <summary>
        /// key by hue
        /// </summary>
        Hue = 4
    }
}