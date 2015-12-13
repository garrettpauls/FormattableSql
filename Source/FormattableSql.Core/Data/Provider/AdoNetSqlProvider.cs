using System;
using System.Data.Common;

namespace FormattableSql.Core.Data.Provider
{
    public sealed class AdoNetSqlProvider : SqlProvider
    {
        private readonly string mConnectionString;
        private readonly DbProviderFactory mDbProvider;

        public AdoNetSqlProvider(DbProviderFactory provider, string connectionString)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            mDbProvider = provider;
            mConnectionString = connectionString;
        }

        public override DbConnection CreateConnection()
        {
            var connection = mDbProvider.CreateConnection();

            if (connection == null)
            {
                throw new InvalidOperationException($"{mDbProvider.GetType().FullName}::CreateConnection returned null.");
            }

            connection.ConnectionString = mConnectionString;
            return connection;
        }
    }
}
