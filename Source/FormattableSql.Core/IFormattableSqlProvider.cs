using FormattableSql.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core
{
    public interface IFormattableSqlProvider
    {
        event CommandPreparedEventHandler CommandPrepared;

        Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken);

        Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, Task> handleRowAsync,
            CancellationToken cancellationToken);
    }
}
