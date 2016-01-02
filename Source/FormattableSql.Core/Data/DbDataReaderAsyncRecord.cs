using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Data
{
    [ExcludeFromCodeCoverage]
    public sealed class DbDataReaderAsyncRecord : IAsyncDataRecord
    {
        private readonly DbDataReader mReader;

        public DbDataReaderAsyncRecord(DbDataReader reader)
        {
            mReader = reader;
        }

        public Task<T> GetValueAsync<T>(int ordinal)
        {
            return GetValueAsync<T>(ordinal, CancellationToken.None);
        }

        public async Task<T> GetValueAsync<T>(int ordinal, CancellationToken cancellationToken)
        {
            T result;

            if (await IsDBNullAsync(ordinal, cancellationToken))
            {
                result = default(T);
            }
            else
            {
                result = await mReader.GetFieldValueAsync<T>(ordinal, cancellationToken);
            }

            return result;
        }

        public Task<T> GetValueAsync<T>(string fieldName)
        {
            return GetValueAsync<T>(fieldName, CancellationToken.None);
        }

        public Task<T> GetValueAsync<T>(string fieldName, CancellationToken cancellationToken)
        {
            return GetValueAsync<T>(mReader.GetOrdinal(fieldName), cancellationToken);
        }

        public Task<bool> IsDBNullAsync(int ordinal)
        {
            return IsDBNullAsync(ordinal, CancellationToken.None);
        }

        public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
        {
            return mReader.IsDBNullAsync(ordinal, cancellationToken);
        }

        public Task<bool> IsDBNullAsync(string fieldName)
        {
            return IsDBNullAsync(fieldName, CancellationToken.None);
        }

        public Task<bool> IsDBNullAsync(string fieldName, CancellationToken cancellationToken)
        {
            return IsDBNullAsync(mReader.GetOrdinal(fieldName), cancellationToken);
        }
    }
}
