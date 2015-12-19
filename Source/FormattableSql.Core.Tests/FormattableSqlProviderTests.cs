using FluentAssertions;
using FormattableSql.Core.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
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
            var fixture = new FormattableSqlProviderFixture()
                .WithCommandConfiguration(
                    cmd => cmd.Setup(x => x.ExecuteNonQueryAsync(ct)).Returns(Task.FromResult(1)));

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
            fixture.Commands[0].Verify(x => x.ExecuteNonQueryAsync(ct), Times.Once);
            fixture.Transaction.Verify(x => x.Commit(), Times.Once);

            result.Should().Be(1);

            var command = fixture.Commands[0].Object;
            command.CommandText.Should().Be("select * from Item where date=@p0, id=@p1");
            command.Parameters.Count.Should().Be(2, "one parameter should be generated for each formattable argument");
            command.Parameters[0].ParameterName.Should().Be("@p0", "each parameter should have a name according to its argument index");
            command.Parameters[0].Value.Should().Be(date, "each parameter should have the value of its respective formattable argument");
            command.Parameters[1].ParameterName.Should().Be("@p1", "each parameter should have a name according to its argument index");
            command.Parameters[1].Value.Should().Be(id, "each parameter should have the value of its respective formattable argument");
        }

        [TestMethod]
        public void ExecuteManyParamsAsyncIntegrationTest()
        {
            // setup
            var ct = CancellationToken.None;
            var fixture = new FormattableSqlProviderFixture()
                .WithCommandConfiguration(
                    cmd => cmd.Setup(x => x.ExecuteNonQueryAsync(ct)).Returns(Task.FromResult(1)));

            var sql = fixture.CreateSut();

            var date = DateTime.MaxValue;
            var id = 1;

            var itemA = new
            {
                Name = "a",
                Date = new DateTime(2015, 12, 12)
            };
            var itemB = new
            {
                Name = "b",
                Date = new DateTime(2016, 1, 1)
            };

            // execute
            var results = sql.ExecuteManyParamsAsync(item => $"insert into Item (name, date) values ({item.Name}, {item.Date})", ct, itemA, itemB).Result;

            // verify
            fixture.SqlProvider.Verify(x => x.CreateConnection(), Times.Once);
            fixture.Connection.Verify(x => x.OpenAsync(ct), Times.Once);
            fixture.VerifyConnectionBeginTransaction(Times.Once());
            fixture.VerifyConnectionCreateCommand(Times.Exactly(2));
            fixture.Commands[0].Verify(x => x.ExecuteNonQueryAsync(ct), Times.Once);
            fixture.Commands[1].Verify(x => x.ExecuteNonQueryAsync(ct), Times.Once);
            fixture.Transaction.Verify(x => x.Commit(), Times.Once);

            results.Should().HaveCount(2, "one result should be returned per item");

            var commandAndItem = new[] { itemA, itemB }.Zip(fixture.Commands, Tuple.Create);
            foreach (var tuple in commandAndItem)
            {
                var commandMock = tuple.Item2;
                var item = tuple.Item1;
                var command = commandMock.Object;

                command.CommandText.Should().Be("insert into Item (name, date) values (@p0, @p1)");
                command.Parameters.Count.Should().Be(2, "one parameter should be generated for each formattable argument");
                command.Parameters[0].ParameterName.Should().Be("@p0", "each parameter should have a name according to its argument index");
                command.Parameters[0].Value.Should().Be(item.Name, "each parameter should have the value of its respective formattable argument");
                command.Parameters[1].ParameterName.Should().Be("@p1", "each parameter should have a name according to its argument index");
                command.Parameters[1].Value.Should().Be(item.Date, "each parameter should have the value of its respective formattable argument");
            }
        }
    }
}
