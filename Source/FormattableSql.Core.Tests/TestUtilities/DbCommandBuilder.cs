using Moq;
using Moq.Protected;
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class DbCommandBuilder
    {
        private readonly Mock<DbCommand> mCommand = new Mock<DbCommand>();

        public DbCommandBuilder()
        {
            mCommand.SetupAllProperties();
            mCommand.Protected().Setup<DbParameterCollection>("DbParameterCollection")
                    .Returns(new DbParameterCollectionImpl());
            mCommand.Protected().Setup<DbDataReader>("ExecuteDbDataReader", ItExpr.IsAny<CommandBehavior>())
                    .Returns<CommandBehavior>(_CreateDataReader);
        }

        public DbCommand Build()
        {
            return mCommand.Object;
        }

        public Mock<DbCommand> BuildMock()
        {
            return mCommand;
        }

        public DbCommandBuilder WithExecuteNonQueryAsyncReturning(int value, CancellationToken token)
        {
            mCommand.Setup(x => x.ExecuteNonQueryAsync(token)).Returns(Task.FromResult(value));
            return this;
        }

        public DbCommandBuilder WithExecuteReaderAsyncReturning(Func<DbDataReader> createReader)
        {
            mCommand.Protected().Setup<Task<DbDataReader>>("ExecuteDbDataReaderAsync", ItExpr.IsAny<CommandBehavior>(), ItExpr.IsAny<CancellationToken>())
                    .Returns<CommandBehavior, CancellationToken>((b, t) => Task.FromResult(createReader()));
            return this;
        }

        public DbCommandBuilder WithExecuteScalarAsyncReturning(object value, CancellationToken token)
        {
            mCommand.Setup(x => x.ExecuteScalarAsync(token))
                    .Returns<CancellationToken>(ct => Task.FromResult(value));
            return this;
        }

        public DbCommandBuilder WithMockParameters()
        {
            return WithParameterFactory(() =>
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

        private static DbDataReader _CreateDataReader(CommandBehavior behavior)
        {
            return new Mock<DbDataReader>().SetupAllProperties().Object;
        }
    }
}
