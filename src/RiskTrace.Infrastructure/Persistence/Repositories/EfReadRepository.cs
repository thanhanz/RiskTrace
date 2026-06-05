using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RiskTrace.Core.Common;
using RiskTrace.Core.Interfaces;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class EfReadRepository<T>(AppDbContext dbContext) : IReadRepository<T>
    where T : class
{
    private readonly DbSet<T> _dbSet = dbContext.Set<T>();

    public Task<bool> AnyAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? _dbSet.AnyAsync(cancellationToken)
            : _dbSet.AnyAsync(predicate, cancellationToken);
    }

    public Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate is null
            ? _dbSet.CountAsync(cancellationToken)
            : _dbSet.CountAsync(predicate, cancellationToken);
    }

    public Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public Task<TResult?> FirstOrDefaultAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return _dbSet
            .Where(predicate)
            .Select(selector)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        var query = predicate is null ? _dbSet : _dbSet.Where(predicate);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
        Expression<Func<T, bool>> predicate,
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(predicate)
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TResult>> ListAsync<TResult>(
        Expression<Func<T, TResult>> selector,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Select(selector)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<TResult>> PagedListAsync<TResult>(
        PaginationRequest pagination,
        Expression<Func<T, TResult>> selector,
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = _dbSet;

        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        if (!string.IsNullOrWhiteSpace(pagination.SortBy))
        {
            query = pagination.IsDescending
                ? query.OrderByDescending(entity => EF.Property<object>(entity, pagination.SortBy))
                : query.OrderBy(entity => EF.Property<object>(entity, pagination.SortBy));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TResult>(
            items,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);
    }
}
