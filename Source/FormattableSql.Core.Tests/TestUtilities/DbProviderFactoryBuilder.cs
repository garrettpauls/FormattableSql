using Moq;
using System;
using System.Data.Common;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class DbProviderFactoryBuilder
    {
        private readonly Mock<DbProviderFactory> mFactory = new Mock<DbProviderFactory>();

        public DbProviderFactory Build()
        {
            return mFactory.Object;
        }

        public DbProviderFactoryBuilder ReturningConnection(Func<DbConnection> connectionFactory)
        {
            mFactory.Setup(x => x.CreateConnection()).Returns(connectionFactory);
            return this;
        }

        public DbProviderFactoryBuilder ReturningMockConnection()
        {
            return ReturningConnection(() => new Mock<DbConnection>().SetupAllProperties().Object);
        }
    }
}
