using RiskTrace.Core.Common;
using System.Linq.Expressions;

namespace RiskTrace.Core.Interfaces
{
    public interface IReadRepository<T> where T : class
    {
        Task<bool> AnyAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

        Task<int> CountAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<T?> FirstOrDefaultAsync(
            Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default);

        Task<TResult?> FirstOrDefaultAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TResult>> ListAsync<TResult>(
            Expression<Func<T, TResult>> selector,
            CancellationToken cancellationToken = default);

        Task<PaginatedResult<TResult>> PagedListAsync<TResult>(
            PaginationRequest pagination,
            Expression<Func<T, TResult>> selector,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default);
    }
}
