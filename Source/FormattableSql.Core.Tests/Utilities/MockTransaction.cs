using System.Data;
using System.Data.Common;

namespace FormattableSql.Core.Tests.Utilities
{
    public sealed class MockTransaction : DbTransaction
    {
        public MockTransaction(DbConnection connection, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            DbConnection = connection;
            IsolationLevel = isolationLevel;
        }

        public bool IsCommitted { get; private set; }

        public bool IsDisposed { get; private set; }
        public override IsolationLevel IsolationLevel { get; }
        public bool IsRolledBack { get; private set; }
        protected override DbConnection DbConnection { get; }

        public override void Commit()
        {
            IsCommitted = true;
        }

        public override void Rollback()
        {
            IsRolledBack = true;
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
        }
    }
}
