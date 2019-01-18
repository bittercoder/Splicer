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

namespace Splicer.Renderer
{
    /// <summary>
    /// Event argument used to track rendering progress.
    /// </summary>
    public class RenderProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The current rendering position (in seconds) 
        /// </summary>
        public readonly double CurrentPosition;

        /// <summary>
        /// The total duration (in seconds) of the asset being rendered.
        /// </summary>
        public readonly double Duration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderProgressEventArgs"/> class.
        /// </summary>
        /// <param name="currentPosition">The current position.</param>
        /// <param name="duration">The duration.</param>
        public RenderProgressEventArgs(double currentPosition, double duration)
        {
            CurrentPosition = currentPosition;
            Duration = duration;
        }
    }
}