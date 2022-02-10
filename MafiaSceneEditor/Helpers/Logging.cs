using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAMSE.Helpers
{
    public class Logging : IList
    {
        private IList _internalList;
        public object this[int index] { get => _internalList[index]; set => _internalList[index] = value; }

        public int Count => _internalList.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        public Logging(IList list)
        {
            _internalList = list;
        }

        public void Add(string item)
        {
            _internalList.Add($"{DateTime.Now}: {item}");
        }

        public void Clear()
        {
            _internalList.Clear();
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(string item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, string item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int Add(object item)
        {
            _internalList.Add($"{DateTime.Now}: {item}");
            return _internalList.Count;
        }

        public bool Contains(object item)
        {
            return _internalList.Contains(item);
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }
    }
}
