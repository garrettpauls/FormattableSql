using FormattableSql.Core.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core
{
    public interface IFormattableSqlProvider
    {
        event FormattableSqlProvider.CommandGeneratedEventHandler CommandGenerated;

        Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, TResult> createResult,
            CancellationToken cancellationToken);
    }
}
