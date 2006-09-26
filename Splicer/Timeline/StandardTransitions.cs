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
using Splicer.Utils;

namespace Splicer.Timeline
{
    public enum DxtKeyTypes
    {
        /// <summary>
        /// Chroma key
        /// </summary>
        RGB = 0,
        /// <summary>
        /// Makes blue and green areas transparent
        /// </summary>
        NoRed = 1,
        /// <summary>
        /// Luminance
        /// </summary>        
        Luminance = 2,
        /// <summary>
        /// key by alpha
        /// </summary>
        Alpha = 3,
        /// <summary>
        /// key by hue
        /// </summary>
        Hue = 4
    }

    public static class StandardTransitions
    {
        public static TransitionDefinition CreateIris()
        {
            TransitionDefinition transitionDefinition = new TransitionDefinition(DxtSubObjects.IrisTransition);
            return transitionDefinition;
        }

        public static TransitionDefinition CreatePixelate()
        {
            TransitionDefinition transitionDefinition = new TransitionDefinition(DxtSubObjects.PixelateTransition);
            return transitionDefinition;
        }

        public static TransitionDefinition CreateFade()
        {
            TransitionDefinition transitionDefinition = new TransitionDefinition(DxtSubObjects.FadeTransition);
            return transitionDefinition;
        }

        public static TransitionDefinition CreateDxtKey(DxtKeyTypes type, int? hue, bool? invert, int? luminance,
                                                        UInt32? rgb, int? similarity)
        {
            TransitionDefinition transitionDefinition = new TransitionDefinition(DxtSubObjects.DxtKey);
            transitionDefinition.Parameters.Add(new Parameter(TransitionParameters.DxtKeyType, (long) type));

            if (hue.HasValue)
            {
                if ((hue < 0) || (hue > 360))
                {
                    throw new ArgumentOutOfRangeException("hue", "hue must be between 0 and 360");
                }
                else if (type != DxtKeyTypes.Hue)
                {
                    throw new ArgumentException("hue specified but selected key type is not \"Hue\"", "hue");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(TransitionParameters.DxtKeyHue, hue.Value));
                }
            }

            if (luminance.HasValue)
            {
                if ((luminance < 0) || (luminance > 100))
                {
                    throw new ArgumentOutOfRangeException("luminance", "luminance must be between 0 and 100");
                }
                else if (type != DxtKeyTypes.Hue)
                {
                    throw new ArgumentException("hue specified but selected key type is not \"Hue\"", "hue");
                }
                else
                {
                    transitionDefinition.Parameters.Add(
                        new Parameter(TransitionParameters.DxtKeyLuminance, luminance.Value));
                }
            }

            if (rgb.HasValue)
            {
                if ((rgb < 0) || (rgb > 0xFFFFFF))
                {
                    throw new ArgumentOutOfRangeException("rgb", "rgb must be between 0x000000 and 0xFFFFFF");
                }
                else if (type != DxtKeyTypes.RGB)
                {
                    throw new ArgumentException("rgb specified but selected key type is not \"RGB\"", "rgb");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(TransitionParameters.DxtKeyRGB, rgb.Value));
                }
            }

            if (similarity.HasValue)
            {
                if ((similarity < 0) || (similarity > 100))
                {
                    throw new ArgumentOutOfRangeException("similarity", "similarity must be between 0 and 100");
                }
                else if ((type != DxtKeyTypes.RGB) && (type != DxtKeyTypes.NoRed))
                {
                    throw new ArgumentException("similarity specified but selected key type does not support it",
                                                "similarity");
                }
                else
                {
                    transitionDefinition.Parameters.Add(
                        new Parameter(TransitionParameters.DxtKeySimilarity, similarity.Value));
                }
            }

            if (invert.HasValue)
            {
                if ((type != DxtKeyTypes.RGB) && (type != DxtKeyTypes.Hue) && (type != DxtKeyTypes.Luminance) &&
                    (type != DxtKeyTypes.NoRed))
                {
                    throw new ArgumentException("invert specified but selected key type does not support it", "invert");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(TransitionParameters.DxtKeyInvert, invert.Value));
                }
            }

            return transitionDefinition;
        }
    }
}