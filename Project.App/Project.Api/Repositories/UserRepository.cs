using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    : Repository<User>(context, logger),
        IUserRepository
{
    /// <summary>
    /// Get all users.
    /// </summary>
    public async Task<IReadOnlyList<User>> GetAllAsync(int? skip = null, int? take = null) =>
        await base.GetAllAsync(skip: skip, take: take);

    /// <summary>
    /// Get a user by their ID. Returns a read-only user.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id) => await GetAsync(id);

    /// <summary>
    /// Get a user by their email.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.Equals(email.Trim(), StringComparison.InvariantCultureIgnoreCase)
        );

    /// <summary>
    /// Check if a user exists by their ID.
    /// </summary>
    public new async Task<bool> ExistsAsync(Guid id) => await base.ExistsAsync(id);

    /// <summary>
    /// Create a new user.
    /// </summary>
    public new async Task<User> CreateAsync(User user) => await base.CreateAsync(user);

    /// <summary>
    /// Update an existing user.
    /// </summary>
    public new async Task<User> UpdateAsync(User user) => await base.UpdateAsync(user);

    /// <summary>
    /// Update user balance with a new value.
    /// </summary>
    public async Task<User> UpdateBalanceAsync(Guid id, double balance) =>
        await UpdateAsync(id, u => u.Balance = balance);

    /// <summary>
    /// Delete a user by their ID.
    /// </summary>
    public new async Task<User> DeleteAsync(Guid id) => await base.DeleteAsync(id);
}
