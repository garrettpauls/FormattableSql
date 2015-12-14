using Moq;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace FormattableSql.Core.Tests.Utilities
{
    public sealed class MockDbConnection : DbConnection
    {
        public MockDbConnection()
        {
            Transaction = new MockTransaction(this);
            Command = new Mock<DbCommand>();
        }

        public Mock<DbCommand> Command { get; }
        public override string ConnectionString { get; set; }
        public override string Database { get; }
        public override string DataSource { get; }
        public bool IsClosed { get; private set; }
        public bool IsOpened { get; private set; }
        public override string ServerVersion { get; }
        public override ConnectionState State { get; }
        public MockTransaction Transaction { get; }

        public override void ChangeDatabase(string databaseName)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            IsClosed = true;
        }

        public override void Open()
        {
            IsOpened = true;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return Transaction;
        }

        protected override DbCommand CreateDbCommand()
        {
            return Command.Object;
        }
    }
}
