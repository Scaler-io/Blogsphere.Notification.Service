using System.Linq.Expressions;
using Azure.Data.Tables;
using Blogsphere.Notification.Service.Entities;

namespace Blogsphere.Notification.Service.Data.Storage;

public class TableRepository<T> : ITableRepository<T> where T : EntityBase
{

    private readonly TableClient _tableClient;

    public TableRepository(TableServiceClient tableServiceClient)
    {
        _tableClient = tableServiceClient.GetTableClient(typeof(T).Name);
        _tableClient.CreateIfNotExistsAsync();
    }

    public async Task<T> GetAsync(string partitionKey, string rowKey)
    {
        var response = await _tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey);
        return response.HasValue ? response.Value : null;
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var results = new List<T>();
        await foreach (var entity in _tableClient.QueryAsync<T>())
        {
            results.Add(entity);
        }
        return results;
    }

    public async Task<IEnumerable<T>> QueryAsync(string filter)
    {
        var results = new List<T>();
        await foreach (var page in _tableClient.QueryAsync<T>(filter).AsPages())
        {
            results.AddRange(page.Values);
        }
        return results;
    }

    public async Task<IEnumerable<T>> QueryAsync(Expression<Func<T, bool>> filter)
    {
        var results = new List<T>();
        await foreach (var entity in _tableClient.QueryAsync(filter))
        {
            results.Add(entity);
        }
        return results;
    }

    public async Task<bool> ExistsAsync(string partitionKey, string rowKey)
    {
        return (await _tableClient.GetEntityIfExistsAsync<T>(partitionKey, rowKey)).HasValue;
    }

    public async Task UpsertAsync(T entity)
    {
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Merge);
    }
    public async Task AddAsync(T entity)
    {
        await _tableClient.AddEntityAsync(entity);
    }

    public async Task AddBatchAsync(IEnumerable<T> entities)
    {
        var batch = new List<TableTransactionAction>();

        foreach (var entity in entities)
        {
            batch.Add(new TableTransactionAction(TableTransactionActionType.Add, entity));
        }

        await _tableClient.SubmitTransactionAsync(batch);
    }

    public async Task<int> CountAsync(string filter = null)
    {
        var count = 0;
        await foreach (var _ in string.IsNullOrEmpty(filter)
            ? _tableClient.QueryAsync<T>()
            : _tableClient.QueryAsync<T>(filter))
        {
            count++;
        }
        return count;
    }

    public async Task DeleteAsync(string partitionKey, string rowKey)
    {
        await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
    }

    public async Task DeleteAsync(T entity)
    {
        await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, entity.ETag);
    }

    public async Task UpdateAsync(T entity)
    {
        await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
    }
}
