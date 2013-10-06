using System;
using System.Collections;
using SCG = System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.interfaces
{
    [ContractClassFor(typeof(IList<>))]
    abstract class ListContract<T> : IList<T>
    {
        public abstract SCG.IEnumerator<T> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract string ToString(string format, IFormatProvider formatProvider);
        public abstract bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
        public abstract EventTypeEnum ListenableEvents { get; }
        public abstract EventTypeEnum ActiveEvents { get; }
        public abstract event CollectionChangedHandler<T> CollectionChanged;
        public abstract event CollectionClearedHandler<T> CollectionCleared;
        public abstract event ItemsAddedHandler<T> ItemsAdded;
        public abstract event ItemInsertedHandler<T> ItemInserted;
        public abstract event ItemsRemovedHandler<T> ItemsRemoved;
        public abstract event ItemRemovedAtHandler<T> ItemRemovedAt;
        public abstract bool IsEmpty { get; }
        public abstract int Add(object value);
        void IList<T>.Clear()
        {
            throw new NotImplementedException();
        }

        public abstract bool Contains(T item);
        public abstract void CopyTo(T[] array, int index);
        public abstract bool Remove(T item);
        public abstract int IndexOf(T item);
        T IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public abstract void Insert(IList<T> pointer, T item);
        public abstract void InsertFirst(T item);
        public abstract void InsertLast(T item);
        public abstract void InsertAll(int index, SCG.IEnumerable<T> items);
        public abstract IList<T> FindAll(Func<T, bool> filter);
        public abstract IList<V> Map<V>(Func<T, V> mapper);
        public abstract IList<V> Map<V>(Func<T, V> mapper, SCG.IEqualityComparer<V> equalityComparer);
        public abstract T Remove();
        public abstract T RemoveFirst();
        public abstract T RemoveLast();
        public abstract IList<T> View(int start, int count);
        public abstract IList<T> ViewOf(T item);
        public abstract IList<T> LastViewOf(T item);
        public abstract IList<T> Underlying { get; }
        public abstract int Offset { get; }
        public abstract bool IsValid { get; }
        public abstract IList<T> Slide(int offset);
        public abstract IList<T> Slide(int offset, int size);
        public abstract bool TrySlide(int offset);
        public abstract bool TrySlide(int offset, int size);
        public abstract IList<T> Span(IList<T> otherView);
        public abstract void Reverse();
        public abstract bool IsSorted();
        public abstract bool IsSorted(SCG.IComparer<T> comparer);
        public abstract void Sort();
        public abstract void Sort(SCG.IComparer<T> comparer);
        public abstract void Shuffle();
        public abstract void Shuffle(Random rnd);
        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        public abstract bool Contains(object value);
        public abstract int IndexOf(object value);
        public abstract void Insert(int index, object value);
        public abstract void Remove(object value);
        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        bool IList<T>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        T IList<T>.this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        bool IList<T>.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        public abstract T First { get; }
        public abstract T Last { get; }
        public abstract bool FIFO { get; set; }

        bool IList.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        bool IList.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        object IList.this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        void ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        public abstract void RetainAll(SCG.IEnumerable<T> items);
        public abstract int ContainsCount(T item);
        public abstract ICollectionValue<T> UniqueItems();
        public abstract ICollectionValue<KeyValuePair<T, int>> ItemMultiplicities();
        public abstract bool ContainsAll(SCG.IEnumerable<T> items);
        public abstract bool Find(ref T item);
        public abstract bool FindOrAdd(ref T item);
        public abstract bool Update(T item);
        public abstract bool Update(T item, out T olditem);
        public abstract bool UpdateOrAdd(T item);
        public abstract bool UpdateOrAdd(T item, out T olditem);

        int IList<T>.Count
        {
            get { throw new NotImplementedException(); }
        }

        public abstract bool Remove(T item, out T removeditem);
        public abstract void RemoveAllCopies(T item);
        public abstract void RemoveAll(SCG.IEnumerable<T> items);
        public abstract int GetUnsequencedHashCode();
        public abstract bool UnsequencedEquals(ICollection<T> otherCollection);
        public abstract void CopyTo(Array array, int index);

        int ICollection.Count
        {
            get { throw new NotImplementedException(); }
        }

        public abstract bool IsSynchronized { get; }
        public abstract object SyncRoot { get; }

        int ICollection<T>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public abstract Speed ContainsSpeed { get; }

        int SCG.ICollection<T>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool SCG.ICollection<T>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        int ICollectionValue<T>.Count
        {
            get { throw new NotImplementedException(); }
        }

        public abstract Speed CountSpeed { get; }

        bool IList<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void SCG.ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        void SCG.ICollection<T>.Clear()
        {
            throw new NotImplementedException();
        }

        bool SCG.ICollection<T>.Contains(T item)
        {
            throw new NotImplementedException();
        }
        void SCG.ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public abstract T[] ToArray();
        public abstract void Apply(Action<T> action);
        public abstract bool Exists(Func<T, bool> predicate);
        public abstract bool Find(Func<T, bool> predicate, out T item);
        public abstract bool All(Func<T, bool> predicate);
        public abstract T Choose();
        public abstract SCG.IEnumerable<T> Filter(Func<T, bool> filter);

        bool IExtensible<T>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public abstract bool AllowsDuplicates { get; }
        public abstract SCG.IEqualityComparer<T> EqualityComparer { get; }
        public abstract bool DuplicatesByCounting { get; }
        public abstract bool Add(T item);
        public abstract void AddAll(SCG.IEnumerable<T> items);
        public abstract bool Check();
        IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards()
        {
            throw new NotImplementedException();
        }

        public abstract bool FindLast(Func<T, bool> predicate, out T item);
        IDirectedCollectionValue<T> IDirectedCollectionValue<T>.Backwards()
        {
            throw new NotImplementedException();
        }

        public abstract EnumerationDirection Direction { get; }
        public abstract int GetSequencedHashCode();
        public abstract bool SequencedEquals(ISequenced<T> otherCollection);
        void SCG.IList<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        T SCG.IList<T>.this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        T IIndexed<T>.this[int index]
        {
            get { throw new NotImplementedException(); }
        }

        public abstract Speed IndexingSpeed { get; }

        IDirectedCollectionValue<T> IIndexed<T>.this[int start, int count]
        {
            get { throw new NotImplementedException(); }
        }

        int IList<T>.IndexOf(T item)
        {
            throw new NotImplementedException();
        }
        public abstract void Insert(int index, T item);
        public abstract int LastIndexOf(T item);
        public abstract int FindIndex(Func<T, bool> predicate);
        public abstract int FindLastIndex(Func<T, bool> predicate);
        T IIndexed<T>.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public abstract void RemoveInterval(int start, int count);
        public abstract void Dispose();
    }
}
