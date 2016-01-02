using FluentAssertions;
using FormattableSql.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Tests.Data
{
    [TestClass]
    public class DbDataReaderAsyncRecordTests
    {
        [TestMethod]
        public void GetValueAsyncNotNullTest()
        {
            var baseReader = _CreateReader(1);
            var reader = new DbDataReaderAsyncRecord(baseReader);

            reader.GetValueAsync<long?>(0).Result
                  .Should().Be(1);
        }

        [TestMethod]
        public void GetValueAsyncNullTest()
        {
            var baseReader = _CreateReader(null);
            var reader = new DbDataReaderAsyncRecord(baseReader);

            reader.GetValueAsync<long?>(0).Result
                  .Should().Be(null);
        }

        private static DbDataReader _CreateReader(long? value)
        {
            var reader = new Mock<DbDataReader>();

            reader.Setup(x => x.IsDBNull(0)).Returns(value.HasValue);
            reader.Setup(x => x.GetFieldValueAsync<long?>(0, It.IsAny<CancellationToken>())).Returns(Task.FromResult(value));

            return reader.Object;
        }
    }
}
