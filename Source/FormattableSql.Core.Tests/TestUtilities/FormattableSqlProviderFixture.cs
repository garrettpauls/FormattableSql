using FormattableSql.Core.Data.Provider;
using Moq;
using Moq.Protected;
using System.Data;
using System.Data.Common;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class FormattableSqlProviderFixture
    {
        public FormattableSqlProviderFixture()
        {
            SqlProvider.Setup(x => x.CreateConnection()).Returns(Connection.Object);
            SqlProvider.Setup(x => x.CreateParameter(It.IsAny<DbCommand>(), It.IsAny<uint>(), It.IsAny<object>()))
                       .Returns<DbCommand, uint, object>(_CreateParameter);

            Connection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(Command.Object);
            Connection.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>()).Returns(Transaction.Object);

            Command.SetupAllProperties();
            Command.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(new DbParameterCollectionImpl());
        }

        public Mock<DbCommand> Command { get; } = new Mock<DbCommand>();

        public Mock<DbConnection> Connection { get; } = new Mock<DbConnection>();

        public Mock<ISqlProvider> SqlProvider { get; } = new Mock<ISqlProvider>();

        public Mock<DbTransaction> Transaction { get; } = new Mock<DbTransaction>();

        public FormattableSqlProvider CreateSut()
        {
            return new FormattableSqlProvider(SqlProvider.Object);
        }

        public void VerifyConnectionBeginTransaction(Times times)
        {
            Connection.Protected().Verify<DbTransaction>("BeginDbTransaction", times, ItExpr.IsAny<IsolationLevel>());
        }

        public void VerifyConnectionCreateCommand(Times times)
        {
            Connection.Protected().Verify<DbCommand>("CreateDbCommand", times);
        }

        private static DbParameter _CreateParameter(DbCommand cmd, uint idx, object value)
        {
            var param = new Mock<DbParameter>();

            param.SetupAllProperties();
            param.Object.ParameterName = $"@p{idx}";
            param.Object.Value = value;

            return param.Object;
        }
    }
}
