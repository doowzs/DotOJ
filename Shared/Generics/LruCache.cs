using System;
using System.Collections.Generic;

namespace Shared.Generics
{
    public class LruCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, LinkedListNode<LruCacheItem>> _cache = new();
        private readonly LinkedList<LruCacheItem> _list = new();

        public int Capacity { get; }
        private Action<TValue> Dispose { get; }

        public LruCache(int capacity, Action<TValue> dispose = null)
        {
            Capacity = capacity;
            Dispose = dispose;
        }

        public bool ContainsKey(TKey key)
        {
            lock (_cache)
            {
                return _cache.ContainsKey(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_cache)
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    value = node.Value.Value;
                    _list.Remove(node);
                    _list.AddLast(node);
                    return true;
                }
                value = default;
                return false;
            }
        }

        public bool TryAdd(TKey key, TValue value)
        {
            lock (_cache)
            {
                if (_cache.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    if (_cache.Count >= Capacity)
                    {
                        RemoveFirst();
                    }
                    var item = new LruCacheItem(key, value);
                    var node = new LinkedListNode<LruCacheItem>(item);
                    _list.AddLast(node);
                    _cache.Add(key, node);
                    return true;
                }
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (_cache)
            {
                if (_cache.Remove(key, out var node))
                {
                    value = node.Value.Value;
                    _list.Remove(node);
                    return true;
                }
                value = default;
                return false;
            }
        }

        private void RemoveFirst()
        {
            var node = _list.First;
            if (node is not null)
            {
                _list.RemoveFirst();
                _cache.Remove(node.Value.Key);
                Dispose?.Invoke(node.Value.Value);
            }
        }

        private class LruCacheItem
        {
            public TKey Key { get; }
            public TValue Value { get; }

            public LruCacheItem(TKey k, TValue v)
            {
                Key = k;
                Value = v;
            }
        }
    }
}