using FormattableSql.Core.Data.Provider;
using System;
using System.Data.Common;
using System.Linq;

namespace FormattableSql.Core.Data.Extensions
{
    public static class CommandExtensions
    {
        public static void ConfigureFrom(this DbCommand command, FormattableString sql, ISqlProvider sqlProvider)
        {
            var parameterNames = sql
                .GetArguments()
                .Select((arg, idx) =>
                {
                    var param = sqlProvider.CreateParameter(command, (uint)idx, arg);
                    command.Parameters.Add(param);
                    return param.ParameterName;
                })
                .Cast<object>()
                .ToArray();

            command.CommandText = string.Format(sql.Format, parameterNames);
        }
    }
}
