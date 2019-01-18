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
using System.Globalization;

namespace Splicer.Timeline
{
    public static class StandardEffects
    {
        /// <summary>
        /// Alpha setter effect, also known as the AlphaSetterEffect
        /// </summary>
        public static readonly Guid AlphaSetterEffect = new Guid("506D89AE-909A-44f7-9444-ABD575896E35");

        /// <summary>
        /// Audio Mixer effect
        /// </summary>
        public static readonly Guid AudioMixerEffect = new Guid("036A9790-C153-11d2-9EF7-006008039E37");

        /// <summary>
        /// Blur effect
        /// </summary>
        public static readonly Guid BlurEffect = new Guid("7312498D-E87A-11D1-81E0-0000F87557DB");

        /// <summary>
        /// DX Matrix effect
        /// </summary>
        public static readonly Guid MatrixEffect = new Guid("4ABF5A06-5568-4834-BEE3-327A6D95A685");

        /// <summary>
        /// The mirror and gray scale effect
        /// </summary>
        public static readonly Guid MirrorAndGrayscaleEffect = new Guid("16B280C8-EE70-11D1-9066-00C04FD9189D");

        /// <summary>
        /// Performs a pixelate effect
        /// </summary>
        public static readonly Guid PixelateEffect = new Guid("4CCEA634-FBE0-11D1-906A-00C04FD9189D");

        public static EffectDefinition CreateDefaultBlur()
        {
            var blurEffectDefinition = new EffectDefinition(BlurEffect);
            return blurEffectDefinition;
        }

        public static EffectDefinition CreateBlurEffect(double startRadius, double duration, double endRadius)
        {
            var blurEffectDefinition = new EffectDefinition(BlurEffect);
            blurEffectDefinition.Parameters.Add(
                new Parameter(EffectParameters.BlurPixelRadius, startRadius, duration, endRadius));

            return blurEffectDefinition;
        }

        public static EffectDefinition CreateAlphaSetter(byte newAlpha)
        {
            var alphaSetterDefinition = new EffectDefinition(AlphaSetterEffect);
            alphaSetterDefinition.Parameters.Add(new Parameter("Alpha", newAlpha));

            return alphaSetterDefinition;
        }

        public static EffectDefinition CreateAlphaSetterRamp(double alphaRamp)
        {
            if ((alphaRamp < 0) || (alphaRamp > 1))
            {
                throw new SplicerException("The alpha ramp value must be a percentage between 0 and 1");
            }

            var alphaSetterDefinition = new EffectDefinition(AlphaSetterEffect);
            alphaSetterDefinition.Parameters.Add(new Parameter("AlphaRamp", alphaRamp));

            return alphaSetterDefinition;
        }

        public static EffectDefinition CreateAudioEnvelope(double volumePercent)
        {
            if ((volumePercent < 0) || (volumePercent > 1))
            {
                throw new SplicerException("Volume percentage must be between 0 and 1");
            }

            var definition = new EffectDefinition(AudioMixerEffect);
            definition.Parameters.Add(new Parameter("Vol", volumePercent));

            return definition;
        }


        public static EffectDefinition CreateAudioEnvelope(double volumePercent, double fadeInDuration,
                                                           double fadeOutDuration, double duration)
        {
            if ((volumePercent < 0) || (volumePercent > 1))
            {
                throw new SplicerException("Volume percentage must be between 0 and 1");
            }

            if ((fadeInDuration + fadeOutDuration) > duration)
            {
                throw new SplicerException(
                    "Sum of the fade-in and fade-out durations must not exceed the total duration");
            }

            string volumeValue = volumePercent.ToString(CultureInfo.InvariantCulture);

            var definition = new EffectDefinition(AudioMixerEffect);

            var volumeParameter = new Parameter();
            volumeParameter.Name = "Vol";

            if (fadeInDuration > 0)
            {
                volumeParameter.Value = "0";
                volumeParameter.Intervals.Add(new Interval(fadeInDuration, volumeValue));
            }
            else
            {
                volumeParameter.Value = volumeValue;
            }

            if (fadeOutDuration > 0)
            {
                volumeParameter.Intervals.Add(new Interval(duration - (fadeInDuration + fadeOutDuration), volumeValue));
                volumeParameter.Intervals.Add(new Interval(duration, "0"));
            }
            else
            {
                volumeParameter.Intervals.Add(new Interval(duration, volumeValue));
            }

            definition.Parameters.Add(volumeParameter);

            return definition;
        }
    }
}