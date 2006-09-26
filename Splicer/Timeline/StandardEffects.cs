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

using System.Globalization;
using Splicer.Utils;

namespace Splicer.Timeline
{
    public static class StandardEffects
    {
        public static EffectDefinition CreateDefaultBlur()
        {
            EffectDefinition blurEffectDefinition = new EffectDefinition(DxtSubObjects.sIEBlurGuid);
            return blurEffectDefinition;
        }

        public static EffectDefinition CreateBlurEffect(double startRadius, double duration, double endRadius)
        {
            EffectDefinition blurEffectDefinition = new EffectDefinition(DxtSubObjects.sIEBlurGuid);
            blurEffectDefinition.Parameters.Add(
                new Parameter(EffectParameters.BlurPixelRadius, startRadius, duration, endRadius));

            return blurEffectDefinition;
        }

        public static EffectDefinition CreateAlphaSetter(byte newAlpha)
        {
            EffectDefinition alphaSetterDefinition = new EffectDefinition(DxtSubObjects.DxtAlphaSetter);
            alphaSetterDefinition.Parameters.Add(new Parameter("Alpha", newAlpha));

            return alphaSetterDefinition;
        }

        public static EffectDefinition CreateAlphaSetterRamp(double alphaRamp)
        {
            if ((alphaRamp < 0) || (alphaRamp > 1))
            {
                throw new SplicerException("The alpha ramp value must be a percentage between 0 and 1");
            }

            EffectDefinition alphaSetterDefinition = new EffectDefinition(DxtSubObjects.DxtAlphaSetter);
            alphaSetterDefinition.Parameters.Add(new Parameter("AlphaRamp", alphaRamp));

            return alphaSetterDefinition;
        }

        public static EffectDefinition CreateAudioEnvelope(double volumePercent, double duration)
        {
            if ((volumePercent < 0) || (volumePercent > 1))
            {
                throw new SplicerException("Volume percentage must be between 0 and 1");
            }

            EffectDefinition definition = new EffectDefinition(DxtSubObjects.AudioMixer);
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

            EffectDefinition definition = new EffectDefinition(DxtSubObjects.AudioMixer);

            Parameter volumeParameter = new Parameter();
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