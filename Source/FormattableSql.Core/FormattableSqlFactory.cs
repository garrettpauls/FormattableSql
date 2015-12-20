using FormattableSql.Core.Data.Provider;
using System;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace FormattableSql.Core
{
    public static class FormattableSqlFactory
    {
        [ExcludeFromCodeCoverage]
        public static IFormattableSqlProvider For(ISqlProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            return new FormattableSqlProvider(provider);
        }

        public static IFormattableSqlProvider ForConnectionString(string connectionStringName)
        {
            if (string.IsNullOrEmpty(connectionStringName))
            {
                throw new ArgumentNullException(nameof(connectionStringName));
            }

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (connectionString == null)
            {
                throw new ArgumentException($"No connection string found with name {connectionStringName}", nameof(connectionStringName));
            }

            var provider = DbProviderFactories.GetFactory(connectionString.ProviderName);

            return For(new AdoNetSqlProvider(provider, connectionString.ConnectionString));
        }
    }
}
