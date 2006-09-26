using System;
using DirectShowLib;

namespace Splicer.Renderer
{
    /// <summary>Used by the <see cref="DirectShowEditor.Completed"/> event.  
    /// Reports the event code that exited the graph.
    /// </summary>
    /// <remarks>Signals that all files have been rendered</remarks>
    public class CompletedArgs : EventArgs
    {
        /// <summary>The result of the rendering</summary>
        /// <remarks>
        /// This code will be a member of DirectShowLib.EventCode.  Typically it 
        /// will be EventCode.Complete, EventCode.ErrorAbort or EventCode.UserAbort.
        /// </remarks>
        public EventCode Result;

        /// <summary>
        /// Used to construct an instace of the class.
        /// </summary>
        /// <param name="ec"></param>
        internal CompletedArgs(EventCode ec)
        {
            Result = ec;
        }
    }
}