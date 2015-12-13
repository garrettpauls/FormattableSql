using DynamicData;
using SimpleNotes.Models;

namespace SimpleNotes.Services
{
    public sealed class NotesService
    {
        private readonly SourceList<Note> mNotes = new SourceList<Note>();

        public IObservableList<Note> Notes => mNotes;

        public void Add(Note note)
        {
            mNotes.Add(note);
        }

        public void Remove(Note note)
        {
            mNotes.Remove(note);
        }
    }
}
