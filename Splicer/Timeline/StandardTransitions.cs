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

namespace Splicer.Timeline
{
    public static class StandardTransitions
    {
        /// <summary>
        /// The compositor transition
        /// </summary>
        public static readonly Guid CompositorTransition = new Guid("BB44391D-6ABD-422f-9E2E-385C9DFF51FC");

        /// <summary>
        /// Dissolve transition (pixel by pixel)
        /// </summary>
        public static readonly Guid DissolveTransition = new Guid("F7F4A1B6-8E87-452F-A2D7-3077F508DBC0");

        /// <summary>
        /// Fade transition
        /// </summary>
        public static readonly Guid FadeTransition = new Guid("16B280C5-EE70-11D1-9066-00C04FD9189D");

        /// <summary>
        /// Iris transition (divide in four, each moving away into the closest corner)
        /// </summary>
        public static readonly Guid IrisTransition = new Guid("049F2CE6-D996-4721-897A-DB15CE9EB73D");

        /// <summary>
        /// KeyTransition, Also known as the DxtKey transition
        /// </summary>
        public static readonly Guid KeyTransition = new Guid("C5B19592-145E-11d3-9F04-006008039E37");

        /// <summary>
        /// Direct-X media wipe transition
        /// </summary>
        public static readonly Guid MediaWipeTransition = new Guid("AF279B30-86EB-11D1-81BF-0000F87557DB");

        /// <summary>
        /// Pixelate transition
        /// </summary>
        public static readonly Guid PixelateTransition = new Guid("4CCEA634-FBE0-11D1-906A-00C04FD9189D");

        /// <summary>
        /// SmtpWipeTransition, also known as the DxtJpeg transition
        /// </summary>
        public static readonly Guid SmtpWipeTransition = new Guid("DE75D012-7A65-11D2-8CEA-00A0C9441E20");

        public static TransitionDefinition CreateIris()
        {
            var transitionDefinition = new TransitionDefinition(IrisTransition);
            return transitionDefinition;
        }

        public static TransitionDefinition CreatePixelate()
        {
            var transitionDefinition = new TransitionDefinition(PixelateTransition);
            return transitionDefinition;
        }

        public static TransitionDefinition CreateFade()
        {
            var transitionDefinition = new TransitionDefinition(FadeTransition);
            return transitionDefinition;
        }

        [CLSCompliant(false)]
        public static TransitionDefinition CreateKey(KeyTransitionType transitionType, int? hue, bool? invert,
                                                     int? luminance,
                                                     UInt32? rgb, int? similarity)
        {
            var transitionDefinition = new TransitionDefinition(KeyTransition);
            transitionDefinition.Parameters.Add(new Parameter(KeyTransitionParameter.KeyType, (long) transitionType));

            if (hue.HasValue)
            {
                if ((hue < 0) || (hue > 360))
                {
                    throw new ArgumentOutOfRangeException("hue", "hue must be between 0 and 360");
                }
                else if (transitionType != KeyTransitionType.Hue)
                {
                    throw new ArgumentException("hue specified but selected key transitionType is not \"Hue\"", "hue");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(KeyTransitionParameter.Hue, hue.Value));
                }
            }

            if (luminance.HasValue)
            {
                if ((luminance < 0) || (luminance > 100))
                {
                    throw new ArgumentOutOfRangeException("luminance", "luminance must be between 0 and 100");
                }
                else if (transitionType != KeyTransitionType.Hue)
                {
                    throw new ArgumentException("hue specified but selected key transitionType is not \"Hue\"", "hue");
                }
                else
                {
                    transitionDefinition.Parameters.Add(
                        new Parameter(KeyTransitionParameter.Luminance, luminance.Value));
                }
            }

            if (rgb.HasValue)
            {
                if ((rgb < 0) || (rgb > 0xFFFFFF))
                {
                    throw new ArgumentOutOfRangeException("rgb", "rgb must be between 0x000000 and 0xFFFFFF");
                }
                else if (transitionType != KeyTransitionType.Rgb)
                {
                    throw new ArgumentException("rgb specified but selected key transitionType is not \"Rgb\"", "rgb");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(KeyTransitionParameter.Rgb, rgb.Value));
                }
            }

            if (similarity.HasValue)
            {
                if ((similarity < 0) || (similarity > 100))
                {
                    throw new ArgumentOutOfRangeException("similarity", "similarity must be between 0 and 100");
                }
                else if ((transitionType != KeyTransitionType.Rgb) && (transitionType != KeyTransitionType.NoRed))
                {
                    throw new ArgumentException(
                        "similarity specified but selected key transitionType does not support it",
                        "similarity");
                }
                else
                {
                    transitionDefinition.Parameters.Add(
                        new Parameter(KeyTransitionParameter.Similarity, similarity.Value));
                }
            }

            if (invert.HasValue)
            {
                if ((transitionType != KeyTransitionType.Rgb) && (transitionType != KeyTransitionType.Hue) &&
                    (transitionType != KeyTransitionType.Luminance) &&
                    (transitionType != KeyTransitionType.NoRed))
                {
                    throw new ArgumentException("invert specified but selected key transitionType does not support it",
                                                "invert");
                }
                else
                {
                    transitionDefinition.Parameters.Add(new Parameter(KeyTransitionParameter.Invert, invert.Value));
                }
            }

            return transitionDefinition;
        }
    }
}