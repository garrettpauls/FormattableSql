using DynamicData;
using ReactiveUI;
using System;

namespace SimpleNotes.Models
{
    public sealed class Note : ReactiveObject, IKey<long>, IEquatable<Note>
    {
        private long mId;
        private string mText;
        private string mTitle;
        private string mUser;

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

        public string User
        {
            get { return mUser; }
            set { this.RaiseAndSetIfChanged(ref mUser, value); }
        }

        public bool Equals(Note other)
        {
            return Id.Equals(other?.Id);
        }
    }
}
