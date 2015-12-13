using System.Data.Common;

namespace FormattableSql.Core.Data.Provider
{
    public interface ISqlProvider
    {
        /// <summary>
        /// Creates a new, unopened connection to the database.
        /// </summary>
        DbConnection CreateConnection();

        /// <summary>
        /// Creates a parameter based on the <paramref name="command" />. The parameter should not
        /// be added to the command's parameter list.
        /// </summary>
        /// <param name="command">The command to use to create the parameter.</param>
        /// <param name="index">
        /// The index of the SQL argument. This is unique among parameters for the given <paramref name="command" />.
        /// </param>
        /// <param name="value">The value of the parameter.</param>
        DbParameter CreateParameter(DbCommand command, uint index, object value);
    }
}
