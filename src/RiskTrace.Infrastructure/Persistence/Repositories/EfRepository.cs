using Microsoft.EntityFrameworkCore;
using RiskTrace.Core.Interfaces;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class EfRepository<T>(AppDbContext dbContext) : IRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public IQueryable<T> Query(string sqlQuery, params object[] parameters)
    {
        return _dbSet.FromSqlRaw(sqlQuery, parameters);
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        return _dbSet.AddAsync(entity, cancellationToken).AsTask();
    }

    public async Task<IEnumerable<T>> AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        var entityList = entities.ToList();
        await _dbSet.AddRangeAsync(entityList, cancellationToken);

        return entityList;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        _dbSet.RemoveRange(entities);
        return Task.CompletedTask;
    }
}
