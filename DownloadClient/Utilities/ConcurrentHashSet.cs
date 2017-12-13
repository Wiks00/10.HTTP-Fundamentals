using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DownloadClient.Utilities
{
    public class ConcurrentHashSet<T> : IDisposable , IEnumerable<T>
    {
        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly HashSet<T> hashSet = new HashSet<T>();

        public ConcurrentHashSet()
        {
        }

        public ConcurrentHashSet(IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                Add(item);
            }
        }

        public bool Add(T item)
        {
            rwLock.EnterWriteLock();
            try
            {
                return hashSet.Add(item);
            }
            finally
            {
                if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            rwLock.EnterWriteLock();
            try
            {
                hashSet.Clear();
            }
            finally
            {
                if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
            }
        }

        public bool Contains(T item)
        {
            rwLock.EnterReadLock();
            try
            {
                return hashSet.Contains(item);
            }
            finally
            {
                if (rwLock.IsReadLockHeld) rwLock.ExitReadLock();
            }
        }

        public bool Remove(T item)
        {
            rwLock.EnterWriteLock();
            try
            {
                return hashSet.Remove(item);
            }
            finally
            {
                if (rwLock.IsWriteLockHeld) rwLock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    return hashSet.Count;
                }
                finally
                {
                    if (rwLock.IsReadLockHeld) rwLock.ExitReadLock();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            rwLock.EnterReadLock();
            try
            {
                return hashSet.GetEnumerator();
            }
            finally
            {
                if (rwLock.IsReadLockHeld) rwLock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                rwLock?.Dispose();
            }
        }

        ~ConcurrentHashSet()
        {
            Dispose(false);
        }
    }
}
