using FormattableSql.Core.Data.Provider;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class FormattableSqlProviderFixture
    {
        private Action<DbCommandBuilder> mCommandConfiguration;

        public FormattableSqlProviderFixture()
        {
            SqlProvider.Setup(x => x.CreateConnection()).Returns(Connection.Object);
            SqlProvider.Setup(x => x.CreateParameter(It.IsAny<DbCommand>(), It.IsAny<uint>(), It.IsAny<object>()))
                       .Returns<DbCommand, uint, object>(_CreateParameter);

            Connection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(_BuildCommand);
            Connection.Protected().Setup<DbTransaction>("BeginDbTransaction", ItExpr.IsAny<IsolationLevel>()).Returns(Transaction.Object);
        }

        public List<Mock<DbCommand>> Commands { get; } = new List<Mock<DbCommand>>();

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

        public FormattableSqlProviderFixture WithCommandConfiguration(Action<DbCommandBuilder> configuration)
        {
            mCommandConfiguration = configuration;
            return this;
        }

        private static DbParameter _CreateParameter(DbCommand cmd, uint idx, object value)
        {
            var param = new Mock<DbParameter>();

            param.SetupAllProperties();
            param.Object.ParameterName = $"@p{idx}";
            param.Object.Value = value;

            return param.Object;
        }

        private DbCommand _BuildCommand()
        {
            var builder = new DbCommandBuilder();

            mCommandConfiguration?.Invoke(builder);

            var command = builder.BuildMock();
            Commands.Add(command);
            return command.Object;
        }
    }
}
