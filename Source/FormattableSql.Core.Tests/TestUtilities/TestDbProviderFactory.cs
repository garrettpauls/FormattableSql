using System.Data.Common;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public sealed class TestDbProviderFactory : DbProviderFactory
    {
        public static readonly TestDbProviderFactory Instance = new TestDbProviderFactory();
    }
}
