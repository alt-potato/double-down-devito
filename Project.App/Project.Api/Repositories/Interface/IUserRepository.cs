using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IUserRepository
{
    /// <summary>
    /// Get all users.
    /// </summary>
    Task<IReadOnlyList<User>> GetAllAsync(int? skip = null, int? take = null);

    /// <summary>
    /// Get a user by their ID.
    /// </summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Get a user by their email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Check if a user exists by their ID.
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Create a new user.
    /// </summary>
    Task<User> CreateAsync(User user);

    /// <summary>
    /// Update an existing user.
    /// </summary>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Update user balance.
    /// </summary>
    Task<User> UpdateBalanceAsync(Guid id, double balance);

    /// <summary>
    /// Delete a user by their ID.
    /// </summary>
    Task<User> DeleteAsync(Guid id);
}
