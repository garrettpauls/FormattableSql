using DynamicData;
using DynamicData.ReactiveUI;
using ReactiveUI;
using SimpleNotes.Framework;
using SimpleNotes.Framework.Reactive;
using SimpleNotes.Models;
using SimpleNotes.Services;
using System;
using System.Reactive.Linq;

namespace SimpleNotes.Views
{
    public sealed class NoteListViewModel : ReactiveObject, IDisposable, IRoutableViewModel
    {
        private readonly DisposableTracker mDisposables = new DisposableTracker();
        private readonly ReactiveList<NoteViewModel> mNotes = new NoRangeReactiveList<NoteViewModel>();
        private readonly NotesService mNotesService;
        private readonly RoutingState mRouter;
        private string mSearchText = "";

        public NoteListViewModel(IScreen hostScreen, NotesService notesService, RoutingState router)
        {
            HostScreen = hostScreen;
            mNotesService = notesService;
            mRouter = router;

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

            AddCommand = ReactiveCommand.Create();
            AddCommand.Subscribe(_Add);

            EditCommand = ReactiveCommandG.Create<Note>();
            EditCommand.Subscribe(_Edit);

            DeleteCommand = ReactiveCommandG.Create<Note>();
            DeleteCommand.Subscribe(_Delete);
        }

        public ReactiveCommand<object> AddCommand { get; }
        public ReactiveCommand<Note> DeleteCommand { get; }
        public ReactiveCommand<Note> EditCommand { get; }
        public IScreen HostScreen { get; }
        public ReactiveList<NoteViewModel> Notes => mNotes;

        public string SearchText
        {
            get { return mSearchText; }
            set { this.RaiseAndSetIfChanged(ref mSearchText, value); }
        }

        public string UrlPathSegment => "NoteListView";

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

        private void _Add(object _)
        {
            var note = new Note
            {
                Title = "New note"
            };
            mNotesService.Add(note);
            EditCommand.Execute(note);
        }

        private void _Delete(Note note)
        {
            if (note != null)
            {
                mNotesService.Remove(note);
            }
        }

        private void _Edit(Note note)
        {
            if (note != null)
            {
                mRouter.Navigate.Execute(new NoteEditViewModel(HostScreen, mRouter, note));
            }
        }
    }
}
