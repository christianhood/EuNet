﻿using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace EuNet.Core
{
    public class ConcurrentObjectPool<T> : IPool
        where T : class, new()
    {
        private ConcurrentQueue<T> _queue;

        private int _alloced;
        private int _total;
        private int _maxCount;

        public ConcurrentObjectPool(int count = 0, int maxCount = 1000)
        {
            _queue = new ConcurrentQueue<T>();
            _maxCount = maxCount;

            for (int i = 0; i < count; ++i)
            {
                T obj = new T();
                _queue.Enqueue(obj);
            }
        }

        public long TotalAllocCount => _total;
        public long AllocCount => _alloced;
        public long PoolingCount => _queue.Count;

        public void Prepare(int count)
        {
            int size = count - _queue.Count;
            if (size <= 0)
                return;

            for (int i = 0; i < count; ++i)
            {
                T obj = new T();
                _queue.Enqueue(obj);
            }
        }

        public virtual T Alloc()
        {
            Interlocked.Increment(ref _total);
            Interlocked.Increment(ref _alloced);

            T obj = null;

            if (_queue.TryDequeue(out obj) == true)
            {
                return obj;
            }

            return new T();
        }

        public virtual void Free(T obj)
        {
            Interlocked.Decrement(ref _alloced);

            if (_queue.Count >= _maxCount)
                return;

            _queue.Enqueue(obj);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"TotalAllocCount : {TotalAllocCount}");
            builder.AppendLine($"AllocCount : {AllocCount}");
            builder.AppendLine($"PoolingCount : {PoolingCount}");

            return builder.ToString();
        }

    }
}
