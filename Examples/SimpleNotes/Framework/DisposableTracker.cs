using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SimpleNotes.Framework
{
    public static class DisposableTrackerExtensions
    {
        public static T TrackWith<T>(this T value, DisposableTracker tracker) where T : IDisposable
        {
            return tracker.Track(value);
        }
    }

    /// <summary>
    /// Tracks <see cref="IDisposable" /> and disposes them all when disposed.
    /// </summary>
    /// <remarks>
    /// This looks somewhat threadsafe, but it's not.
    /// There's a race condition where an item could be added after it's disposed,
    /// or things could be disposed multiple times.
    /// As long as you keep it in one thread it's fine.
    /// </remarks>
    public sealed class DisposableTracker : IDisposable
    {
        private readonly ConcurrentQueue<IDisposable> mDisposables = new ConcurrentQueue<IDisposable>();
        private bool mIsDisposed;

        public void Dispose()
        {
            if (mIsDisposed)
            {
                return;
            }
            mIsDisposed = true;

            var errors = new List<Exception>();

            IDisposable disposable;
            while (mDisposables.TryDequeue(out disposable))
            {
                try
                {
                    disposable?.Dispose();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            if (errors.Any())
            {
                throw new AggregateException(errors);
            }
        }

        public T Track<T>(T disposable)
            where T : IDisposable
        {
            if (mIsDisposed)
            {
                disposable.Dispose();
            }
            else
            {
                mDisposables.Enqueue(disposable);
            }

            return disposable;
        }
    }
}
