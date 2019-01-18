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
    public interface IRenderer
    {
        /// <summary>
        /// Gets the current state of the renderer
        /// </summary>
        RendererState State { get; }

        event EventHandler RenderCompleted;

        /// <summary>
        /// Returns an XML description of the capture graph (as seen by DES).
        /// </summary>
        /// <remarks>
        /// Might be useful for debugging, handy for implementing some unit tests (where you 
        /// want to make sure changes to implementation don't alter the expected DES capture graph).
        /// </remarks>
        /// <returns>String containing XML</returns>
        string ToXml();

        /// <summary>
        /// Saves the renderers underlying filter graph to a file (which can be loaded
        /// into graphedit for examination)
        /// </summary>
        /// <param name="fileName"></param>
        void SaveToGraphFile(string fileName);

        /// <summary>
        /// Begin rendering, and block until complete.
        /// </summary>
        void Render();

        /// <summary>
        /// Begin cancelling, and block until complete.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Begins rendering and returns imediately.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        IAsyncResult BeginRender(AsyncCallback callback, object state);

        /// <summary>
        /// Blocks until rendering is complete
        /// </summary>
        void EndRender(IAsyncResult result);

        /// <summary>
        /// Begin trying to cancel the render
        /// </summary>
        IAsyncResult BeginCancel(AsyncCallback callback, object state);

        /// <summary>
        /// Block until the cancel request has completed
        /// </summary>
        /// <param name="result"></param>
        void EndCancel(IAsyncResult result);
    }
}