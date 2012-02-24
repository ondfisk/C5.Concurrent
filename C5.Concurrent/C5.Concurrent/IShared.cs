using System;

namespace C5.Concurrent
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IShared<T>
    {
        /// <summary>
        /// Atomically set this reduction variable to the given updated value if the current value equals the expected value.
        /// </summary>
        /// <param name="expect"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        bool CompareAndSet(T expect, T update);

        /// <summary>
        /// Set this reduction variable to the given value and return the previous value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        T GetAndSet(T value);

        /// <summary>
        /// Combine this reduction variable with the given value using the given operation.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        T Reduce(T value, Action<T> action);

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        T Value { get; set; }

        /// <summary>
        /// Returns a string version of this reduction variable.
        /// </summary>
        /// <returns></returns>
        string ToString();

        /// <summary>
        /// Atomically set this reduction variable to the given updated value if the current value equals the expected value.
        /// </summary>
        /// <param name="expect"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        bool WeakCompareAndSet(T expect, T update);
    }
}