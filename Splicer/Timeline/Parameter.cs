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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Xml.Serialization;

namespace Splicer.Timeline
{
    public class Parameter
    {
        private Collection<Interval> _intervals;

        public Parameter()
        {
        }

        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public Parameter(string name, double value)
        {
            Name = name;
            Value = value.ToString(CultureInfo.InvariantCulture);
        }

        public Parameter(string name, bool value)
        {
            Name = name;
            Value = value.ToString(CultureInfo.InvariantCulture);
        }

        public Parameter(string name, long value)
        {
            Name = name;
            Value = value.ToString(CultureInfo.InvariantCulture);
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

        public string Value { get; set; }

        public string Name { get; set; }

        [XmlArray("Intervals")]
        [XmlArrayItem("Interval")]
        public Collection<Interval> Intervals
        {
            get
            {
                if (_intervals == null) _intervals = new Collection<Interval>();
                return _intervals;
            }
        }

        public int DispId { get; set; }
    }
}