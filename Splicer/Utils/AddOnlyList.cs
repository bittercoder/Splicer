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