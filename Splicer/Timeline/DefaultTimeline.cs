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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public class DefaultTimeline : ITimeline
    {
        public const double DefaultFPS = 30.0;

        private double _fps;
        private IAMTimeline _timeline;
        private AddOnlyList<IGroup> _groups = new AddOnlyList<IGroup>();
        private List<IMediaFileAssistant> _assistants = new List<IMediaFileAssistant>();

        private event EventHandler _beforeGroupAdded;
        private event EventHandler<AfterGroupAddedEventArgs> _afterGroupAdded;
        private event EventHandler _beforeClipAdded;
        private event EventHandler<AfterClipAddedEventArgs> _afterClipAdded;
        private event EventHandler _beforeCompositionAdded;
        private event EventHandler<AfterCompositionAddedEventArgs> _afterCompositionAdded;
        private event EventHandler _beforeTrackAdded;
        private event EventHandler<AfterTrackAddedEventArgs> _afterTrackAdded;
        private event EventHandler _beforeTransitionAdded;
        private event EventHandler<AfterTransitionAddedEventArgs> _afterTransitionAdded;
        private event EventHandler _beforeEffectAdded;
        private event EventHandler<AfterEffectAddedEventArgs> _afterEffectAdded;

        public DefaultTimeline()
            : this(DefaultFPS)
        {
        }

        public DefaultTimeline(double fps)
        {
            _fps = fps;

            // Create the timeline
            _timeline = (IAMTimeline) new AMTimeline();

            // Set the frames per second
            int hr = _timeline.SetDefaultFPS(fps);
            DESError.ThrowExceptionForHR(hr);
        }

        public void InstallAssistant(IMediaFileAssistant assistant)
        {
            if (!_assistants.Contains(assistant))
            {
                _assistants.Add(assistant);
            }
        }

        public IEnumerable<IMediaFileAssistant> Assitants
        {
            get { return _assistants; }
        }


        public IAMTimeline DesTimeline
        {
            get { return _timeline; }
        }

        public event EventHandler BeforeClipAdded
        {
            add { _beforeClipAdded += value; }
            remove { _beforeClipAdded -= value; }
        }

        public event EventHandler<AfterClipAddedEventArgs> AfterClipAdded
        {
            add { _afterClipAdded += value; }
            remove { _afterClipAdded -= value; }
        }

        public event EventHandler BeforeCompositionAdded
        {
            add { _beforeCompositionAdded += value; }
            remove { _beforeCompositionAdded -= value; }
        }

        public event EventHandler<AfterCompositionAddedEventArgs> AfterCompositionAdded
        {
            add { _afterCompositionAdded += value; }
            remove { _afterCompositionAdded -= value; }
        }

        public event EventHandler BeforeTrackAdded
        {
            add { _beforeTrackAdded += value; }
            remove { _beforeTrackAdded -= value; }
        }

        public event EventHandler<AfterTrackAddedEventArgs> AfterTrackAdded
        {
            add { _afterTrackAdded += value; }
            remove { _afterTrackAdded -= value; }
        }

        public event EventHandler BeforeTransitionAdded
        {
            add { _beforeTransitionAdded += value; }
            remove { _beforeTransitionAdded -= value; }
        }

        public event EventHandler<AfterTransitionAddedEventArgs> AfterTransitionAdded
        {
            add { _afterTransitionAdded += value; }
            remove { _afterTransitionAdded -= value; }
        }

        public event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        public event EventHandler BeforeEffectAdded
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        public double FPS
        {
            get { return _fps; }
        }

        public event EventHandler BeforeGroupAdded
        {
            add { _beforeGroupAdded += value; }
            remove { _beforeGroupAdded -= value; }
        }

        public event EventHandler<AfterGroupAddedEventArgs> AfterGroupAdded
        {
            add { _afterGroupAdded += value; }
            remove { _afterGroupAdded -= value; }
        }

        public AddOnlyList<IGroup> Groups
        {
            get { return _groups; }
        }

        public IGroup AddAudioGroup(string name, double fps)
        {
            OnBeforeGroupAdded();

            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetAudioMediaType();

                IGroup group = new Group(this, GroupType.Audio, mediaType, name, fps);

                _groups.Add(group);

                AttachHandlers(group);

                OnAfterGroupAdded(group);

                return group;
            }
            finally
            {
                if (mediaType != null) DsUtils.FreeAMMediaType(mediaType);
            }
        }

        public IGroup AddVideoGroup(string name, double fps, short bitCount, int width, int height)
        {
            OnBeforeGroupAdded();

            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeUtils.GetVideoMediaType(bitCount, width, height);

                IGroup group = new Group(this, GroupType.Video, mediaType, name, fps);

                _groups.Add(group);

                AttachHandlers(group);

                OnAfterGroupAdded(group);

                return group;
            }
            finally
            {
                if (mediaType != null) DsUtils.FreeAMMediaType(mediaType);
            }
        }

        private void AttachHandlers(IGroup group)
        {
            group.AfterCompositionAdded += new EventHandler<AfterCompositionAddedEventArgs>(group_AfterCompositionAdded);
            group.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(group_AfterEffectAdded);
            group.AfterTrackAdded += new EventHandler<AfterTrackAddedEventArgs>(group_AfterTrackAdded);
            group.AfterTransitionAdded += new EventHandler<AfterTransitionAddedEventArgs>(group_AfterTransitionAdded);
            group.BeforeCompositionAdded += new EventHandler(group_BeforeCompositionAdded);
            group.BeforeEffectAdded += new EventHandler(group_BeforeEffectAdded);
            group.BeforeTrackAdded += new EventHandler(group_BeforeTrackAdded);
            group.BeforeTransitionAdded += new EventHandler(group_BeforeTransitionAdded);
            group.BeforeClipAdded += new EventHandler(group_BeforeClipAdded);
            group.AfterClipAdded += new EventHandler<AfterClipAddedEventArgs>(group_AfterClipAdded);
        }

        private void group_AfterClipAdded(object sender, AfterClipAddedEventArgs e)
        {
            if (_afterClipAdded != null)
            {
                _afterClipAdded(sender, e);
            }
        }

        private void group_BeforeClipAdded(object sender, EventArgs e)
        {
            if (_beforeClipAdded != null)
            {
                _beforeClipAdded(sender, e);
            }
        }

        private void group_BeforeTransitionAdded(object sender, EventArgs e)
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(sender, e);
            }
        }

        private void group_BeforeTrackAdded(object sender, EventArgs e)
        {
            if (_beforeTrackAdded != null)
            {
                _beforeTrackAdded(sender, e);
            }
        }

        private void group_BeforeEffectAdded(object sender, EventArgs e)
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(sender, e);
            }
        }

        private void group_BeforeCompositionAdded(object sender, EventArgs e)
        {
            if (_beforeCompositionAdded != null)
            {
                _beforeCompositionAdded(sender, e);
            }
        }

        private void group_AfterTransitionAdded(object sender, AfterTransitionAddedEventArgs e)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(sender, e);
            }
        }

        private void group_AfterTrackAdded(object sender, AfterTrackAddedEventArgs e)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(sender, e);
            }
        }

        private void group_AfterEffectAdded(object sender, AfterEffectAddedEventArgs e)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(sender, e);
            }
        }

        private void group_AfterCompositionAdded(object sender, AfterCompositionAddedEventArgs e)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(sender, e);
            }
        }

        public IGroup FindFirstGroupOfType(GroupType type)
        {
            foreach (IGroup group in _groups)
            {
                if (group.Type == type) return group;
            }

            return null;
        }

        public double Duration
        {
            get { return TimelineUtils.ToSeconds(LengthInUnits); }
        }

        public long LengthInUnits
        {
            get
            {
                long len;
                int hr = _timeline.GetDuration(out len);
                DESError.ThrowExceptionForHR(hr);

                return len;
            }
        }

        protected void OnBeforeGroupAdded()
        {
            if (_beforeGroupAdded != null) _beforeGroupAdded(this, EventArgs.Empty);
        }

        protected void OnAfterGroupAdded(IGroup group)
        {
            if (_afterGroupAdded != null) _afterGroupAdded(this, new AfterGroupAddedEventArgs(group, this));
        }

        public IGroup AddAudioGroup(string name)
        {
            return AddAudioGroup(name, FPS);
        }

        public IGroup AddVideoGroup(string name, short bitCount, int width, int height)
        {
            return AddVideoGroup(name, FPS, bitCount, width, height);
        }

        public IGroup AddAudioGroup()
        {
            return AddAudioGroup(null);
        }

        public IGroup AddVideoGroup(short bitCount, int width, int height)
        {
            return AddVideoGroup(null, FPS, bitCount, width, height);
        }

        public IClip AddClip(string name, string fileName, GroupMediaType groupMediaType, InsertPosition position,
                             double offset, double clipStart, double clipEnd)
        {
            IGroup group = null;

            if (groupMediaType == GroupMediaType.Audio)
            {
                group = FindFirstGroupOfType(GroupType.Audio);
            }
            else
            {
                group = FindFirstGroupOfType(GroupType.Video);
            }

            if (group == null)
            {
                throw new SplicerException(
                    string.Format("No group found supporting a clip of type \"{0}\"", groupMediaType));
            }

            if (group.Tracks.Count > 0)
            {
                return group.Tracks[0].AddClip(name, fileName, groupMediaType, position, offset, clipStart, clipEnd);
            }
            else
            {
                throw new SplicerException(
                    string.Format("No tracks found in the first group of type \"{0}\"", group.Type));
            }
        }

        public IClip AddClip(string fileName, GroupMediaType groupMediaType, InsertPosition position, double offset,
                             double clipStart, double clipEnd)
        {
            return AddClip(null, fileName, groupMediaType, position, offset, clipStart, clipEnd);
        }

        public IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Video, position, offset, clipStart, clipEnd);
        }

        public IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, position, offset, clipStart, clipEnd);
        }

        public IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        public IClip AddVideo(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, 0, clipEnd);
        }

        public IClip AddVideo(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, 0, -1);
        }

        public IClip AddVideo(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, 0, 0, -1);
        }

        public IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Image, position, offset, clipStart, clipEnd);
        }

        public IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, position, offset, clipStart, clipEnd);
        }

        public IClip AddImage(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        public IClip AddImage(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, 0, clipEnd);
        }

        public IClip AddImage(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, 0, -1);
        }

        public IClip AddImage(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, 0, 0, -1);
        }

        public IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Audio, position, offset, clipStart, clipEnd);
        }

        public IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, position, offset, clipStart, clipEnd);
        }

        public IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        public IClip AddAudio(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, 0, clipEnd);
        }

        public IClip AddAudio(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, 0, -1);
        }

        public IClip AddAudio(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, 0, 0, -1);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string name, string fileName, InsertPosition position,
                                                     double offset, double clipStart, double clipEnd)
        {
            IClip audioClip = AddAudio(name, fileName, position, offset, clipStart, clipEnd);
            IClip videoClip = AddVideo(name, fileName, position, offset, clipStart, clipEnd);

            return new AudioVideoClipPair(audioClip, videoClip);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, InsertPosition position, double offset,
                                                     double clipStart, double clipEnd)
        {
            return AddVideoWithAudio(null, fileName, position, offset, clipStart, clipEnd);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipEnd)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, 0, clipEnd);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, 0, -1);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, 0, 0, -1);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_timeline != null)
            {
                Marshal.ReleaseComObject(_timeline);
                _timeline = null;
            }

            if (_groups != null)
            {
                foreach (IGroup group in _groups)
                {
                    group.Dispose();
                }

                _groups = null;
            }
        }

        #endregion        
    }
}