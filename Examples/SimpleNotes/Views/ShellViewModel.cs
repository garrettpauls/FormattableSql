using DynamicData;
using DynamicData.ReactiveUI;
using ReactiveUI;
using SimpleNotes.Framework;
using SimpleNotes.Framework.Reactive;
using SimpleNotes.Models;
using SimpleNotes.Services;
using Splat;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SimpleNotes.Views
{
    public sealed class ShellViewModel : ReactiveObject, IScreen
    {
        public ShellViewModel() : this(App.Services.Router)
        {
        }

        public ShellViewModel(RoutingState router)
        {
            Router = router;

            _RegisterParts();

            Router.Navigate.Execute(new NoteListViewModel(this, App.Services.NotesService, router));
        }

        public RoutingState Router { get; }

        private void _RegisterParts()
        {
            var dependencyResolver = Locator.CurrentMutable;

            dependencyResolver.RegisterConstant(this, typeof(IScreen));
            dependencyResolver.Register(() => new NoteListView(), typeof(IViewFor<NoteListViewModel>));
            dependencyResolver.Register(() => new NoteEditView(), typeof(IViewFor<NoteEditViewModel>));
        }
    }
}
