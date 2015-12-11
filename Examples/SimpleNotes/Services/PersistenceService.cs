using DynamicData;
using FormattableSql.Core;
using FormattableSql.Core.Data;
using NodaTime;
using ReactiveUI;
using SimpleNotes.Framework;
using SimpleNotes.Framework.Data;
using SimpleNotes.Models;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace SimpleNotes.Services
{
    public sealed class PersistenceService : IDisposable
    {
        private readonly DisposableTracker mDisposables = new DisposableTracker();
        private readonly Subject<bool> mIsInitialized = new Subject<bool>();
        private readonly NotesService mNotesService;

        private readonly FormattableSqlProvider mSql =
            new FormattableSqlProvider(SQLiteSqlProvider.FromFile(
                Path.Combine(Environment.CurrentDirectory, "data.sqlite3")));

        public PersistenceService(NotesService notesService)
        {
            mNotesService = notesService;

            mNotesService.Notes.Connect()
                         .SkipUntil(mIsInitialized)
                         .ForEachItemChange(_HandleNotesChanged)
                         .Subscribe()
                         .TrackWith(mDisposables);

            mNotesService.Notes.Connect()
                         .SkipUntil(mIsInitialized)
                         .Transform(note => new NoteChangeMonitor(note, this))
                         .DisposeMany()
                         .Subscribe()
                         .TrackWith(mDisposables);
        }

        public void Dispose()
        {
            mDisposables.Dispose();
        }

        public async Task InitializeAsync()
        {
            await mSql.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS Note (
    Id             INTEGER     PRIMARY KEY AUTOINCREMENT
                               NOT NULL,
    Title          TEXT        NOT NULL,
    Content        TEXT        NOT NULL,
    CreatedInstant DATETIME    NOT NULL,
    UpdatedInstant DATETIME    NOT NULL
);

CREATE TABLE IF NOT EXISTS NoteHistory (
    NoteId         INTEGER     NOT NULL
                               CONSTRAINT FK_NoteHistory_Note REFERENCES Note (Id) ON DELETE CASCADE,
    Title          TEXT        NOT NULL,
    Content        TEXT        NOT NULL,
    CreatedInstant DATETIME    NOT NULL,
    PRIMARY KEY (
        NoteId,
        CreatedInstant
    )
);
");

            var notes = await mSql.QueryAsync(
                $@"
select Id, Title, Content, CreatedInstant, UpdatedInstant from Note",
                async data =>
                new Note(await _GetInstantAsync(data, "CreatedInstant"))
                {
                    Id = await data.GetValueAsync<long>("Id"),
                    Text = await data.GetValueAsync<string>("Content"),
                    Title = await data.GetValueAsync<string>("Title"),
                    Updated = await _GetInstantAsync(data, "UpdatedInstant")
                });

            foreach (var note in notes)
            {
                mNotesService.Add(note);
            }

            mIsInitialized.OnNext(true);
            mIsInitialized.OnCompleted();
        }

        private static async Task<Instant> _GetInstantAsync(IAsyncDataRecord data, string fieldName)
        {
            var dt = await data.GetValueAsync<DateTime>(fieldName);
            // The read datetime is in UTC, but doesn't have the right kind to convert directly.
            return Instant.FromUtc(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        private async Task _AddAsync(Note note)
        {
            var id = await mSql.ExecuteScalarAsync<long>($@"
insert into Note
(Title, Content, CreatedInstant, UpdatedInstant)
values
({note.Title}, {note.Text}, {note.Created.ToDateTimeUtc()}, {note.Updated.ToDateTimeUtc()})

;select last_insert_rowid()");

            note.Id = id;
        }

        private async void _HandleNotesChanged(ItemChange<Note> change)
        {
            if (change.Reason == ListChangeReason.Add)
            {
                await _AddAsync(change.Current);
            }
            else if (change.Reason == ListChangeReason.Remove)
            {
                await _RemoveAsync(change.Current);
            }
            else if (change.Reason == ListChangeReason.Replace)
            {
                if (change.Previous.HasValue)
                {
                    await _RemoveAsync(change.Previous.Value);
                }

                await _AddAsync(change.Current);
            }
        }

        private Task _RemoveAsync(Note note)
        {
            return mSql.ExecuteAsync($"delete from Note where Id = {note.Id}");
        }

        private Task _SaveChangesAsync(Note note, string propertyName)
        {
            return mSql.ExecuteAsync($@"
update Note
   set Title = {note.Title}
     , Content = {note.Text}
     , UpdatedInstant = {note.Updated.ToDateTimeUtc()}
 where Id = {note.Id}");
        }

        private class NoteChangeMonitor : IDisposable
        {
            private readonly IDisposable mChangedSubscription;
            private readonly Note mNote;
            private readonly PersistenceService mPersistenceService;

            public NoteChangeMonitor(Note note, PersistenceService persistenceService)
            {
                mNote = note;
                mPersistenceService = persistenceService;
                mChangedSubscription = note
                    .Changed
                    .Where(args => args.PropertyName != nameof(Note.Updated))
                    .Subscribe(_OnNoteChanged);
            }

            public void Dispose()
            {
                mChangedSubscription.Dispose();
            }

            private async void _OnNoteChanged(IReactivePropertyChangedEventArgs<IReactiveObject> changed)
            {
                await mPersistenceService._SaveChangesAsync(mNote, changed.PropertyName);
            }
        }
    }
}
