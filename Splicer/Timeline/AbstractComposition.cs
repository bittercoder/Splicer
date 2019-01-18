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
using DirectShowLib.DES;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    public abstract class AbstractComposition : IComposition, IPrioritySetter
    {
        private readonly IAMTimeline _desTimeline;
        private readonly string _name;
        private AddOnlyCollection<IComposition> _compositions = new AddOnlyCollection<IComposition>();
        private AddOnlyCollection<IEffect> _effects = new AddOnlyCollection<IEffect>();
        private int _priority;
        private IAMTimelineComp _timelineComposition;
        private AddOnlyCollection<ITrack> _tracks = new AddOnlyCollection<ITrack>();
        private AddOnlyCollection<ITransition> _transitions = new AddOnlyCollection<ITransition>();

        protected AbstractComposition(ITimeline timeline, string name, int priority)
        {
            _desTimeline = timeline.DesTimeline;
            _name = name;
            _priority = priority;
        }

        protected AbstractComposition(IAMTimeline timeline, string name, int priority)
        {
            _desTimeline = timeline;
            _name = name;
            _priority = priority;
        }

        protected IAMTimelineComp TimelineComposition
        {
            get { return _timelineComposition; }
            set { _timelineComposition = value; }
        }

        #region IComposition Members

        public IGroup Group
        {
            get { return Container.Group; }
        }

        public abstract ICompositionContainer Container { get; }

        public IComposition AddComposition(string name, int priority)
        {
            OnAddingComposition();

            IComposition composition =
                TimelineBuilder.AddCompositionToCollection(this, _desTimeline, _timelineComposition, _compositions, name,
                                                           priority);

            composition.AddingEffect += child_BeforeEffectAdded;
            composition.AddedEffect += child_AfterEffectAdded;
            composition.AddedClip += child_AfterClipAdded;
            composition.AddingClip += child_BeforeClipAdded;
            composition.AddedTransition +=
                child_AfterTransitionAdded;
            composition.AddingTransition += child_BeforeTransitionAdded;
            composition.AddingTrack += child_BeforeTrackAdded;
            composition.AddedTrack += child_AfterTrackAdded;
            composition.AddingComposition += child_BeforeCompositionAdded;
            composition.AddedComposition +=
                child_AfterCompositionAdded;

            OnAddedComposition(composition);

            return composition;
        }

        public AddOnlyCollection<IComposition> Compositions
        {
            get { return _compositions; }
        }

        public ITrack AddTrack(string name, int priority)
        {
            OnAddingTrack();

            ITrack track =
                TimelineBuilder.AddTrackToCollection(this, _desTimeline, _timelineComposition, _tracks, name, priority);

            track.AddingEffect += child_BeforeEffectAdded;
            track.AddedEffect += child_AfterEffectAdded;
            track.AddedClip += child_AfterClipAdded;
            track.AddingClip += child_BeforeClipAdded;
            track.AddedTransition += child_AfterTransitionAdded;
            track.AddingTransition += child_BeforeTransitionAdded;

            OnAddedTrack(track);

            return track;
        }

        public AddOnlyCollection<ITrack> Tracks
        {
            get { return _tracks; }
        }

        public IEffect AddEffect(double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            return AddEffect(null, -1, offset, duration, effectDefinition);
        }

        public IEffect AddEffect(string name, int priority, double offset, double duration,
                                 EffectDefinition effectDefinition)
        {
            OnAddingEffect();

            IEffect effect =
                TimelineBuilder.AddEffectToCollection(this, _desTimeline, (IAMTimelineEffectable) _timelineComposition,
                                                      _effects,
                                                      name, priority, offset, duration, effectDefinition);

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
                TimelineBuilder.AddTransitionToCollection(this, _desTimeline,
                                                          (IAMTimelineTransable) _timelineComposition,
                                                          _transitions, name, offset, duration, transitionDefinition,
                                                          swapInputs);

            OnAddedTransition(transition);

            return transition;
        }

        public AddOnlyCollection<ITransition> Transitions
        {
            get { return _transitions; }
        }

        public int Priority
        {
            get { return _priority; }
        }

        public string Name
        {
            get { return _name; }
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

        public IComposition AddComposition()
        {
            return AddComposition(null, -1);
        }

        public ITrack AddTrack()
        {
            return AddTrack(null, -1);
        }

        #endregion

        #region IPrioritySetter Members

        void IPrioritySetter.SetPriority(int newValue)
        {
            _priority = newValue;
        }

        #endregion

        private event EventHandler _beforeCompositionAdded;
        private event EventHandler<AddedCompositionEventArgs> _afterCompositionAdded;
        private event EventHandler _beforeTrackAdded;
        private event EventHandler<AddedTrackEventArgs> _afterTrackAdded;
        private event EventHandler<AddedEffectEventArgs> _afterEffectAdded;
        private event EventHandler _beforeEffectAdded;
        private event EventHandler _beforeTransitionAdded;
        private event EventHandler<AddedTransitionEventArgs> _afterTransitionAdded;
        private event EventHandler _beforeClipAdded;
        private event EventHandler<AddedClipEventArgs> _afterClipAdded;

        private void child_AfterCompositionAdded(object sender, AddedCompositionEventArgs e)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(sender, e);
            }
        }

        private void child_BeforeCompositionAdded(object sender, EventArgs e)
        {
            if (_beforeCompositionAdded != null)
            {
                _beforeCompositionAdded(sender, e);
            }
        }

        private void child_AfterTrackAdded(object sender, AddedTrackEventArgs e)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(sender, e);
            }
        }

        private void child_BeforeTrackAdded(object sender, EventArgs e)
        {
            if (_beforeTrackAdded != null)
            {
                _beforeTrackAdded(sender, e);
            }
        }

        private void child_BeforeTransitionAdded(object sender, EventArgs e)
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(sender, e);
            }
        }

        private void child_AfterTransitionAdded(object sender, AddedTransitionEventArgs e)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(sender, e);
            }
        }

        private void child_BeforeClipAdded(object sender, EventArgs e)
        {
            if (_beforeClipAdded != null)
            {
                _beforeClipAdded(sender, e);
            }
        }

        private void child_AfterClipAdded(object sender, AddedClipEventArgs e)
        {
            if (_afterClipAdded != null)
            {
                _afterClipAdded(sender, e);
            }
        }

        private void child_AfterEffectAdded(object sender, AddedEffectEventArgs e)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(sender, e);
            }
        }

        private void child_BeforeEffectAdded(object sender, EventArgs e)
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(sender, e);
            }
        }

        protected void OnAddedEffect(IEffect effect)
        {
            if (_afterEffectAdded != null)
            {
                _afterEffectAdded(this, new AddedEffectEventArgs(effect, this));
            }
        }

        protected void OnAddingEffect()
        {
            if (_beforeEffectAdded != null)
            {
                _beforeEffectAdded(this, EventArgs.Empty);
            }
        }

        protected void OnAddingTransition()
        {
            if (_beforeTransitionAdded != null)
            {
                _beforeTransitionAdded(this, EventArgs.Empty);
            }
        }

        protected void OnAddedTransition(ITransition transition)
        {
            if (_afterTransitionAdded != null)
            {
                _afterTransitionAdded(this, new AddedTransitionEventArgs(transition, this));
            }
        }

        protected void OnAddingComposition()
        {
            if (_beforeCompositionAdded != null)
            {
                _beforeCompositionAdded(this, EventArgs.Empty);
            }
        }

        protected void OnAddedComposition(IComposition composition)
        {
            if (_afterCompositionAdded != null)
            {
                _afterCompositionAdded(this, new AddedCompositionEventArgs(composition, this));
            }
        }

        protected void OnAddingTrack()
        {
            if (_beforeTrackAdded != null)
            {
                _beforeTrackAdded(this, EventArgs.Empty);
            }
        }

        protected void OnAddedTrack(ITrack track)
        {
            if (_afterTrackAdded != null)
            {
                _afterTrackAdded(this, new AddedTrackEventArgs(track, this));
            }
        }

        protected void DisposeComposition(bool disposing)
        {
            if (disposing)
            {
                if (_compositions != null)
                {
                    foreach (IComposition composition in _compositions)
                    {
                        var disposable = composition as IDisposable;
                        if (disposable != null) disposable.Dispose();
                    }

                    _compositions = null;
                }

                if (_tracks != null)
                {
                    foreach (ITrack track in _tracks)
                    {
                        track.Dispose();
                    }

                    _tracks = null;
                }

                if (_transitions != null)
                {
                    foreach (ITransition transition in _transitions)
                    {
                        transition.Dispose();
                    }

                    _transitions = null;
                }

                if (_effects != null)
                {
                    foreach (IEffect effect in _effects)
                    {
                        effect.Dispose();
                    }

                    _effects = null;
                }
            }
        }
    }
}