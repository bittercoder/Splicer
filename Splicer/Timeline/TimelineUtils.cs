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
using System.Text;
using DirectShowLib;
using DirectShowLib.DES;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public static class TimelineUtils
    {
        /// <summary>
        /// Number of MilliSeconds in a second.
        /// </summary>
        /// <remarks>
        /// This constant may be useful for calculations
        /// </remarks>
        public const long MILLISECONDS = (1000); // 10 ^ 3

        /// <summary>
        /// Number of NanoSeconds in a second.
        /// </summary>
        /// <remarks>
        /// This constant may be useful for calculations
        /// </remarks>
        public const long NANOSECONDS = (1000000000); // 10 ^ 9

        /// <summary>
        /// Number of 100NS in a second.
        /// </summary>
        /// <remarks>
        /// To convert from seconds to 100NS 
        /// units (used by most DES function), multiply the seconds by UNITS.
        /// </remarks>
        public const long UNITS = (NANOSECONDS/100); // 10 ^ 7

        /// <summary>
        /// Converts from 100ns UNITS to seconds
        /// </summary>
        /// <param name="units"></param>
        /// <returns></returns>
        public static double ToSeconds(long units)
        {
            if (units == -1) return -1;

            return ((double) units/UNITS);
        }

        /// <summary>
        /// Converts from seconds to 100ns UNITS
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public static long ToUnits(double seconds)
        {
            if (seconds == -1) return -1;

            return (long) (UNITS*seconds);
        }

        /// <summary>
        /// Inserts a group into a timeline, and assigns it the supplied media type.
        /// Will free the media type upon completion.
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        internal static IAMTimelineGroup InsertGroup(IAMTimeline timeline, AMMediaType mediaType, string name)
        {
            try
            {
                int hr = 0;

                IAMTimelineObj groupObj;

                // make the root group/composition
                hr = timeline.CreateEmptyNode(out groupObj, TimelineMajorType.Group);
                DESError.ThrowExceptionForHR(hr);

                if (!string.IsNullOrEmpty(name))
                {
                    hr = groupObj.SetUserName(name);
                    DESError.ThrowExceptionForHR(hr);
                }

                IAMTimelineGroup group = (IAMTimelineGroup) groupObj;

                // Set the media type we just created
                hr = group.SetMediaType(mediaType);
                DESError.ThrowExceptionForHR(hr);


                // add the group to the timeline
                hr = timeline.AddGroup(groupObj);
                DESError.ThrowExceptionForHR(hr);

                return group;
            }
            finally
            {
                DsUtils.FreeAMMediaType(mediaType);
            }
        }

        /// <summary>
        /// Insert an effect into an effectable object
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="effectable"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="effectDefinition"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static IAMTimelineObj InsertEffect(IAMTimeline timeline, IAMTimelineEffectable effectable, string name,
                                                    int priority, double offset, double duration,
                                                    EffectDefinition effectDefinition)
        {
            int hr = 0;

            long unitsStart = ToUnits(offset);
            long unitsEnd = ToUnits(offset + duration);

            IAMTimelineObj effectsObj;
            hr = timeline.CreateEmptyNode(out effectsObj, TimelineMajorType.Effect);
            DESError.ThrowExceptionForHR(hr);

            hr = effectsObj.SetSubObjectGUID(effectDefinition.EffectId);
            DESError.ThrowExceptionForHR(hr);

            if (!string.IsNullOrEmpty(name))
            {
                hr = effectsObj.SetUserName(name);
                DESError.ThrowExceptionForHR(hr);
            }

            hr = effectsObj.SetStartStop(unitsStart, unitsEnd);
            DESError.ThrowExceptionForHR(hr);

            IPropertySetter propertySetter = (IPropertySetter) new PropertySetter();
            PopulatePropertySetter(propertySetter, effectDefinition.Parameters);

            hr = effectsObj.SetPropertySetter(propertySetter);
            DESError.ThrowExceptionForHR(hr);

            hr = effectable.EffectInsBefore(effectsObj, priority);
            DESError.ThrowExceptionForHR(hr);

            return effectsObj;
        }

        /// <summary>
        /// Insert a transition into a transitionable object
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="transable"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="transitionDefinition"></param>
        /// <param name="swapInputs"></param>
        /// <returns></returns>
        internal static IAMTimelineObj InsertTransition(IAMTimeline timeline, IAMTimelineTransable transable,
                                                        string name, double offset, double duration,
                                                        TransitionDefinition transitionDefinition, bool swapInputs)
        {
            int hr = 0;

            IAMTimelineObj transitionObj;
            long unitsStart = ToUnits(offset);
            long unitsEnd = ToUnits(offset + duration);

            hr = timeline.CreateEmptyNode(out transitionObj, TimelineMajorType.Transition);
            DESError.ThrowExceptionForHR(hr);

            name = string.IsNullOrEmpty(name) ? "transition" : name;

            if (swapInputs)
            {
                hr = transitionObj.SetUserName(string.Format("{0} (swapped inputs)", name));
                DESError.ThrowExceptionForHR(hr);
            }
            else
            {
                hr = transitionObj.SetUserName(name);
                DESError.ThrowExceptionForHR(hr);
            }

            hr = transitionObj.SetSubObjectGUID(transitionDefinition.TransitionId);
            DESError.ThrowExceptionForHR(hr);

            hr = transitionObj.SetStartStop(unitsStart, unitsEnd);
            DESError.ThrowExceptionForHR(hr);

            IAMTimelineTrans trans1 = transitionObj as IAMTimelineTrans;

            if (swapInputs)
            {
                hr = trans1.SetSwapInputs(true);
                DESError.ThrowExceptionForHR(hr);
            }

            if (transitionDefinition.Parameters.Count > 0)
            {
                IPropertySetter setter1 = (IPropertySetter) new PropertySetter();
                PopulatePropertySetter(setter1, transitionDefinition.Parameters);

                hr = transitionObj.SetPropertySetter(setter1);
                DESError.ThrowExceptionForHR(hr);
            }

            hr = transable.TransAdd(transitionObj);
            DESError.ThrowExceptionForHR(hr);

            return transitionObj;
        }

        /// <summary>
        /// Populates the supplied property setter with values from the parameter list.
        /// </summary>
        /// <param name="setter"></param>
        /// <param name="parameters"></param>
        internal static void PopulatePropertySetter(IPropertySetter setter, List<Parameter> parameters)
        {
            int hr = 0;

            foreach (Parameter param in parameters)
            {
                DexterParam dexterParam;
                dexterParam.Name = param.Name;
                dexterParam.dispID = param.DispId;
                dexterParam.nValues = 1 + param.Intervals.Count;
                DexterValue[] valueArray = new DexterValue[dexterParam.nValues];
                valueArray[0].v = param.Value;
                valueArray[0].rt = 0;
                valueArray[0].dwInterp = Dexterf.Interpolate;
                for (int i = 0, valueIndex = 1; i < param.Intervals.Count; i++, valueIndex++)
                {
                    Interval interval = param.Intervals[i];
                    valueArray[valueIndex].v = interval.Value;
                    valueArray[valueIndex].rt = ToUnits(interval.Time);
                    if (interval.Mode == IntervalMode.Interpolate)
                    {
                        valueArray[valueIndex].dwInterp = Dexterf.Interpolate;
                    }
                    else
                    {
                        valueArray[valueIndex].dwInterp = Dexterf.Jump;
                    }
                }

                hr = setter.AddProp(dexterParam, valueArray);
                DESError.ThrowExceptionForHR(hr);
            }

            StringBuilder builder = new StringBuilder();
            int printed = 0;
            hr = setter.PrintXML(builder, 0, out printed, 0);
            Console.WriteLine(builder.ToString());
        }

        /// <summary>
        /// Creates a track, and assigns it with a priority to a selected timeline composition.
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static IAMTimelineTrack CreateTrack(IAMTimeline timeline, IAMTimelineComp parent, string name,
                                                     int priority)
        {
            int hr = 0;

            IAMTimelineObj newTrack;

            hr = timeline.CreateEmptyNode(out newTrack, TimelineMajorType.Track);
            DESError.ThrowExceptionForHR(hr);

            if (!string.IsNullOrEmpty(name))
            {
                hr = newTrack.SetUserName(name);
                DESError.ThrowExceptionForHR(hr);
            }

            hr = parent.VTrackInsBefore(newTrack, priority);
            DsError.ThrowExceptionForHR(hr);

            return (IAMTimelineTrack) newTrack;
        }

        /// <summary>
        /// Creates a composition, and assigns it with a priority to a selected composition
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="parent"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static IAMTimelineComp CreateComposition(IAMTimeline timeline, IAMTimelineComp parent, string name,
                                                          int priority)
        {
            int hr = 0;

            IAMTimelineObj newComposition;

            hr = timeline.CreateEmptyNode(out newComposition, TimelineMajorType.Composite);
            DESError.ThrowExceptionForHR(hr);

            if (!string.IsNullOrEmpty(name))
            {
                hr = newComposition.SetUserName(name);
                DESError.ThrowExceptionForHR(hr);
            }

            hr = parent.VTrackInsBefore(newComposition, priority);
            DsError.ThrowExceptionForHR(hr);

            return (IAMTimelineComp) newComposition;
        }

        /// <summary>
        /// Creates a des composition, wraps it into an IComposition, adds it to a collecton
        /// and returns the new IComposition wrapper.
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="desComposition"></param>
        /// <param name="compositions"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static IComposition AddCompositionToCollection(ICompositionContainer container, IAMTimeline timeline,
                                                                IAMTimelineComp desComposition,
                                                                AddOnlyList<IComposition> compositions, string name,
                                                                int priority)
        {
            priority = ReorderPriorities(compositions, priority);

            IComposition composition = new Composition(container,
                                                       timeline,
                                                       CreateComposition(timeline, desComposition, name, priority),
                                                       name,
                                                       priority);

            compositions.Add(composition);

            return composition;
        }

        /// <summary>
        /// Creates a des track, wraps it into an ITrack, adds it to a collection and
        /// returns the new ITrack wrapper.
        /// </summary>
        /// <param name="desTimeline"></param>
        /// <param name="desComposition"></param>
        /// <param name="tracks"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
        internal static ITrack AddTrackToCollection(ITrackContainer container, IAMTimeline desTimeline,
                                                    IAMTimelineComp desComposition,
                                                    AddOnlyList<ITrack> tracks, string name, int priority)
        {
            priority = ReorderPriorities(tracks, priority);

            ITrack track = new Track(container, desTimeline, CreateTrack(desTimeline, desComposition, name, priority),
                                     name, priority);

            tracks.Add(track);

            return track;
        }

        /// <summary>
        /// Creates a des effect, wraps it into an IEffect, adds it to a collection
        /// and returns the new IEffect wrapper.
        /// </summary>
        /// <param name="desTimeline"></param>
        /// <param name="desEffectable"></param>
        /// <param name="effects"></param>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="effectDefinition"></param>
        /// <returns></returns>
        internal static IEffect AddEffectToCollection(IEffectContainer container, IAMTimeline desTimeline,
                                                      IAMTimelineEffectable desEffectable,
                                                      AddOnlyList<IEffect> effects, string name, int priority,
                                                      double offset, double duration, EffectDefinition effectDefinition)
        {
            priority = ReorderPriorities(effects, priority);

            IAMTimelineObj desEffect =
                InsertEffect(desTimeline, desEffectable, name, priority, offset, duration, effectDefinition);

            IEffect effect = new Effect(container, desEffect, name, priority, offset, duration, effectDefinition);

            effects.Add(effect);

            return effect;
        }

        /// <summary>
        /// Creates a des transition, wraps it into an ITransition, adds it to a collection
        /// and returns a new ITransition wrapper.
        /// </summary>
        /// <param name="desTimeline"></param>
        /// <param name="transable"></param>
        /// <param name="transitions"></param>
        /// <param name="name"></param>
        /// <param name="offset"></param>
        /// <param name="duration"></param>
        /// <param name="transitionDefinition"></param>
        /// <param name="swapInputs"></param>
        /// <returns></returns>
        internal static ITransition AddTransitionToCollection(ITransitionContainer container, IAMTimeline desTimeline,
                                                              IAMTimelineTransable transable,
                                                              AddOnlyList<ITransition> transitions, string name,
                                                              double offset, double duration,
                                                              TransitionDefinition transitionDefinition, bool swapInputs)
        {
            CheckForTransitionOveralp(container, offset, duration);

            IAMTimelineObj desTransition =
                InsertTransition(desTimeline, transable, name, offset, duration, transitionDefinition, swapInputs);

            ITransition transition =
                new Transition(container, desTransition, name, offset, duration, swapInputs, transitionDefinition);

            transitions.Add(transition);

            return transition;
        }

        private static void CheckForTransitionOveralp(ITransitionContainer container, double offset, double duratinon)
        {
            double prospectStart = offset;
            double prospectEnd = duratinon + offset;
            for (int i = 0; i < container.Transitions.Count; i++)
            {
                ITransition transition = container.Transitions[i];
                double start = transition.Offset;
                double end = transition.Offset + transition.Duration;

                if ((prospectStart < end) && (prospectEnd > start))
                {
                    throw new SplicerException(
                        string.Format("Propsective transition overlaps with an existing transition at index: {0}", i));
                }
            }
        }

        private static int ReorderPriorities<T>(AddOnlyList<T> list, int newPriority) where T : IPriority
        {
            if (newPriority < 0)
            {
                return list.Count;
            }
            else
            {
                foreach (IPriority priority in list)
                {
                    if (priority.Priority >= newPriority)
                    {
                        ((IPrioritySetter) priority).SetPriority(priority.Priority + 1);
                    }
                }

                return newPriority;
            }
        }
    }
}