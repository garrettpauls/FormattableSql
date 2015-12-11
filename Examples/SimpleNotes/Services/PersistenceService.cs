using DynamicData;
using FormattableSql.Core;
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
    CreatedInstant DATETIME    NOT NULL
                               CONSTRAINT DF_Note_CreatedInstant DEFAULT (date('now') ),
    UpdatedInstant DATETIME    NOT NULL
                               CONSTRAINT DF_Note_UpdatedInstant DEFAULT (date('now') )
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
                async data => new Note
                {
                    Id = await data.GetValueAsync<long>("Id"),
                    Text = await data.GetValueAsync<string>("Content"),
                    Title = await data.GetValueAsync<string>("Title")
                });

            foreach (var note in notes)
            {
                mNotesService.Add(note);
            }

            mIsInitialized.OnNext(true);
            mIsInitialized.OnCompleted();
        }

        private async Task _AddAsync(Note note)
        {
            var id = await mSql.ExecuteScalarAsync<long>($@"
insert into Note
(Title, Content)
values
({note.Title}, {note.Text})

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
    }
}
