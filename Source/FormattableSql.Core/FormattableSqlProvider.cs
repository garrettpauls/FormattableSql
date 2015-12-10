using FormattableSql.Core.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core
{
    public sealed class FormattableSqlProvider : IFormattableSqlProvider
    {
        public delegate void CommandGeneratedEventHandler(FormattableSqlProvider sender, DbCommand command);

        private readonly ISqlProvider mSQLProvider;

        public FormattableSqlProvider(ISqlProvider sqlProvider)
        {
            mSQLProvider = sqlProvider;
        }

        public event CommandGeneratedEventHandler CommandGenerated;

        public async Task<IReadOnlyCollection<TResult>> QueryAsync<TResult>(
            FormattableString query,
            Func<IAsyncDataRecord, TResult> createResult,
            CancellationToken cancellationToken)
        {
            var results = new List<TResult>();

            using (var connection = mSQLProvider.CreateConnection())
            using (var command = _CreateCommand(connection, query))
            using (var reader = await _ExecuteReaderAsync(command, cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var item = createResult(new DbDataReaderAsyncRecord(reader));
                    results.Add(item);
                }
            }

            return results.AsReadOnly();
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

            CommandGenerated?.Invoke(this, command);

            return command;
        }
    }
}
