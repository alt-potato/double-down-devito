using FluentAssertions;
using Moq;
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
        _sut = new UserService(_mockUoW.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllUsers_WhenSuccessful()
    {
        // Arrange
        var users = new List<User> { new UserBuilder(), new UserBuilder(), new UserBuilder() };
        _mockUserRepository.Setup(r => r.GetAllAsync(null, null)).ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().BeEquivalentTo(users);
        _mockUserRepository.Verify(r => r.GetAllAsync(null, null), Times.Once);
    }

    [Fact]
    public async Task GetAllUsers_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        _mockUserRepository
            .Setup(r => r.GetAllAsync(null, null))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(_sut.GetAllUsersAsync);
    }

    [Fact]
    public async Task GetUserById_ReturnsUser_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserBuilder().WithId(userId).Build();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(user);
        _mockUserRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserById_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.GetByIdAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetUserByIdAsync(userId));
    }

    [Fact]
    public async Task GetUserByEmail_ReturnsUser_WhenExists()
    {
        // Arrange
        var email = "test@example.com";
        var user = new UserBuilder().WithEmail(email).Build();
        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByEmailAsync(email);

        // Assert
        result.Should().BeEquivalentTo(user);
        _mockUserRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmail_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var email = "test@example.com";
        _mockUserRepository
            .Setup(r => r.GetByEmailAsync(email))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetUserByEmailAsync(email));
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedUser_WhenSuccessful()
    {
        // Arrange
        var newUser = new UserBuilder().WithName("Danny").WithEmail("danny@devito.net").Build();

        // Setup AddAsync to complete successfully
        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.FromResult(newUser));

        // Act
        var result = await _sut.CreateUserAsync(newUser);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Danny");
        result.Email.Should().Be("danny@devito.net");

        // Verify AddAsync was called exactly once with the expected user
        _mockUserRepository.Verify(
            r =>
                r.CreateAsync(It.Is<User>(u => u.Name == "Danny" && u.Email == "danny@devito.net")),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_ReturnsUpdatedUser_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserBuilder().WithId(userId).WithName("Updated Name").Build();
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.FromResult(user));

        // Act
        var result = await _sut.UpdateUserAsync(userId, user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Name.Should().Be("Updated Name");
        _mockUserRepository.Verify(
            r => r.UpdateAsync(It.Is<User>(u => u.Id == userId && u.Name == "Updated Name")),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateUser_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserBuilder().WithId(userId).WithName("Updated Name").Build();
        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.UpdateUserAsync(userId, user));
    }

    [Fact]
    public async Task DeleteUser_ReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserBuilder().WithId(userId).WithName("Deleted User").Build();
        _mockUserRepository.Setup(r => r.DeleteAsync(userId)).Returns(Task.FromResult(user));

        // Act
        var result = await _sut.DeleteUserAsync(userId);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(r => r.DeleteAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ThrowsException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository
            .Setup(r => r.DeleteAsync(userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteUserAsync(userId));
    }

    [Fact]
    public async Task UpdateUserBalance_ReturnsUpdatedUser_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startingUser = new UserBuilder().WithId(userId).WithBalance(1000.0).Build();
        var newBalance = 2000.0;
        var updatedUser = new UserBuilder().WithId(userId).WithBalance(newBalance).Build();

        _mockUserRepository.Setup(r => r.ExistsAsync(userId)).ReturnsAsync(true);
        _mockUserRepository
            .Setup(r => r.UpdateBalanceAsync(It.IsAny<Guid>(), It.IsAny<double>()))
            .Returns(Task.FromResult(updatedUser));

        // Act
        var result = await _sut.UpdateUserBalanceAsync(userId, newBalance);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Balance.Should().Be(newBalance);

        _mockUserRepository.Verify(r => r.ExistsAsync(userId), Times.Once);
        _mockUserRepository.Verify(r => r.UpdateBalanceAsync(userId, newBalance), Times.Once);
    }

    [Fact]
    public async Task UpdateUserBalance_ThrowsNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateUserBalanceAsync(userId, 2000.0)
        );
        exception.Message.Should().Contain($"User {userId} not found");
    }

    [Fact]
    public async Task UpsertGoogleUser_CreatesNewUser_WhenUserDoesNotExist()
    {
        // Arrange
        var email = "test@gmail.com";
        var name = "Test User";
        var avatarUrl = "http://example.com/avatar.jpg";
        var user = new UserBuilder()
            .WithEmail(email)
            .WithName(name)
            .WithAvatarUrl(avatarUrl)
            .Build();

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.FromResult(user));

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, name, avatarUrl);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(name);
        result.AvatarUrl.Should().Be(avatarUrl);
        _mockUserRepository.Verify(
            r =>
                r.CreateAsync(
                    It.Is<User>(u => u.Email == email && u.Name == name && u.AvatarUrl == avatarUrl)
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpsertGoogleUser_UpdatesExistingUser_WhenUserExists()
    {
        // Arrange
        var email = "test@gmail.com";
        var existingUser = new UserBuilder()
            .WithEmail(email)
            .WithName("Old Name")
            .WithAvatarUrl("old-avatar.jpg")
            .Build();
        var newName = "New Name";
        var newAvatarUrl = "new-avatar.jpg";

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(existingUser);

        _mockUserRepository
            .Setup(r => r.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.FromResult(existingUser));

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, newName, newAvatarUrl);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(newName);
        result.AvatarUrl.Should().Be(newAvatarUrl);
        _mockUserRepository.Verify(
            r =>
                r.UpdateAsync(
                    It.Is<User>(u =>
                        u.Email == email && u.Name == newName && u.AvatarUrl == newAvatarUrl
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpsertGoogleUser_UsesEmailAsName_WhenNameIsEmpty()
    {
        // Arrange
        var email = "test@gmail.com";
        string? name = null;
        var avatarUrl = "http://example.com/avatar.jpg";
        var user = new UserBuilder()
            .WithEmail(email)
            .WithName(email)
            .WithAvatarUrl(avatarUrl)
            .Build();

        _mockUserRepository.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        _mockUserRepository
            .Setup(r => r.CreateAsync(It.IsAny<User>()))
            .Returns(Task.FromResult(user));

        // Act
        var result = await _sut.UpsertGoogleUserByEmailAsync(email, name, avatarUrl);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        result.Name.Should().Be(email);
        result.AvatarUrl.Should().Be(avatarUrl);
    }
}
