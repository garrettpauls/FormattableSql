using FluentAssertions;
using FormattableSql.Core.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;

namespace FormattableSql.Core.Tests.Data.Provider
{
    [TestClass]
    public class GenericSqlProviderTests
    {
        [TestMethod]
        public void CreateConnectionCallsFactoryMethod()
        {
            var functionCallCount = 0;
            var connection = new SqlConnection();

            var provider = new GenericSqlProvider(() =>
            {
                functionCallCount++;
                return connection;
            });

            var result = provider.CreateConnection();

            result.Should().BeSameAs(connection, "the connection factory method's result should be returned");
            functionCallCount.Should().Be(1, "the connection factory method should only be called once");
        }
    }
}
