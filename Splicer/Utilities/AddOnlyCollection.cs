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
using System.Collections;
using System.Collections.Generic;

namespace Splicer.Utilities
{
    public class AddOnlyCollection<T> : IEnumerable<T>, IEnumerable
    {
        private readonly List<T> _items;

        public AddOnlyCollection()
        {
            _items = new List<T>();
        }

        public T this[int index]
        {
            get { return _items[index]; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _items).GetEnumerator();
        }

        #endregion

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        internal void Add(T item)
        {
            _items.Add(item);
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public bool Exists(Predicate<T> match)
        {
            return _items.Exists(match);
        }

        public T Find(Predicate<T> match)
        {
            return _items.Find(match);
        }

        public IList<T> FindAll(Predicate<T> match)
        {
            return _items.FindAll(match);
        }
    }
}