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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Properties;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    public sealed class DefaultTimeline : ITimeline
    {
        public const double DefaultFps = 30.0;
        private readonly List<IMediaFileAssistant> _assistants = new List<IMediaFileAssistant>();

        private readonly double _fps;
        private AddOnlyCollection<IGroup> _groups = new AddOnlyCollection<IGroup>();
        private IAMTimeline _timeline;

        public DefaultTimeline()
            : this(DefaultFps)
        {
        }

        public DefaultTimeline(double fps)
            : this(fps, Path.GetTempPath())
        {
        }

        public DefaultTimeline(double fps, string temporaryStoragePath)
        {
            if (string.IsNullOrEmpty(temporaryStoragePath)) throw new ArgumentNullException("temporaryStoragePath");

            if (fps <= 0) throw new SplicerException(Resources.ErrorFramesPerSecondMustBeGreaterThenZero);

            TemporaryStoragePath = temporaryStoragePath;

            _fps = fps;

            // Create the timeline
            _timeline = (IAMTimeline) new AMTimeline();

            // Set the frames per second
            int hr = _timeline.SetDefaultFPS(fps);
            DESError.ThrowExceptionForHR(hr);
        }

        #region ITimeline Members

        public string TemporaryStoragePath { get; set; }

        public void InstallAssistant(IMediaFileAssistant assistant)
        {
            if (!_assistants.Contains(assistant))
            {
                _assistants.Add(assistant);
            }
        }

        public IEnumerable<IMediaFileAssistant> Assistants
        {
            get { return _assistants; }
        }


        public IAMTimeline DesTimeline
        {
            get { return _timeline; }
        }

        public event EventHandler AddingClip
        {
            add { _beforeClipAdded += value; }
            remove { _beforeClipAdded -= value; }
        }

        public event EventHandler<AddedClipEventArgs> AddedClip
        {
            add { _afterClipAdded += value; }
            remove { _afterClipAdded -= value; }
        }

        public event EventHandler AddingComposition
        {
            add { _beforeCompositionAdded += value; }
            remove { _beforeCompositionAdded -= value; }
        }

        public event EventHandler<AddedCompositionEventArgs> AddedComposition
        {
            add { _afterCompositionAdded += value; }
            remove { _afterCompositionAdded -= value; }
        }

        public event EventHandler AddingTrack
        {
            add { _beforeTrackAdded += value; }
            remove { _beforeTrackAdded -= value; }
        }

        public event EventHandler<AddedTrackEventArgs> AddedTrack
        {
            add { _afterTrackAdded += value; }
            remove { _afterTrackAdded -= value; }
        }

        public event EventHandler AddingTransition
        {
            add { _beforeTransitionAdded += value; }
            remove { _beforeTransitionAdded -= value; }
        }

        public event EventHandler<AddedTransitionEventArgs> AddedTransition
        {
            add { _afterTransitionAdded += value; }
            remove { _afterTransitionAdded -= value; }
        }

        public event EventHandler<AddedEffectEventArgs> AddedEffect
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        public event EventHandler AddingEffect
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        public double Fps
        {
            get { return _fps; }
        }

        public event EventHandler AddingGroup
        {
            add { _beforeGroupAdded += value; }
            remove { _beforeGroupAdded -= value; }
        }

        public event EventHandler<AddedGroupEventArgs> AddedGroup
        {
            add { _afterGroupAdded += value; }
            remove { _afterGroupAdded -= value; }
        }

        public AddOnlyCollection<IGroup> Groups
        {
            get { return _groups; }
        }

        public IGroup AddAudioGroup(string name, double fps)
        {
            OnAddingGroup();

            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeTools.GetAudioMediaType();

                IGroup group = new Group(this, GroupType.Audio, mediaType, name, fps);

                _groups.Add(group);

                AttachHandlers(group);

                OnAddedGroup(group);

                return group;
            }
            finally
            {
                if (mediaType != null) DsUtils.FreeAMMediaType(mediaType);
            }
        }

        public IGroup AddVideoGroup(string name, double fps, short bitCount, int width, int height)
        {
            OnAddingGroup();

            AMMediaType mediaType = null;

            try
            {
                mediaType = MediaTypeTools.GetVideoMediaType(bitCount, width, height);

                IGroup group = new Group(this, GroupType.Video, mediaType, name, fps);

                _groups.Add(group);

                AttachHandlers(group);

                OnAddedGroup(group);

                return group;
            }
            finally
            {
                if (mediaType != null) DsUtils.FreeAMMediaType(mediaType);
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
            get { return TimelineBuilder.ToSeconds(LengthInUnits); }
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

        public IGroup AddAudioGroup(string name)
        {
            return AddAudioGroup(name, Fps);
        }

        public IGroup AddVideoGroup(string name, short bitCount, int width, int height)
        {
            return AddVideoGroup(name, Fps, bitCount, width, height);
        }

        public IGroup AddAudioGroup()
        {
            return AddAudioGroup(null);
        }

        public IGroup AddVideoGroup(short bitCount, int width, int height)
        {
            return AddVideoGroup(null, Fps, bitCount, width, height);
        }

        public IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                             double offset, double clipStart, double clipEnd)
        {
            return AddClip(name, fileName, mediaType, position, offset, clipStart, clipEnd, false);
        }

        public IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                             double clipStart, double clipEnd)
        {
            return AddClip(null, fileName, mediaType, position, offset, clipStart, clipEnd);
        }

        public IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                             double offset,
                             double clipStart, double clipEnd, bool manageLifespan)
        {
            IGroup group = null;

            if (mediaType == GroupMediaType.Audio)
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
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorNoGroupForSupportingClipOfType, mediaType));
            }

            if (group.Tracks.Count > 0)
            {
                return group.Tracks[0].AddClip(name, fileName, mediaType, position, offset, clipStart, clipEnd,
                                               manageLifespan);
            }
            else
            {
                throw new SplicerException(
                    string.Format(CultureInfo.InvariantCulture, Resources.ErrorNoTracksFoundInFirstGroupOfType,
                                  group.Type));
            }
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
            return AddVideoWithAudio(name, fileName, position, offset, clipStart, clipEnd, false);
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

        public IAudioVideoClipPair AddVideoWithAudio(string name, string fileName, InsertPosition position,
                                                     double offset, double clipStart, double clipEnd,
                                                     bool shadowCopyAudio)
        {
            string audioFile = null;

            if (shadowCopyAudio)
            {
                audioFile = Path.Combine(TemporaryStoragePath,
                                         string.Format(CultureInfo.InvariantCulture,
                                                       Resources.TemporaryAudioFilenameTemplate, Guid.NewGuid(),
                                                       Path.GetExtension(fileName)));
                File.Copy(fileName, audioFile);
            }
            else
            {
                audioFile = fileName;
            }

            IClip audioClip = AddClip(name, audioFile, GroupMediaType.Audio, position, offset, clipStart, clipEnd,
                                      shadowCopyAudio);
            IClip videoClip = AddVideo(name, fileName, position, offset, clipStart, clipEnd);

            return new AudioVideoClipPair(audioClip, videoClip);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, InsertPosition position, double offset,
                                                     double clipStart, double clipEnd, bool shadowCopyAudio)
        {
            return AddVideoWithAudio(null, fileName, position, offset, clipStart, clipEnd, shadowCopyAudio);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipStart, double clipEnd,
                                                     bool shadowCopyAudio)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, clipStart, clipEnd,
                                     shadowCopyAudio);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipEnd,
                                                     bool shadowCopyAudio)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, 0, clipEnd, shadowCopyAudio);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, bool shadowCopyAudio)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, offset, 0, -1, shadowCopyAudio);
        }

        public IAudioVideoClipPair AddVideoWithAudio(string fileName, bool shadowCopyAudio)
        {
            return AddVideoWithAudio(null, fileName, InsertPosition.Relative, 0, 0, -1, shadowCopyAudio);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private event EventHandler _beforeGroupAdded;
        private event EventHandler<AddedGroupEventArgs> _afterGroupAdded;
        private event EventHandler _beforeClipAdded;
        private event EventHandler<AddedClipEventArgs> _afterClipAdded;
        private event EventHandler _beforeCompositionAdded;
        private event EventHandler<AddedCompositionEventArgs> _afterCompositionAdded;
        private event EventHandler _beforeTrackAdded;
        private event EventHandler<AddedTrackEventArgs> _afterTrackAdded;
        private event EventHandler _beforeTransitionAdded;
        private event EventHandler<AddedTransitionEventArgs> _afterTransitionAdded;
        private event EventHandler _beforeEffectAdded;
        private event EventHandler<AddedEffectEventArgs> _afterEffectAdded;

        private void AttachHandlers(IGroup group)
        {
            group.AddedComposition += group_AfterCompositionAdded;
            group.AddedEffect += group_AfterEffectAdded;
            group.AddedTrack += group_AfterTrackAdded;
            group.AddedTransition += group_AfterTransitionAdded;
            group.AddingComposition += group_BeforeCompositionAdded;
            group.AddingEffect += group_BeforeEffectAdded;
            group.AddingTrack += group_BeforeTrackAdded;
            group.AddingTransition += group_BeforeTransitionAdded;
            group.AddingClip += group_BeforeClipAdded;
            group.AddedClip += group_AfterClipAdded;
        }

        private void group_AfterClipAdded(object sender, AddedClipEventArgs e)
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

        private void group_AfterTransitionAdded(object sender, AddedTransitionEventArgs e)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(sender, e);
            }
        }

        private void group_AfterTrackAdded(object sender, AddedTrackEventArgs e)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(sender, e);
            }
        }

        private void group_AfterEffectAdded(object sender, AddedEffectEventArgs e)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(sender, e);
            }
        }

        private void group_AfterCompositionAdded(object sender, AddedCompositionEventArgs e)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(sender, e);
            }
        }

        private void OnAddingGroup()
        {
            if (_beforeGroupAdded != null) _beforeGroupAdded(this, EventArgs.Empty);
        }

        private void OnAddedGroup(IGroup group)
        {
            if (_afterGroupAdded != null) _afterGroupAdded(this, new AddedGroupEventArgs(group, this));
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~DefaultTimeline()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_groups != null)
                {
                    foreach (IGroup group in _groups)
                    {
                        group.Dispose();
                    }

                    _groups = null;
                }
            }

            if (_timeline != null)
            {
                Marshal.ReleaseComObject(_timeline);
                _timeline = null;
            }
        }
    }
}