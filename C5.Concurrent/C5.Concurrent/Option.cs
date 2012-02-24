using System;

namespace C5.Concurrent
{
    public sealed class Option<T>
    {
        /// <summary>
        /// Value defining whether the option is some (using field to make property readonly)
        /// </summary>
        private readonly bool _isSome;

        /// <summary>
        /// Returns true if the option has a value that is not None.
        /// </summary>
        public bool IsSome
        {
            get { return _isSome; }
        }

        /// <summary>
        /// Value defining whether the option is none (using field to make property readonly)
        /// </summary>
        private readonly bool _isNone;

        /// <summary>
        /// Returns true if the option has the None value.
        /// </summary>
        public bool IsNone
        {
            get { return _isNone; }
        }

        /// <summary>
        /// The value of the option
        /// </summary>
        private readonly T _value;

        /// <summary>
        /// Returns the underlying value, or throws a NullReferenceException if the value is None.
        /// </summary>
        public T Value
        {
            get
            {
                if (IsSome)
                {
                    return _value;
                }
                throw new NullReferenceException();
            }
        }

        /// <summary>
        /// Creates an option which is None.
        /// </summary>
        private Option()
        {
            _isNone = true;
        }

        /// <summary>
        /// Creates an option that has a value which is not None.
        /// </summary>
        private Option(T value)
        {
            _value = value;
            _isSome = true;
        }

        /// <summary>
        /// Creates an option that has a value which is not None.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Option<T> Some(T value)
        {
            return new Option<T>(value);
        }

        /// <summary>
        /// Creates an option value that has the None value.
        /// </summary>
        public static Option<T> None
        {
            get { return new Option<T>(); }
        }
    }
}