using Project.Api.Models;

namespace Project.Api.Repositories.Interface;

public interface IUserRepository
{
    /// <summary>
    /// Get all users.
    /// </summary>
    Task<List<User>> GetAllUsersAsync();

    /// <summary>
    /// Get a user by their ID.
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id);

    /// <summary>
    /// Get a user by their email.
    /// </summary>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Create a new user.
    /// </summary>
    Task<User> CreateUserAsync(User user);

    /// <summary>
    /// Update an existing user.
    /// </summary>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// Update user balance.
    /// </summary>
    Task<User> UpdateUserBalanceAsync(Guid id, double balance);

    /// <summary>
    /// Delete a user by their ID.
    /// </summary>
    Task<User> DeleteUserAsync(Guid id);
}
