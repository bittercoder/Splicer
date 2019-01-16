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
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Properties;

namespace Splicer.Timeline
{
    public enum GroupMediaType
    {
        Audio,
        Image,
        Video
    }

    public enum InsertPosition
    {
        Absolute,
        Relative
    }

    /// <summary>
    /// Class containing information about the timeline groups (one
    /// for audio, one for video)
    /// </summary>
    public sealed class Group : AbstractComposition, IGroup, IDisposable
    {
        private readonly double _fps;
        private readonly ITimeline _timeline;
        private readonly GroupType _type;
        private IAMTimelineGroup _group;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of group this is</param>
        /// <param name="mediaType">Media type of the new group</param>
        /// <param name="timeline">Timeline to use for the group</param>
        /// <param name="fps">Fps for the group</param>
        public Group(ITimeline timeline, GroupType type, AMMediaType mediaType, string name, double fps)
            : base(timeline, name, -1)
        {
            if (timeline == null) throw new ArgumentNullException("timeline");
            if (mediaType == null) throw new ArgumentNullException("mediaType");
            if (fps <= 0) throw new SplicerException(Resources.ErrorFramesPerSecondMustBeGreaterThenZero);

            _timeline = timeline;
            _type = type;
            _fps = fps;

            _group = TimelineBuilder.InsertGroup(_timeline.DesTimeline, mediaType, name);
            TimelineComposition = (IAMTimelineComp) _group;
        }

        #region IGroup Members

        public double Fps
        {
            get { return _fps; }
        }

        public ITimeline Timeline
        {
            get { return _timeline; }
        }

        public GroupType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Release everything
        /// </summary>        
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        IGroup IBelongsToGroup.Group
        {
            get { return this; }
        }

        public override ICompositionContainer Container
        {
            get { throw new SplicerException(Resources.ErrorGroupsDontSupportContainerProperty); }
        }

        #endregion

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            DisposeComposition(disposing);

            if (_group != null)
            {
                Marshal.ReleaseComObject(_group);
                _group = null;

                TimelineComposition = null;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~Group()
        {
            Dispose(false);
        }
    }
}