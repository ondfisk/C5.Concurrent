using System.Threading;

namespace C5.Concurrent
{
    /// <summary>
    /// An object reference that may be updated atomically.
    /// The class is derived from java.util.concurrent.atomic.AtomicReference by Doug Lea.
    /// </summary>
    /// <typeparam name="T">The type of object referred to by this reference</typeparam>
    public class AtomicReference<T> where T : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicReference&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public AtomicReference(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicReference&lt;T&gt;"/> class.
        /// </summary>
        public AtomicReference()
        {
        }

        private T _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Eventually sets to the given value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void LazySet(T newValue)
        {
            Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>
        /// Atomically sets the value to the given updated value
        /// if the current value <tt>==</tt> the expected value.
        /// </summary>
        /// <param name="expected">The expected value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        /// True if successful. False return indicates that the actual value was not equal to the expected value.
        /// </returns>
        public bool CompareAndSet(T expected, T newValue)
        {
            return Interlocked.CompareExchange(ref _value, newValue, expected) == expected;
        }

        /// <summary>
        /// Atomically sets to the given value and returns the old value.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <returns>The previous value.</returns>
        public T GetAndSet(T newValue)
        {
            return Interlocked.Exchange(ref _value, newValue);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("AtomicReference<{0}>", typeof(T));
        }
    }
}
