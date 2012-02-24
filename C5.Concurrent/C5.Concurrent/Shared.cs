using System;

namespace C5.Concurrent
{
    /// <summary>
    /// Shared provides a reduction variable for a value of a reference type
    /// Shared is multiple thread safe. The methods use lock-free atomic compare-and-set.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Shared<T> : IShared<T>
    {
        private T _value;

        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shared&lt;T&gt;"/> class.
        /// </summary>
        public Shared()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Shared&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="value">The initial value.</param>
        public Shared(T value)
        {
            _value = value;
        }

        public bool CompareAndSet(T expect, T update)
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

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Shared<{0}>", typeof(T));
        }

        public bool WeakCompareAndSet(T expect, T update)
        {
            throw new NotImplementedException();
        }
    }
}
