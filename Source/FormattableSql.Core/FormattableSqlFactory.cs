using FormattableSql.Core.Data.Provider;
using System;
using System.Configuration;
using System.Data.Common;

namespace FormattableSql.Core
{
    public static class FormattableSqlFactory
    {
        public static IFormattableSqlProvider For(ISqlProvider provider)
        {
            return new FormattableSqlProvider(provider);
        }

        public static IFormattableSqlProvider ForConnectionString(string connectionStringName)
        {
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
