using FormattableSql.Core.Data;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Reactive
{
    public interface IReactiveFormattableSqlProvider
    {
        IObservable<DbCommand> CommandPrepared { get; }

        IObservable<TResult> Query<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken);
    }
}
