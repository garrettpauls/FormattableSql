using System;
using System.Data.Common;
using System.Globalization;

namespace FormattableSql.Core.Data
{
    public abstract class SqlProvider : ISqlProvider
    {
        public abstract DbConnection CreateConnection();

        public virtual DbParameter CreateParameter(DbCommand command, uint index, object value)
        {
            var param = command.CreateParameter();

            param.ParameterName = GetParameterName(command, index, value);
            param.Value = value ?? DBNull.Value;

            return param;
        }

        protected virtual string GetParameterName(DbCommand command, uint index, object value)
        {
            return $"@p__fsql__{index.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
