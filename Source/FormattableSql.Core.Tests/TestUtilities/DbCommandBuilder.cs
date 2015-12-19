using Moq;
using Moq.Protected;
using System;
using System.Data.Common;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class DbCommandBuilder
    {
        private readonly Mock<DbCommand> mCommand = new Mock<DbCommand>();

        public DbCommandBuilder()
        {
            mCommand.SetupAllProperties();
            mCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection").Returns(new DbParameterCollectionImpl());
        }

        public DbCommand Build()
        {
            return mCommand.Object;
        }

        public DbCommandBuilder WithMockParameters()
        {
            return this
                .WithParameterFactory(() =>
                {
                    var param = new Mock<DbParameter>();
                    param.SetupAllProperties();
                    return param.Object;
                });
        }

        public DbCommandBuilder WithParameterFactory(Func<DbParameter> parameterFactory)
        {
            mCommand.Protected().Setup<DbParameter>("CreateDbParameter").Returns(parameterFactory);
            return this;
        }
    }
}
