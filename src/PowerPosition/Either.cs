using System;

namespace PowerPosition
{
    /// <summary>
    /// Interface definition of Either. Either is typically used to represent
    /// the result of a computation. Left is used to indicate success. Right
    /// is used to indicate failure.
    /// </summary>
    /// <typeparam name="TLeft">type of the Left value</typeparam>
    /// <typeparam name="TRight">type of the Right value</typeparam>
    public interface IEither<out TLeft, out TRight>
    {
        /// <summary>
        /// Check the type of the value held and invoke the matching handler function
        /// </summary>
        /// <typeparam name="T">the return type of the handler functions</typeparam>
        /// <param name="ofLeft">handler for the Left type</param>
        /// <param name="ofRight">handler for the Right type</param>
        /// <returns>the value returned by the invoked handler function</returns>
        T Case<T>(Func<TLeft, T> ofLeft, Func<TRight, T> ofRight);

        /// <summary>
        /// Check the type of the value held and invoke the matching handler function
        /// </summary>
        /// <param name="ofLeft">handler for the Left type</param>
        /// <param name="ofRight">handler for the Right type</param>
        void Case(Action<TLeft> ofLeft, Action<TRight> ofRight);
    }

    /// <summary>
    /// Static helper class for Either
    /// </summary>
    public static class Either
    {
        private sealed class LeftImpl<TLeft, TRight> : IEither<TLeft, TRight>
        {
            private readonly TLeft _value;

            public LeftImpl(TLeft value)
            {
                _value = value;
            }

            public T Case<T>(Func<TLeft, T> ofLeft, Func<TRight, T> ofRight)
            {
                if (ofLeft == null)
                    throw new ArgumentNullException("ofLeft");

                return ofLeft(_value);
            }

            public void Case(Action<TLeft> ofLeft, Action<TRight> ofRight)
            {
                if (ofLeft == null)
                    throw new ArgumentNullException("ofLeft");

                ofLeft(_value);
            }
        }

        private sealed class RightImpl<TLeft, TRight> : IEither<TLeft, TRight>
        {
            private readonly TRight _value;

            public RightImpl(TRight value)
            {
                _value = value;
            }

            public T Case<T>(Func<TLeft, T> ofLeft, Func<TRight, T> ofRight)
            {
                if (ofRight == null)
                    throw new ArgumentNullException("ofRight");

                return ofRight(_value);
            }

            public void Case(Action<TLeft> ofLeft, Action<TRight> ofRight)
            {
                if (ofRight == null)
                    throw new ArgumentNullException("ofRight");

                ofRight(_value);
            }
        }

        /// <summary>
        /// Create an Either with Left value
        /// </summary>
        /// <typeparam name="TLeft">type of the Left value</typeparam>
        /// <typeparam name="TRight">type of the Right value</typeparam>
        /// <param name="value">the value to hold</param>
        /// <returns>an Either with the specified Left value</returns>
        public static IEither<TLeft, TRight> Left<TLeft, TRight>(TLeft value)
        {
            return new LeftImpl<TLeft, TRight>(value);
        }

        /// <summary>
        /// Create an Either with Right value
        /// </summary>
        /// <typeparam name="TLeft">type of the Left value</typeparam>
        /// <typeparam name="TRight">type of the Right value</typeparam>
        /// <param name="value">the value to hold</param>
        /// <returns>an Either with the specified Right value</returns>
        public static IEither<TLeft, TRight> Right<TLeft, TRight>(TRight value)
        {
            return new RightImpl<TLeft, TRight>(value);
        }

        /// <summary>
        /// Create an Either with the specified value
        /// </summary>
        /// <typeparam name="TLeft">type of the Left value</typeparam>
        /// <typeparam name="TRight">type of the Right value</typeparam>
        /// <param name="value">the value to hold</param>
        /// <returns>an Either with the specified value</returns>
        public static IEither<TLeft, TRight> Create<TLeft, TRight>(TLeft value)
        {
            return new LeftImpl<TLeft, TRight>(value);
        }

        /// <summary>
        /// Create an Either with the specified value
        /// </summary>
        /// <typeparam name="TLeft">type of the Left value</typeparam>
        /// <typeparam name="TRight">type of the Right value</typeparam>
        /// <param name="value">the value to hold</param>
        /// <returns>an Either with the specified value</returns>
        public static IEither<TLeft, TRight> Create<TLeft, TRight>(TRight value)
        {
            return new RightImpl<TLeft, TRight>(value);
        }
    }
}