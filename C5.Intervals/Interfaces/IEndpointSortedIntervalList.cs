using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace C5.Intervals
{
    [ContractClass(typeof(EndpointSortedIntervalListContract<,>))]
    public interface IEndpointSortedIntervalList<I, T> : IEnumerable<I>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        /// <summary>
        /// The value indicates the type of asymptotic complexity in terms of the indexer of
        /// this collection. This is to allow generic algorithms to alter their behaviour 
        /// for collections that provide good performance when applied to either random or
        /// sequencial access.
        /// </summary>
        /// <value>A characterization of the speed of lookup operations.</value>
        [Pure]
        Speed IndexingSpeed { get; }

        /// <summary>
        /// Get the number of intervals in the list.
        /// </summary>
        /// <value>The number of intervals in the list.</value>
        [Pure]
        int Count { get; }

        /// <summary>
        /// <para>
        /// Get the interval at index <paramref name="i"/>. First interval has index 0.
        /// </para>
        /// <para>
        /// The result is equal to <c>coll.Skip(i).First()</c>.
        /// </para>
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The <c>i</c>'th interval.</returns>
        [Pure]
        I this[int i] { get; }

        /// <summary>
        /// Get the first interval in the list.
        /// </summary>
        /// <value>The first intervals in the list.</value>
        [Pure]
        I First { get; }

        /// <summary>
        /// Get the last interval in the list.
        /// </summary>
        /// <value>The last intervals in the list.</value>
        [Pure]
        I Last { get; }

        #endregion

        #region Find

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int Find(IInterval<T> query);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int FindFirst(IInterval<T> query);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        int FindLast(IInterval<T> query);

        #endregion

        #region Enumerable

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IEnumerable<I> EnumerateFromIndex(int index);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inclusiveFrom"></param>
        /// <param name="exclusiveTo"></param>
        /// <returns></returns>
        IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        IEnumerable<I> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Extensible

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        bool Add(I interval);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        bool Remove(I interval);

        /// <summary>
        /// 
        /// </summary>
        void Clear();

        #endregion
    }

    [ContractClassFor(typeof (IEndpointSortedIntervalList<,>))]
    abstract class EndpointSortedIntervalListContract<I, T> : IEndpointSortedIntervalList<I, T>
        where I : class, IInterval<T>
        where T : IComparable<T>
    {
        #region Properties

        /// <inheritdoc/>
        public Speed IndexingSpeed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public I this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        /// <inheritdoc/>
        public I First
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public I Last
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Find

        /// <inheritdoc/>
        public int Find(IInterval<T> query)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int FindFirst(IInterval<T> query)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public int FindLast(IInterval<T> query)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<I> EnumerateBackwardsFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerator<I> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        
        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool Add(I interval)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool Remove(I interval)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Clear()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
