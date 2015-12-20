using FluentAssertions;
using FormattableSql.Core.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;

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
                    builder => builder.WithExecuteNonQueryAsyncReturning(1, ct));

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
                    builder => builder.WithExecuteNonQueryAsyncReturning(1, ct));

            var sql = fixture.CreateSut();

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

        [TestMethod]
        public void ExecuteScalarAsyncIntegrationTest()
        {
            // setup
            var date = DateTime.MaxValue;
            var id = 1;

            var ct = CancellationToken.None;
            var fixture = new FormattableSqlProviderFixture()
                .WithCommandConfiguration(
                    builder => builder.WithExecuteScalarAsyncReturning(date.ToString("O"), ct));

            var sql = fixture.CreateSut();

            // execute
            var result = sql.ExecuteScalarAsync<DateTime>($"select top 1 Date from Item where Id={id} order by Date desc", ct).Result;

            // verify
            fixture.SqlProvider.Verify(x => x.CreateConnection(), Times.Once);
            fixture.Connection.Verify(x => x.OpenAsync(ct), Times.Once);
            fixture.VerifyConnectionBeginTransaction(Times.Once());
            fixture.VerifyConnectionCreateCommand(Times.Once());
            fixture.Commands[0].Verify(x => x.ExecuteScalarAsync(ct), Times.Once);
            fixture.Transaction.Verify(x => x.Commit(), Times.Once);

            result.Should().Be(date);

            var command = fixture.Commands[0].Object;
            command.CommandText.Should().Be("select top 1 Date from Item where Id=@p0 order by Date desc");
            command.Parameters.Count.Should().Be(1, "one parameter should be generated for each formattable argument");
            command.Parameters[0].ParameterName.Should().Be("@p0", "each parameter should have a name according to its argument index");
            command.Parameters[0].Value.Should().Be(id, "each parameter should have the value of its respective formattable argument");
        }

        [TestMethod]
        public void QueryAsyncIntegrationTest()
        {
            // setup
            var ct = CancellationToken.None;
            var readers = new List<DbDataReader>();
            var queryData = new[]
            {
                new
                {
                    Name = "a",
                    Date = new DateTime(2015, 12, 12)
                },
                new
                {
                    Name = "b",
                    Date = new DateTime(2015, 1, 1)
                }
            };
            var fixture = new FormattableSqlProviderFixture()
                .WithCommandConfiguration(
                    builder => builder.WithExecuteReaderAsyncReturning(
                        () => new DbDataReaderBuilder()
                                  .TrackWith(readers)
                                  .WithResults(queryData)
                                  .Build()));

            var sql = fixture.CreateSut();
            var id = 1;

            // execute
            var results = sql.QueryAsync(
                $"select Name, Date from Item where Id={id}",
                async (row, ct2) => new
                {
                    Name = await row.GetValueAsync<string>("Name", ct2),
                    Date = await row.GetValueAsync<DateTime>("Date", ct2)
                }, CancellationToken.None).Result;

            // verify
            fixture.SqlProvider.Verify(x => x.CreateConnection(), Times.Once);
            fixture.Connection.Verify(x => x.OpenAsync(ct), Times.Once);
            fixture.VerifyConnectionCreateCommand(Times.Once());

            readers.Should().HaveCount(1, "one reader should have been created");
            results.Should().HaveCount(2, "two results should have been returned by the reader");

            var command = fixture.Commands[0].Object;
            command.CommandText.Should().Be("select Name, Date from Item where Id=@p0");
            command.Parameters.Count.Should().Be(1, "one parameter should be generated for each formattable argument");
            command.Parameters[0].ParameterName.Should().Be("@p0", "each parameter should have a name according to its argument index");
            command.Parameters[0].Value.Should().Be(id, "each parameter should have the value of its respective formattable argument"); ;

            var resultPair = results.Zip(queryData, Tuple.Create);

            foreach (var tuple in resultPair)
            {
                var result = tuple.Item1;
                var data = tuple.Item2;

                result.Name.Should().Be(data.Name, "the Name result should map to the Name value");
                result.Date.Should().Be(data.Date, "the Date result should map to the Date value");
            }
        }
    }
}
