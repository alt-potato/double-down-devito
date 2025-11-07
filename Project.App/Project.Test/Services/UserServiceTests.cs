using FluentAssertions;
using Moq;
using Project.Api.DTOs;
using Project.Api.Models;
using Project.Api.Services;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Services;

public class UserServiceTests : ServiceTestBase<UserService>
{
    private readonly UserService _sut; // service under test

    public UserServiceTests()
    {
        _sut = new UserService(_mockUoW.Object, _mockMapper.Object, _mockLogger.Object);
    }

    #region GetAllUsersAsync Tests

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers_WhenSuccessful()
    {
        // Arrange
        var users = new List<User>
        {
            new UserBuilder().WithId(Guid.NewGuid()).WithName("User 1").Build(),
            new UserBuilder().WithId(Guid.NewGuid()).WithName("User 2").Build(),
            new UserBuilder().WithId(Guid.NewGuid()).WithName("User 3").Build(),
        };
        var userDtos = users.Select(u => new UserDTO { Id = u.Id, Name = u.Name }).ToList();

        _mockUserRepository.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(users);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<UserDTO>>(users)).Returns(userDtos);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(userDtos);
        _mockUserRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<UserDTO>>(users), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        var emptyUsers = new List<User>();
        _mockUserRepository.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(emptyUsers);
        _mockMapper.Setup(m => m.Map<IReadOnlyList<UserDTO>>(emptyUsers)).Returns([]);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().BeEmpty();
        _mockUserRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
        _mockMapper.Verify(m => m.Map<IReadOnlyList<UserDTO>>(emptyUsers), Times.Once);
    }

    #endregion

    #region GetUserByIdAsync Tests

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUserDTO_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserBuilder().WithId(userId).Build();
        var userDto = new UserDTO
        {
            Id = userId,
            Name = user.Name,
            Email = user.Email,
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(userDto);
        result!.Id.Should().Be(userId);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    #endregion

    #region GetUserByEmailAsync Tests

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUserDTO_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new UserBuilder().WithEmail(email).Build();
        var userDto = new UserDTO
        {
            Id = user.Id,
            Email = email,
            Name = user.Name,
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(userDto);
        result!.Email.Should().Be(email);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(user), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_CreatesUser_WhenDtoIsValid()
    {
        // Arrange
        var createDto = new CreateUserDTO
        {
            Name = "Test User",
            Email = "test@example.com",
            AvatarUrl = "https://example.com/avatar.jpg",
        };
        var userFromMapper = new User { Name = createDto.Name, Email = createDto.Email };
        var createdUser = new UserBuilder()
            .WithId(Guid.NewGuid())
            .WithName(createDto.Name)
            .WithEmail(createDto.Email)
            .WithAvatarUrl(createDto.AvatarUrl)
            .Build();
        var resultDto = new UserDTO
        {
            Id = createdUser.Id,
            Name = createDto.Name,
            Email = createDto.Email,
        };

        _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(userFromMapper);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(createdUser)).Returns(resultDto);

        // Act
        var result = await _sut.CreateUserAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(createdUser.Id);
        result.Name.Should().Be(createDto.Name);
        result.Email.Should().Be(createDto.Email);
        _mockMapper.Verify(m => m.Map<User>(createDto), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(createdUser), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_ThrowsInternalServerException_WhenCreationFails()
    {
        // Arrange
        var createDto = new CreateUserDTO { Name = "Test User", Email = "test@example.com" };
        var userFromMapper = new User { Name = createDto.Name, Email = createDto.Email };

        _mockMapper.Setup(m => m.Map<User>(createDto)).Returns(userFromMapper);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync((User)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InternalServerException>(() => _sut.CreateUserAsync(createDto));

        _mockMapper.Verify(m => m.Map<User>(createDto), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    #endregion

    #region UpdateUserAsync Tests

    [Fact]
    public async Task UpdateUserAsync_UpdatesUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserBuilder().WithId(userId).Build();
        var updateDto = new UpdateUserDTO
        {
            Name = "Updated Name",
            AvatarUrl = "https://example.com/new-avatar.jpg",
        };
        var updatedUser = new UserBuilder()
            .WithId(userId)
            .WithName(updateDto.Name)
            .WithAvatarUrl(updateDto.AvatarUrl)
            .Build();
        var resultDto = new UserDTO
        {
            Id = userId,
            Name = updateDto.Name,
            AvatarUrl = updateDto.AvatarUrl,
        };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(It.IsAny<User>())).Returns(resultDto);

        // Act
        var result = await _sut.UpdateUserAsync(userId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be(updateDto.Name);
        result.AvatarUrl.Should().Be(updateDto.AvatarUrl);
        _mockMapper.Verify(m => m.Map(updateDto, existingUser), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(It.IsAny<User>()), Times.Once);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updateDto = new UpdateUserDTO { Name = "Updated Name" };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateUserAsync(userId, updateDto));

        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsInternalServerException_WhenUpdateFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingUser = new UserBuilder().WithId(userId).Build();
        var updateDto = new UpdateUserDTO { Name = "Updated Name" };

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InternalServerException>(() =>
            _sut.UpdateUserAsync(userId, updateDto)
        );

        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(existingUser), Times.Once);
    }

    #endregion

    #region DeleteUserAsync Tests

    [Fact]
    public async Task DeleteUserAsync_ReturnsTrue_WhenUserIsDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deletedUser = new UserBuilder().WithId(userId).Build();
        _mockUserRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync(deletedUser);

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.DeleteAsync(userId)).ReturnsAsync((User)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _sut.DeleteUserAsync(userId));

        _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    #endregion

    #region UpdateUserBalanceAsync Tests

    [Fact]
    public async Task UpdateUserBalanceAsync_UpdatesBalance_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newBalance = 1500.0;
        var updatedUser = new UserBuilder().WithId(userId).WithBalance(newBalance).Build();
        var resultDto = new UserDTO { Id = userId, Balance = newBalance };

        _mockUserRepository
            .Setup(r => r.UpdateBalanceAsync(userId, newBalance))
            .ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpdateUserBalanceAsync(userId, newBalance);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Balance.Should().Be(newBalance);
        _mockUserRepository.Verify(r => r.UpdateBalanceAsync(userId, newBalance), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(updatedUser), Times.Once);
    }

    [Fact]
    public async Task UpdateUserBalanceAsync_ThrowsBadRequestException_WhenBalanceIsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var zeroBalance = 0.0;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateUserBalanceAsync(userId, zeroBalance)
        );

        _mockUserRepository.Verify(
            r => r.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<double>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateUserBalanceAsync_ThrowsBadRequestException_WhenBalanceIsNegative()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var negativeBalance = -100.0;

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _sut.UpdateUserBalanceAsync(userId, negativeBalance)
        );

        _mockUserRepository.Verify(
            r => r.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<double>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateUserBalanceAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newBalance = 1500.0;

        _mockUserRepository
            .Setup(r => r.UpdateBalanceAsync(userId, newBalance))
            .ThrowsAsync(new NotFoundException($"User with ID {userId} not found."));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateUserBalanceAsync(userId, newBalance)
        );

        _mockUserRepository.Verify(r => r.UpdateBalanceAsync(userId, newBalance), Times.Once);
    }

    #endregion

    #region UpsertGoogleUserByEmailAsync Tests

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_CreatesNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "newuser@example.com";
        var name = "New Google User";
        var avatarUrl = "https://example.com/google-avatar.jpg";
        var newUser = new UserBuilder()
            .WithEmail(email)
            .WithName(name)
            .WithAvatarUrl(avatarUrl)
            .Build();
        var resultDto = new UserDTO
        {
            Id = newUser.Id,
            Email = email,
            Name = name,
            AvatarUrl = avatarUrl,
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(newUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(newUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, name, avatarUrl);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(newUser.Id);
        result.Email.Should().Be(email);
        result.Name.Should().Be(name);
        result.AvatarUrl.Should().Be(avatarUrl);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(newUser), Times.Once);
    }

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_UpdatesExistingUser_WhenUserExists()
    {
        // Arrange
        var email = "existing@example.com";
        var existingUser = new UserBuilder().WithEmail(email).WithName("Old Name").Build();
        var updatedUser = new UserBuilder().WithEmail(email).WithName("New Name").Build();
        var resultDto = new UserDTO
        {
            Id = existingUser.Id,
            Email = email,
            Name = "New Name",
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, "New Name", null);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingUser.Id);
        result.Name.Should().Be("New Name");
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(updatedUser), Times.Once);
    }

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_UpdatesName_WhenNameIsProvidedAndDifferent()
    {
        // Arrange
        var email = "test@example.com";
        var existingUser = new UserBuilder().WithEmail(email).WithName("Old Name").Build();
        var updatedUser = new UserBuilder().WithEmail(email).WithName("New Name").Build();
        var resultDto = new UserDTO
        {
            Id = existingUser.Id,
            Email = email,
            Name = "New Name",
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, "New Name", null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Name == "New Name")),
            Times.Once
        );
    }

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_UpdatesAvatarUrl_WhenAvatarUrlIsProvided()
    {
        // Arrange
        var email = "test@example.com";
        var existingUser = new UserBuilder().WithEmail(email).WithAvatarUrl(null).Build();
        var updatedUser = new UserBuilder()
            .WithEmail(email)
            .WithAvatarUrl("https://example.com/new-avatar.jpg")
            .Build();
        var resultDto = new UserDTO
        {
            Id = existingUser.Id,
            Email = email,
            AvatarUrl = "https://example.com/new-avatar.jpg",
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(
            email,
            null,
            "https://example.com/new-avatar.jpg"
        );

        // Assert
        result.Should().NotBeNull();
        result.AvatarUrl.Should().Be("https://example.com/new-avatar.jpg");
        _mockUserRepository.Verify(
            r =>
                r.UpdateAsync(
                    It.Is<User>(u => u.AvatarUrl == "https://example.com/new-avatar.jpg")
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_DoesNotUpdateName_WhenNameIsNullOrEmpty()
    {
        // Arrange
        var email = "test@example.com";
        var existingUser = new UserBuilder().WithEmail(email).WithName("Original Name").Build();
        var updatedUser = new UserBuilder().WithEmail(email).WithName("Original Name").Build();
        var resultDto = new UserDTO
        {
            Id = existingUser.Id,
            Email = email,
            Name = "Original Name",
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Original Name");
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Name == "Original Name")),
            Times.Once
        );
    }

    [Fact]
    public async Task UpsertGoogleUserByEmailAsync_DoesNotUpdateName_WhenNameIsSame()
    {
        // Arrange
        var email = "test@example.com";
        var existingUser = new UserBuilder().WithEmail(email).WithName("Same Name").Build();
        var updatedUser = new UserBuilder().WithEmail(email).WithName("Same Name").Build();
        var resultDto = new UserDTO
        {
            Id = existingUser.Id,
            Email = email,
            Name = "Same Name",
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);
        _mockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync(updatedUser);
        _mockMapper.Setup(m => m.Map<UserDTO>(updatedUser)).Returns(resultDto);

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, "Same Name", null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Same Name");
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Name == "Same Name")),
            Times.Once
        );
    }

    #endregion

    #region GetByEmailAsync Tests

    [Fact]
    public async Task GetByEmailAsync_ReturnsUserDTO_WhenUserExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new UserBuilder().WithEmail(email).Build();
        var userDto = new UserDTO
        {
            Id = user.Id,
            Email = email,
            Name = user.Name,
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);
        _mockMapper.Setup(m => m.Map<UserDTO>(user)).Returns(userDto);

        // Act
        var result = await _sut.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(userDto);
        result!.Email.Should().Be(email);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        _mockMapper.Verify(m => m.Map<UserDTO>(user), Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.GetByEmailAsync(email);

        // Assert
        result.Should().BeNull();
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    #endregion
}
