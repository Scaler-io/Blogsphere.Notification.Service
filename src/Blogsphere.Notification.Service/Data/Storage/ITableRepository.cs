using System.Linq.Expressions;
using Blogsphere.Notification.Service.Entities;

namespace Blogsphere.Notification.Service.Data.Storage;

public interface ITableRepository<T> where T : EntityBase
{
    Task<T> GetAsync(string partitionKey, string rowKey);  
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> QueryAsync(string filter);
    Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> filter);
    Task<bool> ExistsAsync(string partitionKey, string rowKey);
    Task UpsertAsync(T entity);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string partitionKey, string rowKey);
    Task DeleteAsync(T entity);
    Task AddBatchAsync(IEnumerable<T> entities);
    Task<int> CountAsync(string filter = null);
}
