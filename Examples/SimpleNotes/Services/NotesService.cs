using DynamicData;
using SimpleNotes.Models;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace SimpleNotes.Services
{
    public sealed class NotesService
    {
        private readonly SourceList<Note> mNotes = new SourceList<Note>();

        public NotesService()
        {
            Observable
                .Generate(1, x => true, x => x + 1, x => new Note
                {
                    Title = $"Note {x}",
                    Text = $"Some text for note {x}",
                    User = Environment.UserName
                }, x => TimeSpan.FromSeconds(1), Scheduler.Default)
                .Take(5)
                .Subscribe(note => mNotes.Add(note));
        }

        public IObservableList<Note> Notes => mNotes;
    }
}
