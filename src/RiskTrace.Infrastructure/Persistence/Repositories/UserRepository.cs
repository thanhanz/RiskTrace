using Microsoft.EntityFrameworkCore;
using RiskTrace.Domain.Entities;
using RiskTrace.UseCases.Ports.Repositories;

namespace RiskTrace.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
