using AutoMapper;
using Project.Api.DTOs;
using Project.Api.Models;
using Project.Api.Repositories.Interface;
using Project.Api.Services.Interface;
using Project.Api.Utilities;
using Project.Api.Utilities.Extensions;

namespace Project.Api.Services
{
    public class UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        : IUserService
    {
        private readonly IUnitOfWork _uow = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<IReadOnlyList<UserDTO>> GetAllUsersAsync()
        {
            _logger.LogDebug("Getting all users...");

            IReadOnlyList<User> users = await _uow.Users.GetAllAsync();
            return _mapper.Map<IReadOnlyList<UserDTO>>(users);
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            _logger.LogDebug("Getting user {userId}...", userId);

            User? user = await _uow.Users.GetByIdAsync(userId);

            if (user == null)
            {
                _logger.LogInformation("User {userId} not found.", userId);
                return null;
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            _logger.LogDebug("Getting user by email {email}...", email);

            User? user = await _uow.Users.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogInformation("User with email {email} not found.", email);
                return null;
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO dto)
        {
            _logger.LogDebug("Creating a new user...");

            User user = _mapper.Map<User>(dto);
            user.Id = Guid.CreateVersion7();
            // timestamps set automatically

            User createdUser =
                await _uow.Users.CreateAsync(user)
                ?? _logger.LogAndThrow<User>(
                    new InternalServerException("Unable to create user."),
                    $"Unable to create user with data {dto}."
                );

            _logger.LogInformation("User {userId} created!", createdUser.Id);
            return _mapper.Map<UserDTO>(createdUser);
        }

        public async Task<UserDTO> UpdateUserAsync(Guid userId, UpdateUserDTO user)
        {
            _logger.LogDebug("Updating user {userId}...", userId);

            User userToUpdate =
                await _uow.Users.GetByIdAsync(userId)
                ?? _logger.LogAndThrow<User>(
                    new NotFoundException($"User with ID {userId} not found.")
                );

            _mapper.Map(user, userToUpdate);

            User updatedUser =
                await _uow.Users.UpdateAsync(userToUpdate)
                ?? _logger.LogAndThrow<User>(
                    new InternalServerException("Unable to update user."),
                    $"Unable to update user with data {user}."
                );

            return _mapper.Map<UserDTO>(updatedUser);
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            _logger.LogDebug("Deleting user {userId}", userId);

            bool success = await _uow.Users.DeleteAsync(userId) != null;

            if (!success)
            {
                _logger.LogWarning("User {userId} not found", userId);
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            return success;
        }

        public async Task<UserDTO> UpdateUserBalanceAsync(Guid userId, double newBalance)
        {
            _logger.LogDebug(
                "Updating balance for user {userId} to {newBalance}...",
                userId,
                newBalance
            );

            // enforce balance > 0
            if (newBalance <= 0)
            {
                _logger.LogWarning("Balance must be greater than 0.");
                throw new BadRequestException("Balance must be greater than 0.");
            }

            // throws NotFoundException if user not found
            User user = await _uow.Users.UpdateBalanceAsync(userId, newBalance);

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpsertGoogleUserByEmailAsync(
            string email,
            string? name,
            string? avatarUrl
        )
        {
            _logger.LogDebug("Upserting Google user by email {Email}", email);

            User? user = await _uow.Users.GetByEmailAsync(email);
            if (user == null)
            {
                // if user does not exist, create a new one

                user = new User
                {
                    Id = Guid.CreateVersion7(),
                    Email = email,
                    Name = string.IsNullOrWhiteSpace(name) ? email : name!,
                    AvatarUrl = avatarUrl,
                };

                User newUser = await _uow.Users.CreateAsync(user);

                _logger.LogInformation("New user {userId} created through Google!", newUser.Id);
                return _mapper.Map<UserDTO>(newUser);
            }

            // update lightweight profile fields
            if (!string.IsNullOrWhiteSpace(name) && !name.Equals(user.Name))
                user.Name = name!;
            if (!string.IsNullOrWhiteSpace(avatarUrl))
                user.AvatarUrl = avatarUrl;

            User updatedUser = await _uow.Users.UpdateAsync(user);

            _logger.LogInformation("User {userId} updated through Google!", user.Id);

            return _mapper.Map<UserDTO>(updatedUser);
        }

        public async Task<UserDTO?> GetByEmailAsync(string email)
        {
            _logger.LogDebug("Getting user by email {email}...", email);

            return _mapper.Map<UserDTO>(await _uow.Users.GetByEmailAsync(email));
        }
    }
}
