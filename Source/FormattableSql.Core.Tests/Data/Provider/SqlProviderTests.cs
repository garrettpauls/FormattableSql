using FluentAssertions;
using FormattableSql.Core.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;

using DbCommandBuilder = FormattableSql.Core.Tests.TestUtilities.DbCommandBuilder;

namespace FormattableSql.Core.Tests.Data.Provider
{
    [TestClass]
    public class SqlProviderTests
    {
        [TestMethod]
        public void CreateParameterWithNullValue()
        {
            var provider = new SqlProviderImpl();
            var command = new DbCommandBuilder()
                .WithMockParameters()
                .Build();

            var param = provider.CreateParameter(command, 0, null);

            param.Should().NotBeNull();
            param.Value.Should().NotBeNull("null should be replaced with DBNull");
            param.Value.Should().Be(DBNull.Value, "null should be replaced with DBNull");
            param.ParameterName.Should().Be("@p__fsql__0");
        }

        [TestMethod]
        public void CreateParameterWithValue()
        {
            var provider = new SqlProviderImpl();
            var value = new object();
            var command = new DbCommandBuilder()
                .WithMockParameters()
                .Build();

            var param = provider.CreateParameter(command, 0, value);

            param.Should().NotBeNull();
            param.Value.Should().BeSameAs(value, "the parameter's value should be the same as what called the method");
            param.ParameterName.Should().Be("@p__fsql__0");
        }

        private class SqlProviderImpl : SqlProvider
        {
            public override DbConnection CreateConnection()
            {
                throw new NotImplementedException();
            }
        }
    }
}
