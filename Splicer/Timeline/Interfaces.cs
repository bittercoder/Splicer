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
using DirectShowLib.DES;
using Splicer.Utilities;

namespace Splicer.Timeline
{
    public interface IBelongsToGroup
    {
        IGroup Group { get; }
    }

    public abstract class AddedToContainerEventArgs<TItem, TContainer> : EventArgs
    {
        private readonly TContainer _container;
        private readonly TItem _item;

        protected AddedToContainerEventArgs(TItem item, TContainer container)
        {
            if (item == null) throw new ArgumentNullException("item");
            if (container == null) throw new ArgumentNullException("container");
            _item = item;
            _container = container;
        }

        public TItem Item
        {
            get { return _item; }
        }

        public TContainer Container
        {
            get { return _container; }
        }
    }

    public class AddedEffectEventArgs : AddedToContainerEventArgs<IEffect, IEffectContainer>
    {
        public AddedEffectEventArgs(IEffect item, IEffectContainer container)
            : base(item, container)
        {
        }
    }

    public class AddedTransitionEventArgs : AddedToContainerEventArgs<ITransition, ITransitionContainer>
    {
        public AddedTransitionEventArgs(ITransition item, ITransitionContainer container)
            : base(item, container)
        {
        }
    }

    public class AddedTrackEventArgs : AddedToContainerEventArgs<ITrack, ITrackContainer>
    {
        public AddedTrackEventArgs(ITrack item, ITrackContainer container)
            : base(item, container)
        {
        }
    }

    public class AddedClipEventArgs : AddedToContainerEventArgs<IClip, IClipContainer>
    {
        public AddedClipEventArgs(IClip item, IClipContainer container)
            : base(item, container)
        {
        }
    }

    public class AddedGroupEventArgs : AddedToContainerEventArgs<IGroup, IGroupContainer>
    {
        public AddedGroupEventArgs(IGroup item, IGroupContainer container)
            : base(item, container)
        {
        }
    }

    public class AddedCompositionEventArgs : AddedToContainerEventArgs<IComposition, ICompositionContainer>
    {
        public AddedCompositionEventArgs(IComposition item, ICompositionContainer container)
            : base(item, container)
        {
        }
    }

    public interface IName
    {
        string Name { get; }
    }

    public interface IPriority
    {
        int Priority { get; }
    }

    internal interface IPrioritySetter
    {
        void SetPriority(int newValue);
    }

    public interface IEffect : IName, IPriority, IDisposable, IBelongsToGroup
    {
        double Offset { get; }
        double Duration { get; }
        EffectDefinition EffectDefinition { get; }
        IEffectContainer Container { get; }
    }

    public interface ITransition : IName, IDisposable, IBelongsToGroup
    {
        double Offset { get; }
        double Duration { get; }
        bool SwapInputs { get; }
        TransitionDefinition TransitionDefinition { get; }
        ITransitionContainer Container { get; }
    }

    public interface IEffectContainer : IBelongsToGroup
    {
        AddOnlyCollection<IEffect> Effects { get; }
        event EventHandler<AddedEffectEventArgs> AddedEffect;
        event EventHandler AddingEffect;
        IEffect AddEffect(double offset, double duration, EffectDefinition effectDefinition);
        IEffect AddEffect(string name, int priority, double offset, double duration, EffectDefinition effectDefinition);
    }

    public interface ITransitionContainer : IBelongsToGroup
    {
        AddOnlyCollection<ITransition> Transitions { get; }
        event EventHandler AddingTransition;
        event EventHandler<AddedTransitionEventArgs> AddedTransition;

        ITransition AddTransition(double offset, double duration, TransitionDefinition transitionDefinition);

        ITransition AddTransition(double offset, double duration, TransitionDefinition transitionDefinition,
                                  bool swapInputs);

        ITransition AddTransition(string name, double offset, double duration, TransitionDefinition transitionDefinition,
                                  bool swapInputs);
    }

    public interface ITrackContainer : IBelongsToGroup
    {
        AddOnlyCollection<ITrack> Tracks { get; }
        event EventHandler AddingTrack;
        event EventHandler<AddedTrackEventArgs> AddedTrack;
        ITrack AddTrack(string name, int priority);
        ITrack AddTrack();
    }

    public interface IClipContainer : IBelongsToGroup
    {
        AddOnlyCollection<IClip> Clips { get; }
        event EventHandler AddingClip;
        event EventHandler<AddedClipEventArgs> AddedClip;

        IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                      double offset,
                      double clipStart, double clipEnd, bool manageLifespan);

        IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipEnd);
        IClip AddVideo(string fileName, double offset);
        IClip AddVideo(string fileName);

        IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipEnd);
        IClip AddImage(string fileName, double offset);
        IClip AddImage(string fileName);

        IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipEnd);
        IClip AddAudio(string fileName, double offset);
        IClip AddAudio(string fileName);

        IClip AddImage(string name, Image image, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddImage(Image image, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(Image image, double offset, double clipStart, double clipEnd);
        IClip AddImage(Image image, double offset, double clipEnd);
        IClip AddImage(Image image, double offset);
        IClip AddImage(Image image);
    }

    public interface ICompositionContainer : IBelongsToGroup
    {
        AddOnlyCollection<IComposition> Compositions { get; }
        event EventHandler AddingComposition;
        event EventHandler<AddedCompositionEventArgs> AddedComposition;
        IComposition AddComposition(string name, int priority);
        IComposition AddComposition();
    }

    public interface IComposition : IName, IPriority, ICompositionContainer, ITrackContainer, IEffectContainer,
                                    ITransitionContainer,
                                    IBelongsToGroup
    {
        ICompositionContainer Container { get; }
        event EventHandler AddingClip;
        event EventHandler<AddedClipEventArgs> AddedClip;
    }

    public interface ITrack : IName, IPriority, IEffectContainer, ITransitionContainer, IClipContainer, IDisposable,
                              IBelongsToGroup
    {
        double Duration { get; }
        IVirtualClipCollection VirtualClips { get; }
        ITrackContainer Container { get; }
    }

    public interface IVirtualClip : IName
    {
        double Offset { get; }
        double Duration { get; }
        double MediaStart { get; }
        IClip SourceClip { get; }
    }

    public interface IVirtualClipCollection : IEnumerable<IVirtualClip>
    {
        IVirtualClip this[int index] { get; }
        int Count { get; }
    }

    public interface IClip : IName, IEffectContainer, IDisposable
    {
        double Offset { get; }
        double Duration { get; }
        double MediaStart { get; }
        MediaFile File { get; }
        ResizeFlags StretchMode { get; set; }
        IClipContainer Container { get; }
    }

    public interface IGroup : IComposition, IDisposable
    {
        ITimeline Timeline { get; }
        GroupType Type { get; }
        double Fps { get; }
    }

    public interface IGroupContainer
    {
        AddOnlyCollection<IGroup> Groups { get; }
        event EventHandler AddingGroup;
        event EventHandler<AddedGroupEventArgs> AddedGroup;
        IGroup AddAudioGroup(string name, double fps);
        IGroup AddVideoGroup(string name, double fps, short bitCount, int width, int height);
        IGroup AddAudioGroup(string name);
        IGroup AddVideoGroup(string name, short bitCount, int width, int height);
        IGroup AddAudioGroup();
        IGroup AddVideoGroup(short bitCount, int width, int height);
        IGroup FindFirstGroupOfType(GroupType type);
    }

    public interface IAudioVideoClipPair
    {
        IClip AudioClip { get; }
        IClip VideoClip { get; }
    }

    public interface ITimeline : IGroupContainer, IDisposable
    {
        double Fps { get; }
        IAMTimeline DesTimeline { get; }
        double Duration { get; }
        long LengthInUnits { get; }
        IEnumerable<IMediaFileAssistant> Assistants { get; }

        string TemporaryStoragePath { get; set; }
        event EventHandler AddingClip;
        event EventHandler<AddedClipEventArgs> AddedClip;
        event EventHandler AddingComposition;
        event EventHandler<AddedCompositionEventArgs> AddedComposition;
        event EventHandler AddingTrack;
        event EventHandler<AddedTrackEventArgs> AddedTrack;
        event EventHandler AddingTransition;
        event EventHandler<AddedTransitionEventArgs> AddedTransition;
        event EventHandler<AddedEffectEventArgs> AddedEffect;
        event EventHandler AddingEffect;

        IClip AddClip(string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position, double offset,
                      double clipStart, double clipEnd);

        IClip AddVideo(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddClip(string name, string fileName, GroupMediaType mediaType, InsertPosition position,
                      double offset,
                      double clipStart, double clipEnd, bool manageLifespan);

        IClip AddVideo(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddVideo(string fileName, double offset, double clipEnd);
        IClip AddVideo(string fileName, double offset);
        IClip AddVideo(string fileName);

        IClip AddImage(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddImage(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddImage(string fileName, double offset, double clipEnd);
        IClip AddImage(string fileName, double offset);
        IClip AddImage(string fileName);

        IClip AddAudio(string name, string fileName, InsertPosition position, double offset, double clipStart,
                       double clipEnd);

        IClip AddAudio(string fileName, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipStart, double clipEnd);
        IClip AddAudio(string fileName, double offset, double clipEnd);
        IClip AddAudio(string fileName, double offset);
        IClip AddAudio(string fileName);

        IAudioVideoClipPair AddVideoWithAudio(string name, string fileName, InsertPosition position, double offset,
                                              double clipStart, double clipEnd);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, InsertPosition position, double offset, double clipStart,
                                              double clipEnd);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipStart, double clipEnd);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipEnd);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset);
        IAudioVideoClipPair AddVideoWithAudio(string fileName);

        IAudioVideoClipPair AddVideoWithAudio(string name, string fileName, InsertPosition position, double offset,
                                              double clipStart, double clipEnd, bool shadowCopyAudio);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, InsertPosition position, double offset, double clipStart,
                                              double clipEnd, bool shadowCopyAudio);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipStart, double clipEnd,
                                              bool shadowCopyAudio);

        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, double clipEnd, bool shadowCopyAudio);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, double offset, bool shadowCopyAudio);
        IAudioVideoClipPair AddVideoWithAudio(string fileName, bool shadowCopyAudio);

        /*IClip AddImage(Image image, InsertPosition position, double offset, double clipStart, double clipEnd);
        IClip AddImage(Image image, double offset, double clipStart, double clipEnd);
        IClip AddImage(Image image, double offset, double clipEnd);
        IClip AddImage(Image image, double offset);
        IClip AddImage(Image image);*/

        void InstallAssistant(IMediaFileAssistant assistant);
    }

    public interface IMediaFileAssistant
    {
        bool WillAssist(MediaFile file);
        IDisposable Assist(MediaFile file);
    }
}