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

using System.Globalization;
using DirectShowLib;
using Splicer.Properties;

namespace Splicer.Utilities
{
    /// <summary>
    /// Represents query information for a single ping
    /// </summary>
    public sealed class PinQueryInfo
    {
        private readonly PinDirection _direction;
        private readonly string _name;
        private readonly string _queryId;

        public PinQueryInfo(PinDirection direction, string name, string queryId)
        {
            _direction = direction;
            _name = name;
            _queryId = queryId;
        }

        public PinDirection Direction
        {
            get { return _direction; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string QueryId
        {
            get { return _queryId; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, Resources.PinQueryInfoTemplate, Direction, Name, QueryId);
        }
    }
}