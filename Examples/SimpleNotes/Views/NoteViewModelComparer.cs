using System;
using System.Collections.Generic;

namespace SimpleNotes.Views
{
    public sealed class NoteViewModelComparer : IComparer<NoteViewModel>
    {
        public int Compare(NoteViewModel x, NoteViewModel y)
        {
            return StringComparer.OrdinalIgnoreCase.Compare(
                x?.Model?.Title,
                y?.Model?.Title);
        }
    }
}
