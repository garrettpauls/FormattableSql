using FluentAssertions;
using FormattableSql.Core.Data.Provider;
using FormattableSql.Core.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Tests
{
    [TestClass]
    public class FormattableSqlProviderTests
    {
        [TestMethod]
        public void ExecuteAsyncTest()
        {
            var a = "a";
            var b = 2;
            var c = Guid.NewGuid();

            _RunTest(
                (sql, token) => sql.ExecuteAsync($"A={a}; B={b}; C={c}", token).Wait(token),
                (sql, connection, transaction, command, token) =>
                {
                    command.Object.Parameters.Should().HaveCount(3, "all format parameters should be added as db parameters");
                    command.Object.Parameters[0].ParameterName.Should().Be("@0", "commands should be added in order");
                    command.Object.Parameters[0].Value.Should().Be(a, "parameter[0] should have the value of the corresponding format argument");
                    command.Object.Parameters[1].ParameterName.Should().Be("@1", "commands should be added in order");
                    command.Object.Parameters[1].Value.Should().Be(b, "parameter[1] should have the value of the corresponding format argument");
                    command.Object.Parameters[2].ParameterName.Should().Be("@2", "commands should be added in order");
                    command.Object.Parameters[2].Value.Should().Be(c, "parameter[2] should have the value of the corresponding format argument");
                });
        }

        private static void _RunTest(
            Action<FormattableSqlProvider, CancellationToken> testAction,
            Action<Mock<ISqlProvider>, MockDbConnection, MockTransaction, Mock<DbCommand>, CancellationToken> verifyAction)
        {
            var provider = new Mock<ISqlProvider>(MockBehavior.Strict);
            var connection = new MockDbConnection();
            var transaction = new MockTransaction(connection);
            var command = new Mock<DbCommand>(MockBehavior.Strict);
            var token = new CancellationToken();

            provider.Setup(x => x.CreateConnection()).Returns(connection);
            provider.Setup(x => x.CreateParameter(It.IsAny<DbCommand>(), It.IsAny<uint>(), It.IsAny<object>()))
                    .Returns<DbCommand, uint, object>((_, idx, value) =>
                    {
                        var param = new Mock<DbParameter>(MockBehavior.Loose);
                        param.Object.ParameterName = $"@{idx}";
                        param.Object.Value = value;
                        return param.Object;
                    });

            var sql = new FormattableSqlProvider(provider.Object);

            testAction(sql, token);

            verifyAction(provider, connection, transaction, command, token);
        }
    }
}
