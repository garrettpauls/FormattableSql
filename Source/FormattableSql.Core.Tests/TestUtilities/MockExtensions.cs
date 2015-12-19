using Moq;

namespace FormattableSql.Core.Tests.TestUtilities
{
    public static class MockExtensions
    {
        public static Mock<T> AsMock<T>(this T value) where T : class
            => Mock.Get(value);
    }
}
