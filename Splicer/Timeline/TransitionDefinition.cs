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
using System.Globalization;
using System.Xml.Serialization;

namespace Splicer.Timeline
{
    public class Parameter
    {
        private string _name;
        private string _value;
        private int _dispId;
        private List<Interval> _intervals;

        public Parameter()
        {
        }

        public Parameter(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public Parameter(string name, double value)
        {
            _name = name;
            _value = value.ToString(CultureInfo.InvariantCulture);
        }

        public Parameter(string name, bool value)
        {
            _name = name;
            _value = value.ToString(CultureInfo.InvariantCulture);
        }

        public Parameter(string name, long value)
        {
            _name = name;
            _value = value.ToString(CultureInfo.InvariantCulture);
        }

        public Parameter(string name, string startValue, double end, string endValue)
            : this(name, startValue)
        {
            Intervals.Add(new Interval(end, endValue));
        }

        public Parameter(string name, double startValue, double end, double endValue)
            : this(name, startValue)
        {
            Intervals.Add(new Interval(end, endValue.ToString(CultureInfo.InvariantCulture)));
        }

        public Parameter(string name, long startValue, double end, long endValue)
            : this(name, startValue)
        {
            Intervals.Add(new Interval(end, endValue.ToString(CultureInfo.InvariantCulture)));
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        [XmlArray("Intervals")]
        [XmlArrayItem("Interval")]
        public List<Interval> Intervals
        {
            get
            {
                if (_intervals == null) _intervals = new List<Interval>();
                return _intervals;
            }
            set { _intervals = value; }
        }

        public int DispId
        {
            get { return _dispId; }
            set { _dispId = value; }
        }
    }

    /// <summary>
    /// Interval mode, defines the type of interval for this parameter - this is equivalent to
    /// Dexterf in the DirectShowLib (though we avoid using that, so there isn't any required for
    /// DirectShowLib when dealing with serialized configurations).
    /// </summary>
    public enum IntervalMode
    {
        /// <summary>
        /// Comes out as a "linear" tag from the DES XML
        /// </summary>
        Interpolate,

        /// <summary>
        /// Comes out as an "at" tag from the DES XML
        /// </summary>
        Jump
    }

    public class Interval
    {
        private IntervalMode _mode;
        private double _time;
        private string _value;

        public Interval()
        {
        }

        public Interval(double time, string value)
            : this(IntervalMode.Interpolate, time, value)
        {
        }

        public Interval(IntervalMode mode, double time, string value)
        {
            _mode = mode;
            _time = time;
            _value = value;
        }

        public IntervalMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public double Time
        {
            get { return _time; }
            set { _time = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    public class TransitionDefinition
    {
        private Guid _transitionId;
        private List<Parameter> _parameters;

        public TransitionDefinition()
        {
        }

        public TransitionDefinition(Guid transitionId)
        {
            _transitionId = transitionId;
        }

        public Guid TransitionId
        {
            get { return _transitionId; }
            set { _transitionId = value; }
        }

        public List<Parameter> Parameters
        {
            get
            {
                if (_parameters == null) _parameters = new List<Parameter>();
                return _parameters;
            }
            set { _parameters = value; }
        }
    }

    public class EffectDefinition
    {
        private Guid _effectId;
        private List<Parameter> _parameters;

        public EffectDefinition()
        {
        }

        public EffectDefinition(Guid effectId)
        {
            _effectId = effectId;
        }

        public Guid EffectId
        {
            get { return _effectId; }
            set { _effectId = value; }
        }

        public List<Parameter> Parameters
        {
            get
            {
                if (_parameters == null) _parameters = new List<Parameter>();
                return _parameters;
            }
            set { _parameters = value; }
        }
    }
}