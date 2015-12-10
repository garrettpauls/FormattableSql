using FormattableSql.Core;
using FormattableSql.Core.Data;
using System;
using System.Data.Common;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Reactive
{
    public sealed class ReactiveFormattableSqlProvider : IReactiveFormattableSqlProvider
    {
        private readonly FormattableSqlProvider mBaseProvider;

        public ReactiveFormattableSqlProvider(FormattableSqlProvider baseProvider)
        {
            mBaseProvider = baseProvider;

            CommandPrepared = Observable.FromEvent<CommandPreparedEventHandler, DbCommand>(
                handler => mBaseProvider.CommandPrepared += handler,
                handler => mBaseProvider.CommandPrepared -= handler);
        }

        public IObservable<DbCommand> CommandPrepared { get; }

        public IObservable<TResult> Query<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken)
        {
            var subject = new Subject<TResult>();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    // ReSharper disable once AccessToDisposedClosure
                    // Subject should never be disposed before QueryAsync is completed.
                    await mBaseProvider.QueryAsync(
                        query,
                        async row => subject.OnNext(await createResultAsync(row)),
                        cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                catch (Exception ex)
                {
                    subject.OnError(ex);
                }
                finally
                {
                    subject.OnCompleted();
                    subject.Dispose();
                }
            }, cancellationToken).Unwrap();

            return subject;
        }
    }
}
