using FluentAssertions;
using FormattableSql.Core.Data.Provider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;

namespace FormattableSql.Core.Tests
{
    [TestClass]
    public class FormattableSqlFactoryTests
    {
        [TestMethod]
        public void ForConnectionStringNoConnectionStringOnFileTest()
        {
            new Action(() => FormattableSqlFactory.ForConnectionString("not-on-file"))
                .ShouldThrow<ArgumentException>()
                .Which.ParamName.Should().Be("connectionStringName");
        }

        [TestMethod]
        public void ForConnectionStringNoProviderFactoryTest()
        {
            new Action(() => FormattableSqlFactory.ForConnectionString("invalidProviderString"))
                .ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void ForConnectionStringNullThrowsExceptionTest()
        {
            new Action(() => FormattableSqlFactory.ForConnectionString(null))
                .ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("connectionStringName");
        }

        [TestMethod]
        public void ForConnectionStringSuccessTest()
        {
            var sql = FormattableSqlFactory.ForConnectionString("validProviderString");
            sql.Should().NotBeNull();
        }

        [TestMethod]
        public void ForNullProviderThrowsExceptionTest()
        {
            new Action(() => FormattableSqlFactory.For(null))
                .ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should().Be("provider");
        }
    }
}
