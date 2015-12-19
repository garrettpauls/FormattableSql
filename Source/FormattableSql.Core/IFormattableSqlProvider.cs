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

        Task<int> ExecuteAsync(
            FormattableString sql,
            CancellationToken cancellationToken);

        Task<int> ExecuteAsync(
            FormattableString sql);

        Task<IReadOnlyList<int>> ExecuteManyAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            CancellationToken cancellationToken,
            IEnumerable<TItem> items);

        Task<IReadOnlyList<int>> ExecuteManyAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            IEnumerable<TItem> items);

        Task<IReadOnlyList<int>> ExecuteManyParamsAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            CancellationToken cancellationToken,
            params TItem[] items);

        Task<IReadOnlyList<int>> ExecuteManyParamsAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            params TItem[] items);

        Task<T> ExecuteScalarAsync<T>(
            FormattableString sql);

        Task<T> ExecuteScalarAsync<T>(
            FormattableString sql,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, CancellationToken, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync);

        Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, CancellationToken, Task> handleRowAsync,
            CancellationToken cancellationToken);

        Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, Task> handleRowAsync);
    }
}
