using DynamicData;
using DynamicData.ReactiveUI;
using ReactiveUI;
using SimpleNotes.Framework;
using SimpleNotes.Framework.Reactive;
using SimpleNotes.Services;
using System;
using System.Reactive.Linq;

namespace SimpleNotes.Views
{
    public sealed class ShellViewModel : ReactiveObject, IDisposable
    {
        private readonly DisposableTracker mDisposables = new DisposableTracker();
        private readonly ReactiveList<NoteViewModel> mNotes = new NoRangeReactiveList<NoteViewModel>();
        private string mSearchText = "";

        public ShellViewModel() : this(new NotesService())
        {
        }

        public ShellViewModel(NotesService notesService)
        {
            var filter = this
                .WhenAnyValue(x => x.SearchText)
                .Select(_BuildFilter);

            notesService
                .Notes.Connect()
                .Transform(note => new NoteViewModel(note))
                .Filter(filter)
                .Sort(new NoteViewModelComparer())
                .SubscribeOn(RxApp.MainThreadScheduler)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(mNotes)
                .DisposeMany()
                .Subscribe()
                .TrackWith(mDisposables);
        }

        public ReactiveList<NoteViewModel> Notes => mNotes;

        public string SearchText
        {
            get { return mSearchText; }
            set { this.RaiseAndSetIfChanged(ref mSearchText, value); }
        }

        public void Dispose()
        {
            mDisposables.Dispose();
        }

        private static Func<NoteViewModel, bool> _BuildFilter(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return note => true;
            }

            return note => (note.Model.Title?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }
    }
}
