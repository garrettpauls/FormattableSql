using FormattableSql.Core.Data;
using FormattableSql.Core.Data.Extensions;
using FormattableSql.Core.Data.Provider;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core
{
    public delegate void CommandPreparedEventHandler(FormattableSqlProvider sender, DbCommand command);

    public sealed class FormattableSqlProvider : IFormattableSqlProvider
    {
        private readonly ISqlProvider mSQLProvider;

        public FormattableSqlProvider(ISqlProvider sqlProvider)
        {
            mSQLProvider = sqlProvider;
        }

        public event CommandPreparedEventHandler CommandPrepared;

        public async Task<int> ExecuteAsync(
            FormattableString sql,
            CancellationToken cancellationToken)
        {
            int result;

            using (var connection = await _OpenNewConnectionAsync(cancellationToken))
            using (var transaction = connection.BeginTransaction())
            using (var command = _CreateCommand(connection, sql))
            {
                result = await command.ExecuteNonQueryAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
                transaction.Commit();
            }

            return result;
        }

        [ExcludeFromCodeCoverage]
        public Task<int> ExecuteAsync(FormattableString sql)
        {
            return ExecuteAsync(sql, CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        public Task<IReadOnlyList<int>> ExecuteManyAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            CancellationToken cancellationToken,
            IEnumerable<TItem> items)
        {
            return ExecuteManyParamsAsync(buildSql, cancellationToken, items.ToArray());
        }

        [ExcludeFromCodeCoverage]
        public Task<IReadOnlyList<int>> ExecuteManyAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            IEnumerable<TItem> items)
        {
            return ExecuteManyAsync(buildSql, CancellationToken.None, items);
        }

        public async Task<IReadOnlyList<int>> ExecuteManyParamsAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            CancellationToken cancellationToken,
            params TItem[] items)
        {
            var results = new int[items.Length];

            using (var connection = await _OpenNewConnectionAsync(cancellationToken))
            using (var transaction = connection.BeginTransaction())
            {
                for (int i = 0; i < items.Length; i++)
                {
                    using (var command = _CreateCommand(connection, buildSql(items[i])))
                    {
                        results[i] = await command.ExecuteNonQueryAsync(cancellationToken);
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                }

                transaction.Commit();
            }

            return results;
        }

        [ExcludeFromCodeCoverage]
        public Task<IReadOnlyList<int>> ExecuteManyParamsAsync<TItem>(
            Func<TItem, FormattableString> buildSql,
            params TItem[] items)
        {
            return ExecuteManyAsync(buildSql, items.AsEnumerable());
        }

        [ExcludeFromCodeCoverage]
        public Task<T> ExecuteScalarAsync<T>(
            FormattableString sql)
        {
            return ExecuteScalarAsync<T>(sql, CancellationToken.None);
        }

        public async Task<T> ExecuteScalarAsync<T>(
            FormattableString sql,
            CancellationToken cancellationToken)
        {
            T result;

            using (var connection = await _OpenNewConnectionAsync(cancellationToken))
            using (var transaction = connection.BeginTransaction())
            using (var command = _CreateCommand(connection, sql))
            {
                var objResult = await command.ExecuteScalarAsync(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                result = (T)Convert.ChangeType(objResult, typeof(T));
                cancellationToken.ThrowIfCancellationRequested();
                transaction.Commit();
            }

            return result;
        }

        public async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, CancellationToken, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken)
        {
            var results = new List<TResult>();

            await QueryAsync(query, async (row, ct) => results.Add(await createResultAsync(row, ct)), cancellationToken);

            return results.AsReadOnly();
        }

        [ExcludeFromCodeCoverage]
        public Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync)
        {
            return QueryAsync(query, (row, ct) => createResultAsync(row), CancellationToken.None);
        }

        public async Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, CancellationToken, Task> handleRowAsync,
            CancellationToken cancellationToken)
        {
            using (var connection = await _OpenNewConnectionAsync(cancellationToken))
            using (var command = _CreateCommand(connection, query))
            using (var reader = await _ExecuteReaderAsync(command, cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    await handleRowAsync(new DbDataReaderAsyncRecord(reader), cancellationToken);
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, Task> handleRowAsync)
        {
            return QueryAsync(query, (row, ct) => handleRowAsync(row), CancellationToken.None);
        }

        [ExcludeFromCodeCoverage]
        private static async Task<DbDataReader> _ExecuteReaderAsync(
            DbCommand command,
            CancellationToken cancellationToken)
        {
            return await command.ExecuteReaderAsync(cancellationToken);
        }

        private DbCommand _CreateCommand(
                                                                                                                            DbConnection connection,
            FormattableString sql)
        {
            var command = connection.CreateCommand();

            command.ConfigureFrom(sql, mSQLProvider);

            CommandPrepared?.Invoke(this, command);

            return command;
        }

        private async Task<DbConnection> _OpenNewConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = mSQLProvider.CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
