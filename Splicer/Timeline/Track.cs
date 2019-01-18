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
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using DirectShowLib.DES;
using Splicer.Properties;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    public sealed class Track : ITrack, IPrioritySetter
    {
        public const double DefaultImageDisplayDuration = 1.0;
        private readonly ITrackContainer _container;
        private readonly double _fps;
        private readonly string _name;
        private readonly IAMTimeline _timeline;
        private readonly VirtualClipCollection _virtualClips = new VirtualClipCollection();
        private AddOnlyCollection<IClip> _clips = new AddOnlyCollection<IClip>();
        private AddOnlyCollection<IEffect> _effects = new AddOnlyCollection<IEffect>();
        private long _length;
        private int _priority;
        private IAMTimelineTrack _track;
        private AddOnlyCollection<ITransition> _transitions = new AddOnlyCollection<ITransition>();

        public Track(ITrackContainer container, IAMTimeline timeline, IAMTimelineTrack track, string name, int priority)
        {
            _container = container;
            _timeline = timeline;
            _track = track;
            _name = name;
            _priority = priority;

            int hr = timeline.GetDefaultFPS(out _fps);
            DESError.ThrowExceptionForHR(hr);
        }

        #region IPrioritySetter Members

        void IPrioritySetter.SetPriority(int newValue)
        {
            _priority = newValue;
        }

        #endregion

        #region ITrack Members

        public IGroup Group
        {
            get { return Container.Group; }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Video, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, 0, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, offset, 0, -1);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddVideo(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Video, InsertPosition.Relative, 0, 0, -1);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Image, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, 0, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, offset, 0, -1);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddImage(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Image, InsertPosition.Relative, 0, 0, -1);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            return AddClip(name, fileName, GroupMediaType.Audio, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string fileName, double offset, double clipEnd)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, 0, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string fileName, double offset)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, offset, 0, -1);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddAudio(string fileName)
        {
            return AddClip(fileName, GroupMediaType.Audio, InsertPosition.Relative, 0, 0, -1);
        }

        public ITrackContainer Container
        {
            get { return _container; }
        }

        public AddOnlyCollection<IClip> Clips
        {
            get { return _clips; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public string Name
        {
            get { return _name; }
        }

        public IEffect AddEffect(double offset, double duration, EffectDefinition effectDefinition)
        {
            return AddEffect(null, -1, offset, duration, effectDefinition);
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            OnAddingEffect();

            IEffect effect =
                TimelineBuilder.AddEffectToCollection(this, _timeline, (IAMTimelineEffectable) _track, _effects, name,
                                                      priority,
                                                      offset, duration, effectDefinition);

            OnAddedEffect(effect);

            return effect;
        }

        public AddOnlyCollection<IEffect> Effects
        {
            get { return _effects; }
        }

        public ITransition AddTransition(double offset, double duration,
                                         TransitionDefinition transitionDefinition)
        {
            return AddTransition(null, offset, duration, transitionDefinition, false);
        }

        public ITransition AddTransition(double offset, double duration,
                                         TransitionDefinition transitionDefinition, bool swapInputs)
        {
            return AddTransition(null, offset, duration, transitionDefinition, swapInputs);
        }

        public ITransition AddTransition(string name, double offset, double duration,
                                         TransitionDefinition transitionDefinition, bool swapInputs)
        {
            OnAddingTransition();

            ITransition transition =
                TimelineBuilder.AddTransitionToCollection(this, _timeline, (IAMTimelineTransable) _track, _transitions,
                                                          name,
                                                          offset, duration, transitionDefinition, swapInputs);

            OnAddedTransition(transition);

            return transition;
        }

        public AddOnlyCollection<ITransition> Transitions
        {
            get { return _transitions; }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                             double clipStart, double clipEnd)
        {
            return AddClip(null, fileName, mediaType, position, offset, clipStart, clipEnd);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                             double offset,
                             double clipStart, double clipEnd)
        {
            return AddClip(name, fileName, mediaType, position, offset, clipStart, clipEnd, false);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                             double offset,
                             double clipStart, double clipEnd, bool manageLifespan)
        {
            CheckMediaTypeAgainstGroup(mediaType);

            OnAddingClip();

            if ((clipEnd < 0) && (mediaType == GroupMediaType.Image))
            {
                clipEnd = DefaultImageDisplayDuration + clipStart;
            }

            long durationInUnits;

            var mediaFile = new MediaFile(fileName, manageLifespan);

            double absoluteOffset = position == InsertPosition.Absolute ? offset : Duration + offset;

            IList<IDisposable> activeAssistants = null;

            IAMTimelineSrc timelineSrc = null;

            try
            {
                activeAssistants = FetchAssistants(mediaFile);

                timelineSrc = CreateMedia(mediaFile,
                                          mediaType,
                                          TimelineBuilder.ToUnits(absoluteOffset),
                                          TimelineBuilder.ToUnits(clipStart),
                                          TimelineBuilder.ToUnits(clipEnd),
                                          out durationInUnits);
            }
            finally
            {
                DisposeOfAssistants(activeAssistants);
            }

            if (!string.IsNullOrEmpty(name))
            {
                ((IAMTimelineObj) timelineSrc).SetUserName(name);
            }

            IClip clip =
                new Clip(this, _timeline, timelineSrc, name, absoluteOffset, TimelineBuilder.ToSeconds(durationInUnits),
                         clipStart, mediaFile);

            clip.AddingEffect += clip_BeforeEffectAdded;
            clip.AddedEffect += clip_AfterEffectAdded;

            _clips.Add(clip);

            _virtualClips.AddVirtualClip(clip);

            OnAddedClip(clip);

            return clip;
        }

        public IClip AddImage(string name, Image image, InsertPosition position, double offset, double clipStart,
                              double clipEnd)
        {
            if (image == null) throw new ArgumentNullException("image");

            string fileName = CreateFileFromImage(image);

            return AddClip(name, fileName, GroupMediaType.Image, position, offset, clipStart, clipEnd, true);
        }

        public IClip AddImage(Image image, InsertPosition position, double offset, double clipStart, double clipEnd)
        {
            return AddImage(null, image, position, offset, clipStart, clipEnd);
        }

        public IClip AddImage(Image image, double offset, double clipStart, double clipEnd)
        {
            return AddImage(null, image, InsertPosition.Relative, offset, clipStart, clipEnd);
        }

        public IClip AddImage(Image image, double offset, double clipEnd)
        {
            return AddImage(null, image, InsertPosition.Relative, offset, 0, clipEnd);
        }

        public IClip AddImage(Image image, double offset)
        {
            return AddImage(null, image, InsertPosition.Relative, offset, 0, -1);
        }

        public IClip AddImage(Image image)
        {
            return AddImage(null, image, InsertPosition.Relative, 0, 0, -1);
        }

        public double Duration
        {
            get { return TimelineBuilder.ToSeconds(_length); }
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

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IVirtualClipCollection VirtualClips
        {
            get { return _virtualClips; }
        }

        #endregion

        private event EventHandler<AddedEffectEventArgs> _afterEffectAdded;
        private event EventHandler _beforeEffectAdded;
        private event EventHandler _beforeTransitionAdded;
        private event EventHandler<AddedTransitionEventArgs> _afterTransitionAdded;
        private event EventHandler _beforeClipAdded;
        private event EventHandler<AddedClipEventArgs> _afterClipAdded;

        private void CheckMediaTypeAgainstGroup(GroupMediaType mediaType)
        {
            if (mediaType == GroupMediaType.Audio)
            {
                if (Group.Type != GroupType.Audio)
                {
                    throw new SplicerException(Resources.ErrorCantAddAudioClipsToVideoGroup);
                }
            }
            else
            {
                if (Group.Type != GroupType.Video)
                {
                    throw new SplicerException(Resources.ErrorCantAddVideoClipsToAudioGroup);
                }
            }
        }

        private IList<IDisposable> FetchAssistants(MediaFile file)
        {
            var activeAssistants = new List<IDisposable>();

            foreach (IMediaFileAssistant assistant in Group.Timeline.Assistants)
            {
                if (assistant.WillAssist(file))
                {
                    activeAssistants.Add(assistant.Assist(file));
                }
            }

            return activeAssistants;
        }

        private static void DisposeOfAssistants(IList<IDisposable> activeAssistants)
        {
            if (activeAssistants != null)
                foreach (IDisposable disposable in activeAssistants)
                {
                    disposable.Dispose();
                }
        }

        private void clip_AfterEffectAdded(object sender, AddedEffectEventArgs e)
        {
            // bubble the event up
            OnAddedEffect(e.Item);
        }

        private void clip_BeforeEffectAdded(object sender, EventArgs e)
        {
            // bubble the event up
            OnAddingEffect();
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private IAMTimelineSrc CreateMedia(MediaFile mediaFile, GroupMediaType mediaType, long offset, long start,
                                           long end, out long duration)
        {
            int hr;
            IAMTimelineObj sourceObj;
            IAMTimelineSrc source;

            // If the endpoint is -1, find the real file length
            if (end < 0)
            {
                end = mediaFile.LengthInUnits;
            }

            duration = (end - start);

            // create the timeline source object

            hr = _timeline.CreateEmptyNode(out sourceObj, TimelineMajorType.Source);

            DESError.ThrowExceptionForHR(hr);

            long absolouteStart = offset;
            long absolouteEnd = absolouteStart + duration;

            // set up source length
            hr = sourceObj.SetStartStop(absolouteStart, absolouteEnd);
            DESError.ThrowExceptionForHR(hr);

            try
            {
                source = (IAMTimelineSrc) sourceObj;

                // Set the file name
                hr = source.SetMediaName(mediaFile.FileName);
                DESError.ThrowExceptionForHR(hr);

                // Set the start/end - we fix the Fps at 0 for static media (images)
                if (mediaType == GroupMediaType.Image)
                {
                    hr = source.SetDefaultFPS(0.0);
                    DESError.ThrowExceptionForHR(hr);
                }
                else
                {
                    hr = source.SetMediaTimes(start, end);
                    DESError.ThrowExceptionForHR(hr);
                }

                // set the stretch mode for non-audio media
                if (mediaType != GroupMediaType.Audio)
                {
                    hr = source.SetStretchMode(ResizeFlags.Stretch);
                    DESError.ThrowExceptionForHR(hr);
                }

                // Connect the track to the source
                hr = _track.SrcAdd(sourceObj);
                DESError.ThrowExceptionForHR(hr);

                // Set the times, get back the times adjusted to fit the frame rate
                hr = source.FixMediaTimes(ref start, ref end);
                DESError.ThrowExceptionForHR(hr);

                // Calculate the last frame number for the file
                double d1 = (end - start);
                double d2 = (TimelineBuilder.Units/_fps);
                double d3 = d1/d2;
                var d4 = (int) Math.Round(d3);

                // Update the MediaFile (used to see when we've walked past
                // the end of a file)
                mediaFile.LengthInFrames = d4;

                if (absolouteEnd > _length)
                {
                    _length = absolouteEnd;
                }

                return source;
            }
            catch
            {
                if (sourceObj != null)
                {
                    Marshal.ReleaseComObject(sourceObj);
                }

                throw;
            }
        }

        private string CreateFileFromImage(Image image)
        {
            string tempPath = Group.Timeline.TemporaryStoragePath;

            string fileName = Path.Combine(tempPath,
                                           string.Format(CultureInfo.InvariantCulture,
                                                         Resources.TemporaryImageFilenameTemplate, Guid.NewGuid()));

            image.Save(fileName, ImageFormat.Bmp);

            return fileName;
        }

        private void OnAddedEffect(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AddedEffectEventArgs(effect, this));
            }
        }

        private void OnAddingEffect()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        private void OnAddingTransition()
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(this, EventArgs.Empty);
            }
        }

        private void OnAddedTransition(ITransition transition)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(this, new AddedTransitionEventArgs(transition, this));
            }
        }

        private void OnAddingClip()
        {
            if (_beforeClipAdded != null)
            {
                _beforeClipAdded(this, EventArgs.Empty);
            }
        }

        private void OnAddedClip(IClip clip)
        {
            if (_afterClipAdded != null)
            {
                _afterClipAdded(this, new AddedClipEventArgs(clip, this));
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        ~Track()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_clips != null)
                {
                    foreach (IClip clip in _clips)
                    {
                        clip.Dispose();
                    }
                    _clips = null;
                }

                if (_effects != null)
                {
                    foreach (IEffect effect in _effects)
                    {
                        effect.Dispose();
                    }
                    _effects = null;
                }

                if (_transitions != null)
                {
                    foreach (ITransition transition in _transitions)
                    {
                        transition.Dispose();
                    }
                    _transitions = null;
                }
            }

            if (_track != null)
            {
                Marshal.ReleaseComObject(_track);
                _track = null;
            }
        }
    }
}