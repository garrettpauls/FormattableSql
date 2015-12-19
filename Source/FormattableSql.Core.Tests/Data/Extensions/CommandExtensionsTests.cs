using FluentAssertions;
using FormattableSql.Core.Data.Extensions;
using FormattableSql.Core.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace FormattableSql.Core.Tests.Data.Extensions
{
    [TestClass()]
    public class CommandExtensionsTests
    {
        [TestMethod()]
        public void ConfigureFromTest()
        {
            var command = new SqlCommand();
            var provider = new Mock<ISqlProvider>(MockBehavior.Strict);
            provider.Setup(x => x.CreateParameter(command, It.IsAny<uint>(), It.IsAny<object>()))
                    .Returns((SqlCommand cmd, uint idx, object val) => new SqlParameter($"@p{idx}", val));

            var a = Guid.NewGuid();
            var b = "value-b";
            var c = DateTime.Now;

            CommandExtensions.ConfigureFrom(command, $"UPDATE Table SET A={a}, B={b} WHERE C={c}", provider.Object);

            command.CommandText.Should().Be("UPDATE Table SET A=@p0, B=@p1 WHERE C=@p2", "string arguments should be converted into parameters");
            command.Parameters.Should().HaveCount(3, "each argument should be converted into a single parameter");
            command.Parameters.Cast<DbParameter>()
                   .Select(x => x.ParameterName).Should().BeEquivalentTo("@p0", "@p1", "@p2");
        }
    }
}
