using FluentAssertions;
using FormattableSql.Core.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Tests
{
    [TestClass]
    public class FormattableSqlProviderTests
    {
        [TestMethod]
        public void ExecuteAsyncIntegrationTest()
        {
            // setup
            var ct = CancellationToken.None;
            var fixture = new FormattableSqlProviderFixture();
            fixture.Command.Setup(x => x.ExecuteNonQueryAsync(ct)).Returns(Task.FromResult(1));

            var sql = fixture.CreateSut();

            var date = DateTime.MaxValue;
            var id = 1;

            // execute
            var result = sql.ExecuteAsync($"select * from Item where date={date}, id={id}", ct).Result;

            // verify
            fixture.SqlProvider.Verify(x => x.CreateConnection(), Times.Once);
            fixture.Connection.Verify(x => x.OpenAsync(ct), Times.Once);
            fixture.VerifyConnectionBeginTransaction(Times.Once());
            fixture.VerifyConnectionCreateCommand(Times.Once());
            fixture.Command.Verify(x => x.ExecuteNonQueryAsync(ct), Times.Once);
            fixture.Transaction.Verify(x => x.Commit(), Times.Once);

            result.Should().Be(1);

            var command = fixture.Command.Object;
            command.CommandText.Should().Be("select * from Item where date=@p0, id=@p1");
            command.Parameters.Count.Should().Be(2, "one parameter should be generated for each formattable argument");
            command.Parameters[0].ParameterName.Should().Be("@p0", "each parameter should have a name according to its argument index");
            command.Parameters[0].Value.Should().Be(date, "each parameter should have the value of its respective formattable argument");
            command.Parameters[1].ParameterName.Should().Be("@p1", "each parameter should have a name according to its argument index");
            command.Parameters[1].Value.Should().Be(id, "each parameter should have the value of its respective formattable argument");
        }
    }
}
