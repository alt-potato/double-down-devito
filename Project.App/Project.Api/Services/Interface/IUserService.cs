using Project.Api.DTOs;
using Project.Api.Models;

namespace Project.Api.Services.Interface
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserDTO>> GetAllUsersAsync();

        Task<UserDTO?> GetUserByIdAsync(Guid userId);

        Task<UserDTO?> GetUserByEmailAsync(string email);

        Task<UserDTO> CreateUserAsync(CreateUserDTO user);

        Task<UserDTO> UpdateUserAsync(Guid userId, UpdateUserDTO user);

        Task<bool> DeleteUserAsync(Guid userId);

        Task<UserDTO> UpdateUserBalanceAsync(Guid userId, double balance);

        Task<UserDTO> UpsertGoogleUserByEmailAsync(string email, string? name, string? avatarUrl); //update + insert new google login to our db
    }
}
