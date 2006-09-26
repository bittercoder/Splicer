using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public class Track : ITrack, IPrioritySetter
    {
        public const double DefaultImageDisplayDuration = 1.0;
        private event EventHandler<AfterEffectAddedEventArgs> _afterEffectAdded;
        private event EventHandler _beforeEffectAdded;
        private event EventHandler _beforeTransitionAdded;
        private event EventHandler<AfterTransitionAddedEventArgs> _afterTransitionAdded;
        private event EventHandler _beforeClipAdded;
        private event EventHandler<AfterClipAddedEventArgs> _afterClipAdded;
        private AddOnlyList<IEffect> _effects = new AddOnlyList<IEffect>();
        private AddOnlyList<ITransition> _transitions = new AddOnlyList<ITransition>();
        private AddOnlyList<IClip> _clips = new AddOnlyList<IClip>();
        private IAMTimeline _timeline;
        private IAMTimelineTrack _track;
        private string _name;
        private int _priority;
        private long _length;
        private readonly double _fps;
        private VirtualClipCollection _virtualClips = new VirtualClipCollection();
        private ITrackContainer _container;

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

        public IGroup Group
        {
            get { return Container.Group; }
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

        public ITrackContainer Container
        {
            get { return _container; }
        }

        public AddOnlyList<IClip> Clips
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
            OnBeforeEffectAdded();

            IEffect effect =
                TimelineUtils.AddEffectToCollection(this, _timeline, (IAMTimelineEffectable) _track, _effects, name,
                                                    priority,
                                                    offset, duration, effectDefinition);

            OnAfterEffectAdded(effect);

            return effect;
        }

        public AddOnlyList<IEffect> Effects
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
            OnBeforeTransitionAdded();

            ITransition transition =
                TimelineUtils.AddTransitionToCollection(this, _timeline, (IAMTimelineTransable) _track, _transitions,
                                                        name,
                                                        offset, duration, transitionDefinition, swapInputs);

            OnAfterTransitionAdded(transition);

            return transition;
        }

        public AddOnlyList<ITransition> Transitions
        {
            get { return _transitions; }
        }

        public IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                             double clipStart, double clipEnd)
        {
            return AddClip(null, fileName, mediaType, position, offset, clipStart, clipEnd);
        }

        private void CheckMediaTypeAgainstGroup(GroupMediaType mediaType)
        {
            if (mediaType == GroupMediaType.Audio)
            {
                if (Group.Type != GroupType.Audio)
                {
                    throw new SplicerException(
                        "You can not add audio clips to a track which exists within a non-audio group");
                }
            }
            else
            {
                if (Group.Type != GroupType.Video)
                {
                    throw new SplicerException(
                        "You can not add video or image clips to a track which exists within a non-video group");
                }
            }
        }

        private IList<IDisposable> FetchAssistants(MediaFile file)
        {
            List<IDisposable> activeAssistants = new List<IDisposable>();

            foreach (IMediaFileAssistant assistant in Group.Timeline.Assitants)
            {
                if (assistant.WillAssist(file))
                {
                    activeAssistants.Add(assistant.Assist(file));
                }
            }

            return activeAssistants;
        }

        private void DisposeOfAssistants(IList<IDisposable> activeAssistants)
        {
            if (activeAssistants != null)
                foreach (IDisposable disposable in activeAssistants)
                {
                    disposable.Dispose();
                }
        }

        public IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                             double offset,
                             double clipStart, double clipEnd)
        {
            CheckMediaTypeAgainstGroup(mediaType);

            OnBeforeClipAdded();

            if ((clipEnd < 0) && (mediaType == GroupMediaType.Image))
            {
                clipEnd = DefaultImageDisplayDuration + clipStart;
            }

            long durationInUnits;

            MediaFile mediaFile = new MediaFile(fileName);

            double absoluteOffset = position == InsertPosition.Absoloute ? offset : Duration + offset;

            IList<IDisposable> activeAssistants = null;

            IAMTimelineSrc timelineSrc = null;

            try
            {
                activeAssistants = FetchAssistants(mediaFile);

                timelineSrc = CreateMedia(mediaFile,
                                          mediaType,
                                          TimelineUtils.ToUnits(absoluteOffset),
                                          TimelineUtils.ToUnits(clipStart),
                                          TimelineUtils.ToUnits(clipEnd),
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
                new Clip(this, _timeline, timelineSrc, name, absoluteOffset, TimelineUtils.ToSeconds(durationInUnits),
                         clipStart, mediaFile);

            clip.BeforeEffectAdded += new EventHandler(clip_BeforeEffectAdded);
            clip.AfterEffectAdded += new EventHandler<AfterEffectAddedEventArgs>(clip_AfterEffectAdded);

            _clips.Add(clip);

            _virtualClips.AddVirtualClip(clip);

            OnAfterClipAdded(clip);

            return clip;
        }

        private void clip_AfterEffectAdded(object sender, AfterEffectAddedEventArgs e)
        {
            // bubble the event up
            OnAfterEffectAdded(e.Item);
        }

        private void clip_BeforeEffectAdded(object sender, EventArgs e)
        {
            // bubble the event up
            OnBeforeEffectAdded();
        }

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

                // Set the start/end - we fix the FPS at 0 for static media (images)
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
                double d2 = (TimelineUtils.UNITS/_fps);
                double d3 = d1/d2;
                int d4 = (int) Math.Round(d3);

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

        public double Duration
        {
            get { return TimelineUtils.ToSeconds(_length); }
        }

        public event EventHandler<AfterEffectAddedEventArgs> AfterEffectAdded
        {
            add { _afterEffectAdded += value; }
            remove { _afterEffectAdded -= value; }
        }

        protected void OnAfterEffectAdded(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AfterEffectAddedEventArgs(effect, this));
            }
        }

        public event EventHandler BeforeEffectAdded
        {
            add { _beforeEffectAdded += value; }
            remove { _beforeEffectAdded -= value; }
        }

        protected void OnBeforeEffectAdded()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler BeforeTransitionAdded
        {
            add { _beforeTransitionAdded += value; }
            remove { _beforeTransitionAdded -= value; }
        }

        protected void OnBeforeTransitionAdded()
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AfterTransitionAddedEventArgs> AfterTransitionAdded
        {
            add { _afterTransitionAdded += value; }
            remove { _afterTransitionAdded -= value; }
        }

        protected void OnAfterTransitionAdded(ITransition transition)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(this, new AfterTransitionAddedEventArgs(transition, this));
            }
        }

        public event EventHandler BeforeClipAdded
        {
            add { _beforeClipAdded += value; }
            remove { _beforeClipAdded -= value; }
        }

        protected void OnBeforeClipAdded()
        {
            if (_beforeClipAdded != null)
            {
                _beforeClipAdded(this, EventArgs.Empty);
            }
        }

        public event EventHandler<AfterClipAddedEventArgs> AfterClipAdded
        {
            add { _afterClipAdded += value; }
            remove { _afterClipAdded -= value; }
        }

        protected void OnAfterClipAdded(IClip clip)
        {
            if (_afterClipAdded != null)
            {
                _afterClipAdded(this, new AfterClipAddedEventArgs(clip, this));
            }
        }

        #region IDisposable Members

        public void Dispose()
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

            if (_track != null)
            {
                Marshal.ReleaseComObject(_track);
                _track = null;
            }
        }

        #endregion

        #region IPrioritySetter Members

        void IPrioritySetter.SetPriority(int newValue)
        {
            _priority = newValue;
        }

        #endregion

        public IVirtualClipCollection VirtualClips
        {
            get { return _virtualClips; }
        }
    }
}