using FluentAssertions;
using FormattableSql.Core.Data.Provider;
using FormattableSql.Core.Tests.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FormattableSql.Core.Tests.Data.Provider
{
    [TestClass]
    public class AdoNetSqlProviderTests
    {
        [TestMethod]
        public void ConstructorNullConnectionStringThrowsException()
        {
            var dbProvider = new DbProviderFactoryBuilder()
                .Build();
            new Action(() => new AdoNetSqlProvider(dbProvider, null))
                .ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("connectionString");
        }

        [TestMethod]
        public void ConstructorNullProviderThrowsException()
        {
            new Action(() => new AdoNetSqlProvider(null, ""))
                .ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("provider");
        }

        [TestMethod]
        public void CreateConnectionSuccessful()
        {
            var dbProvider = new DbProviderFactoryBuilder()
                .ReturningMockConnection()
                .Build();
            var connectionString = "connection string";
            var provider = new AdoNetSqlProvider(dbProvider, connectionString);

            var connection = provider.CreateConnection();

            connection.Should().NotBeNull("CreateConnection should never return null");
            connection.ConnectionString.Should().Be(connectionString);
        }

        [TestMethod]
        public void CreateNullConnection()
        {
            var dbProvider = new DbProviderFactoryBuilder()
                .ReturningConnection(() => null)
                .Build();
            var connectionString = "connection string";
            var provider = new AdoNetSqlProvider(dbProvider, connectionString);

            new Action(() => provider.CreateConnection()).ShouldThrow<InvalidOperationException>();
        }
    }
}
