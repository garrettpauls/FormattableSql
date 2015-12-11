using FormattableSql.Core.Data;
using System;
using System.Data.Common;
using System.Data.SQLite;

namespace SimpleNotes.Framework.Data
{
    public sealed class SQLiteSqlProvider : SqlProvider
    {
        private readonly string mConnectionString;

        public SQLiteSqlProvider(string connectionString)
        {
            mConnectionString = connectionString;
        }

        public static SQLiteSqlProvider FromFile(string file)
        {
            return new SQLiteSqlProvider(new SQLiteConnectionStringBuilder
            {
                DataSource = file,
                ForeignKeys = true
            }.ConnectionString);
        }

        public override DbConnection CreateConnection()
        {
            return new SQLiteConnection(mConnectionString);
        }
    }
}
