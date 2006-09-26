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
using DirectShowLib.DES;

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
        Absoloute,
        Relative
    }

    /// <summary>
    /// Class containing information about the timeline groups (one
    /// for audio, one for video)
    /// </summary>
    public class Group : AbstractComposition, IGroup, IDisposable
    {
        private GroupType _type;
        private double _fps;
        protected IAMTimelineGroup _group;
        private ITimeline _timeline;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type of group this is</param>
        /// <param name="mediaType">Media type of the new group</param>
        /// <param name="timeline">Timeline to use for the group</param>
        /// <param name="fps">FPS for the group</param>
        public Group(ITimeline timeline, GroupType type, AMMediaType mediaType, string name, double fps)
            : base(timeline.DesTimeline, name, -1)
        {
            _timeline = timeline;
            _type = type;
            _fps = fps;

            _group = TimelineUtils.InsertGroup(_timeline.DesTimeline, mediaType, name);
            _timelineComposition = (IAMTimelineComp) _group;
        }

        public double FPS
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
        public override void Dispose()
        {
            base.Dispose();

            if (_group != null)
            {
                Marshal.ReleaseComObject(_group);
                _group = null;
            }

            GC.SuppressFinalize(this);
        }

        IGroup IBelongsToGroup.Group
        {
            get { return this; }
        }

        public override ICompositionContainer Container
        {
            get { throw new SplicerException("Groups are top level timeline components and do not support this property"); }
        }
    }
}