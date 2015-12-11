using DynamicData;
using NodaTime;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace SimpleNotes.Models
{
    public sealed class Note : ReactiveObject, IKey<long>, IEquatable<Note>, IDisposable
    {
        private readonly IDisposable mUpdateUpdatedSubscription;
        private long mId;
        private string mText;
        private string mTitle;
        private Instant mUpdated;

        public Note(Instant? created = null)
        {
            Created = created ?? SystemClock.Instance.Now;
            Updated = Created;

            mUpdateUpdatedSubscription =
                Changing
                    .Where(args => args.PropertyName != nameof(Updated))
                    .Subscribe(args => Updated = SystemClock.Instance.Now);
        }

        public Instant Created { get; }

        public long Id
        {
            get { return mId; }
            set { this.RaiseAndSetIfChanged(ref mId, value); }
        }

        long IKey<long>.Key => Id;

        public string Text
        {
            get { return mText; }
            set { this.RaiseAndSetIfChanged(ref mText, value); }
        }

        public string Title
        {
            get { return mTitle; }
            set { this.RaiseAndSetIfChanged(ref mTitle, value); }
        }

        public Instant Updated
        {
            get { return mUpdated; }
            set { this.RaiseAndSetIfChanged(ref mUpdated, value); }
        }

        public void Dispose()
        {
            mUpdateUpdatedSubscription.Dispose();
        }

        public bool Equals(Note other)
        {
            return Id.Equals(other?.Id);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
