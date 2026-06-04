using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RiskTrace.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        IQueryable<T> Query(string sqlQuery, params object[] parameters);

        Task AddAsync(T entity, CancellationToken cancellationToken = default);

        Task <IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default); 

        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

        Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
        
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
    }
}
