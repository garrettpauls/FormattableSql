using ReactiveUI;
using SimpleNotes.Framework;
using System;
using System.Threading.Tasks;

namespace SimpleNotes.Services
{
    public sealed class ServiceManager : IDisposable
    {
        private readonly DisposableTracker mDisposables = new DisposableTracker();

        public ServiceManager()
        {
            NotesService = new NotesService();
            PersistenceService = new PersistenceService(NotesService).TrackWith(mDisposables);
            Router = new RoutingState();
        }

        public NotesService NotesService { get; }
        public PersistenceService PersistenceService { get; }
        public RoutingState Router { get; }

        public void Dispose()
        {
            mDisposables.Dispose();
        }

        public async Task InitializeAsync()
        {
            await PersistenceService.InitializeAsync();
        }
    }
}
