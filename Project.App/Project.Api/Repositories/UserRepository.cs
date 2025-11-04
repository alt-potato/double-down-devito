using Microsoft.EntityFrameworkCore;
using Project.Api.Data;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    /// <summary>
    /// Get all users.
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }

    /// <summary>
    /// Get a user by their ID. Returns a read-only user.
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    /// <summary>
    /// Get a user by their email.
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var normalized = email.Trim().ToLower(); // normalize emails to lowercase
        return await _context
            .Users.AsNoTracking() // don't track changes
            .FirstOrDefaultAsync(u => u.Email == normalized);
    }

    /// <summary>
    /// Create a new user.
    /// </summary>
    public async Task<User> CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Update an existing user.
    /// </summary>
    public async Task<User> UpdateUserAsync(User user)
    {
        var existingUser =
            await _context.Users.FindAsync(user.Id)
            ?? throw new NotFoundException($"User with ID {user.Id} not found");

        _context.Entry(existingUser).CurrentValues.SetValues(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The user you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingUser;
    }

    /// <summary>
    /// Update user balance.
    /// </summary>
    public async Task<User> UpdateUserBalanceAsync(Guid id, double balance)
    {
        var existingUser =
            await _context.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        existingUser.Balance = balance;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The user you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingUser;
    }

    /// <summary>
    /// Delete a user by their ID.
    /// </summary>
    public async Task<User> DeleteUserAsync(Guid id)
    {
        var existingUser =
            await _context.Users.FindAsync(id)
            ?? throw new NotFoundException($"User with ID {id} not found");

        _context.Users.Remove(existingUser);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The user you are trying to update has been modified by another user. Please refresh and try again."
            );
        }

        return existingUser;
    }
}
