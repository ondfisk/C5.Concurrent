using System;

namespace C5.Concurrent
{
    public class Shared<T> : IShared<T>
    {
        public bool CompareAndSet(T expect, T update)
        {
            throw new NotImplementedException();
        }

        public T Get()
        {
            throw new NotImplementedException();
        }

        public T GetAndSet(T value)
        {
            throw new NotImplementedException();
        }

        public T Reduce(T value, Action<T> action)
        {
            throw new NotImplementedException();
        }

        public void Set(T value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a string version of this reduction variable.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public bool WeakCompareAndSet(T expect, T update)
        {
            throw new NotImplementedException();
        }
    }
}
