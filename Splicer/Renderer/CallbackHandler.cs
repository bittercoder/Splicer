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
using System.Runtime.InteropServices;
using DirectShowLib;
using Splicer.Timeline;

namespace Splicer.Renderer
{
    /// <summary>
    /// Class used by both audio and video callback
    /// </summary>
    public class CallbackHandler : ISampleGrabberCB
    {
        #region Data members

        /// <summary>
        /// Client callback routine
        /// </summary>
        protected IDESCombineCB m_pCallback;

        // The list of files
        protected IGroup m_Group;

        // The event sink (used to notify on end of file)
        protected IMediaEventSink m_pEventSink;

        // The event code to be used for end of file
        protected EventCode m_ec;

        // Holds the index into m_Files we are currently processing
        protected int m_iCurFile;

        // Which frame number we are currently processing
        protected int m_iCurFrame;

        // Maximum frame number for the current file
        protected int m_iMaxFrame;

        // File name of the currently processing file
        protected string m_CurFileName;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGroup">Timeline group info</param>
        /// <param name="pCallback">Client callback</param>
        /// <param name="pEventSink">Event sync to call on file complete</param>
        /// <param name="ec">Event code to send on file completion</param>
        public CallbackHandler(
            IGroup pGroup,
            IDESCombineCB pCallback,
            IMediaEventSink pEventSink,
            EventCode ec
            )
        {
            m_pCallback = pCallback;
            m_Group = pGroup;
            m_pEventSink = pEventSink;
            m_ec = ec;

            m_iCurFrame = 0;
            m_iCurFile = 0;
            // TODO: fix this or chuck it
            MediaFile mf = null; // m_Group.File(m_iCurFile);
            if (mf != null)
            {
                m_CurFileName = mf.FileName;
                m_iMaxFrame = mf.LengthInFrames;
            }
            else
            {
                m_CurFileName = null;
                m_iMaxFrame = int.MaxValue;
            }
        }


        // ISampleGrabberCB methods
        public int SampleCB(double SampleTime, IMediaSample pSample)
        {
            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        public int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            // Call the client
            int iRet;

            if (m_pCallback != null)
            {
                iRet = m_pCallback.BufferCB(m_CurFileName, SampleTime, pBuffer, BufferLen);
            }
            else
            {
                iRet = 0;
            }

            m_iCurFrame++;

            // Have we finished the current file?
            if (m_iCurFrame >= m_iMaxFrame)
            {
                // Send the notification
                int hr = m_pEventSink.Notify(m_ec, m_iCurFile, m_iCurFrame);

                // Find the next file
                m_iCurFile++;

                // TODO: get this working again
                //int count = m_Group.Count;
                int count = 0;

                if (m_iCurFile < count)
                {
                    // TODO: get this working again
                    //MediaFile mf = m_Group.File(m_iCurFile);
                    MediaFile mf = null;
                    m_CurFileName = mf.FileName;
                    m_iMaxFrame += mf.LengthInFrames;
                }
                else
                {
                    // A failsafe
                    m_iMaxFrame = int.MaxValue;
                }
            }

            return iRet;
        }
    }
}