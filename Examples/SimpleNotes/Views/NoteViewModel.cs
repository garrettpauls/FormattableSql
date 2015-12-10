using ReactiveUI;
using SimpleNotes.Models;
using System;

namespace SimpleNotes.Views
{
    public sealed class NoteViewModel : ReactiveObject, IEquatable<NoteViewModel>
    {
        public NoteViewModel(Note model)
        {
            Model = model;
        }

        public Note Model { get; }

        public bool Equals(NoteViewModel other)
        {
            return Model.Equals(other?.Model);
        }
    }
}
