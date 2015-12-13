using ReactiveUI;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace SimpleNotes.Framework.Reactive
{
    public static class ReactiveCommandG
    {
        public static ReactiveCommand<T> Create<T>(IObservable<bool> canExecute = null, IScheduler scheduler = null) where T : class
        {
            canExecute = canExecute ?? Observable.Return(true);
            return new ReactiveCommand<T>(canExecute, x => Observable.Return(x as T), scheduler);
        }
    }
}
