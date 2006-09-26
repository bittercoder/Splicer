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
using System.Collections;
using System.Collections.Generic;

namespace Splicer.Utils
{
    public class AddOnlyList<T> : IEnumerable<T>, IEnumerable
    {
        private List<T> _list;

        public AddOnlyList()
        {
            _list = new List<T>();
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public T this[int index]
        {
            get { return _list[index]; }
        }

        #endregion

        #region ICollection<T> Members

        internal void Add(T item)
        {
            _list.Add(item);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _list).GetEnumerator();
        }

        #endregion

        public bool Exists(Predicate<T> match)
        {
            return _list.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return _list.Find(match);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            return _list.FindAll(match);
        }
    }
}