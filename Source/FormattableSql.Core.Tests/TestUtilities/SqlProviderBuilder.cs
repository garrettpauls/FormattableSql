using FormattableSql.Core.Data.Provider;

using Moq;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class SqlProviderBuilder
    {
        private readonly Mock<ISqlProvider> mSqlProvider = new Mock<ISqlProvider>();

        public ISqlProvider Build()
        {
            return mSqlProvider.Object;
        }
    }
}
