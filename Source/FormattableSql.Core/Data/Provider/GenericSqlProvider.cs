using System;
using System.Data.Common;

namespace FormattableSql.Core.Data.Provider
{
    public sealed class GenericSqlProvider : SqlProvider
    {
        private readonly Func<DbConnection> mConnectionFactory;

        public GenericSqlProvider(Func<DbConnection> connectionFactory)
        {
            mConnectionFactory = connectionFactory;
        }

        public override DbConnection CreateConnection()
        {
            return mConnectionFactory();
        }
    }
}
