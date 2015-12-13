using ReactiveUI;
using SimpleNotes.Models;
using System;
using System.Reactive;

namespace SimpleNotes.Views
{
    public sealed class NoteEditViewModel : ReactiveObject, IRoutableViewModel
    {
        private readonly RoutingState mRouter;

        public NoteEditViewModel(IScreen hostScreen, RoutingState router, Note note)
        {
            HostScreen = hostScreen;
            mRouter = router;
            Model = note;

            DoneCommand = ReactiveCommand.Create();
            DoneCommand.Subscribe(_Done);
        }

        public ReactiveCommand<object> DoneCommand { get; }

        public IScreen HostScreen { get; }

        public DateTime LocalCreationInstant
                    => Model.Created.ToDateTimeUtc().ToLocalTime();

        public DateTime LocalUpdateInstant
            => Model.Updated.ToDateTimeUtc().ToLocalTime();

        public Note Model { get; }
        public string UrlPathSegment => "NoteEditViewModel";

        private void _Done(object _)
        {
            mRouter.NavigateBack.Execute(Unit.Default);
        }
    }
}
