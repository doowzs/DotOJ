using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace Shared.Generics
{
    public class LruCache<TKey, TValue>
    {
        private readonly AsyncReaderWriterLock _lock = new();
        private readonly Dictionary<TKey, LinkedListNode<LruCacheItem>> _cache = new();
        private readonly LinkedList<LruCacheItem> _list = new();

        public int Capacity { get; }
        private Action<TValue> Dispose { get; }

        public LruCache(int capacity, Action<TValue> dispose = null)
        {
            Capacity = capacity;
            Dispose = dispose;
        }

        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            using var locked = await _lock.ReaderLockAsync();
            return _cache.ContainsKey(key);
        }

        public async Task<(bool, TValue)> TryGetValueAsync(TKey key)
        {
            using var locked = await _lock.ReaderLockAsync();
            if (_cache.TryGetValue(key, out var node))
            {
                _list.Remove(node);
                _list.AddLast(node);
                return (true, node.Value.Value);
            }
            return (false, default);
        }

        public async Task<bool> TryAdd(TKey key, TValue value)
        {
            using var locked = await _lock.WriterLockAsync();
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

        public async Task<(bool, TValue)> TryRemove(TKey key)
        {
            using var locked = await _lock.WriterLockAsync();
            if (_cache.Remove(key, out var node))
            {
                _list.Remove(node);
                return (true, node.Value.Value);
            }
            return (false, default);
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