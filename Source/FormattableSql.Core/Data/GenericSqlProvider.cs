using System;
using System.Data.Common;

namespace FormattableSql.Core.Data
{
    public class GenericSqlProvider : SqlProvider
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
