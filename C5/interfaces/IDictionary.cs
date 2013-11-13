using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace C5.interfaces
{
    [ContractClassFor(typeof(IDictionary<,>))]
    abstract class DictionaryContract<K, V> : IDictionary<K, V>
    {
        public abstract IEnumerator<KeyValuePair<K, V>> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract string ToString(string format, IFormatProvider formatProvider);
        public abstract bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
        public abstract EventTypeEnum ListenableEvents { get; }
        public abstract EventTypeEnum ActiveEvents { get; }
        public abstract event CollectionChangedHandler<KeyValuePair<K, V>> CollectionChanged;
        public abstract event CollectionClearedHandler<KeyValuePair<K, V>> CollectionCleared;
        public abstract event ItemsAddedHandler<KeyValuePair<K, V>> ItemsAdded;
        public abstract event ItemInsertedHandler<KeyValuePair<K, V>> ItemInserted;
        public abstract event ItemsRemovedHandler<KeyValuePair<K, V>> ItemsRemoved;
        public abstract event ItemRemovedAtHandler<KeyValuePair<K, V>> ItemRemovedAt;
        public abstract bool IsEmpty { get; }
        public abstract int Count { get; }
        public abstract Speed CountSpeed { get; }
        public abstract void CopyTo(KeyValuePair<K, V>[] array, int index);
        public abstract KeyValuePair<K, V>[] ToArray();
        public abstract void Apply(Action<KeyValuePair<K, V>> action);
        public abstract bool Exists(Func<KeyValuePair<K, V>, bool> predicate);
        public abstract bool Find(Func<KeyValuePair<K, V>, bool> predicate, out KeyValuePair<K, V> item);
        public abstract bool All(Func<KeyValuePair<K, V>, bool> predicate);
        public abstract KeyValuePair<K, V> Choose();
        public abstract IEnumerable<KeyValuePair<K, V>> Filter(Func<KeyValuePair<K, V>, bool> filter);
        public abstract IEqualityComparer<K> EqualityComparer { get; }
        public abstract V this[K key] { get; set; }
        public abstract bool IsReadOnly { get; }
        public abstract ICollectionValue<K> Keys { get; }
        public abstract ICollectionValue<V> Values { get; }
        public abstract Func<K, V> Func { get; }
        public abstract void Add(K key, V val);
        public abstract void AddAll<U, W>(IEnumerable<KeyValuePair<U, W>> entries)
            where U : K
            where W : V;
        public abstract Speed ContainsSpeed { get; }
        public abstract bool ContainsAll<H>(IEnumerable<H> items) where H : K;
        public abstract bool Remove(K key);
        public abstract bool Remove(K key, out V val);
        public abstract void Clear();

        [Pure]
        public bool Contains(K key)
        {
            throw new NotImplementedException();
        }

        public abstract bool Find(ref K key, out V val);
        public abstract bool Update(K key, V val);
        public abstract bool Update(K key, V val, out V oldval);
        public abstract bool FindOrAdd(K key, ref V val);
        public abstract bool UpdateOrAdd(K key, V val);
        public abstract bool UpdateOrAdd(K key, V val, out V oldval);
        public abstract bool Check();
    }
}
