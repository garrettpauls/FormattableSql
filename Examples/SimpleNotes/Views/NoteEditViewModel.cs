using ReactiveUI;
using SimpleNotes.Models;
using System;
using System.Reactive;

namespace SimpleNotes.Views
{
    public sealed class NoteEditViewModel : ReactiveObject, IRoutableViewModel
    {
        private readonly Note mNote;
        private readonly RoutingState mRouter;

        public NoteEditViewModel(IScreen hostScreen, RoutingState router, Note note)
        {
            HostScreen = hostScreen;
            mRouter = router;
            mNote = note;

            DoneCommand = ReactiveCommand.Create();
            DoneCommand.Subscribe(_Done);
        }

        public ReactiveCommand<object> DoneCommand { get; }

        public IScreen HostScreen { get; }

        public string UrlPathSegment => "NoteEditViewModel";

        private void _Done(object _)
        {
            mRouter.NavigateBack.Execute(Unit.Default);
        }
    }
}
