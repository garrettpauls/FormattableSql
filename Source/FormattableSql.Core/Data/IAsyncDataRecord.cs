using System.Threading;
using System.Threading.Tasks;

namespace FormattableSql.Core.Data
{
    public interface IAsyncDataRecord
    {
        Task<T> GetValueAsync<T>(int ordinal);

        Task<T> GetValueAsync<T>(int ordinal, CancellationToken cancellationToken);

        Task<T> GetValueAsync<T>(string fieldName);

        Task<T> GetValueAsync<T>(string fieldName, CancellationToken cancellationToken);

        Task<bool> IsDBNullAsync(int ordinal);

        Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken);

        Task<bool> IsDBNullAsync(string fieldName);

        Task<bool> IsDBNullAsync(string fieldName, CancellationToken cancellationToken);
    }
}