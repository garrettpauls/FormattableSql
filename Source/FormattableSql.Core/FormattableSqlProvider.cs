using FormattableSql.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, Task<TResult>> createResultAsync,
            CancellationToken cancellationToken)
        {
            var results = new List<TResult>();

            await QueryAsync(query, async row => results.Add(await createResultAsync(row)), cancellationToken);

            return results.AsReadOnly();
        }

        public async Task QueryAsync(
            FormattableString query,
            Func<IAsyncDataRecord, Task> handleRowAsync,
            CancellationToken cancellationToken)
        {
            using (var connection = mSQLProvider.CreateConnection())
            using (var command = _CreateCommand(connection, query))
            using (var reader = await _ExecuteReaderAsync(command, cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    await handleRowAsync(new DbDataReaderAsyncRecord(reader));
                }
            }
        }

        private static async Task<DbDataReader> _ExecuteReaderAsync(DbCommand command, CancellationToken cancellationToken)
        {
            await command.Connection.OpenAsync(cancellationToken);
            return await command.ExecuteReaderAsync(cancellationToken);
        }

        private DbCommand _CreateCommand(DbConnection connection, FormattableString query)
        {
            var command = connection.CreateCommand();

            var parameterNames = query
                .GetArguments()
                .Select((arg, idx) =>
                {
                    var param = mSQLProvider.CreateParameter(command, (uint)idx, arg);
                    command.Parameters.Add(param);
                    return param.ParameterName;
                })
                .Cast<object>()
                .ToArray();

            command.CommandText = string.Format(query.Format, parameterNames);

            CommandPrepared?.Invoke(this, command);

            return command;
        }
    }
}
