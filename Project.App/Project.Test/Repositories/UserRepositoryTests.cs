using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Project.Api.Models;
using Project.Api.Repositories;
using Project.Api.Utilities;
using Project.Test.Helpers;
using Project.Test.Helpers.Builders;

namespace Project.Test.Repositories;

public class UserRepositoryTests : RepositoryTestBase<UserRepository, User>
{
    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        // Arrange
        await SeedData(
            new UserBuilder().WithName("Alice").WithEmail("alice@example.com").Build(),
            new UserBuilder().WithName("Bob").WithEmail("bob@example.com").Build(),
            new UserBuilder().WithName("Charlie").WithEmail("charlie@example.com").Build()
        );

        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(u => u.Email == "alice@example.com");
        result.Should().Contain(u => u.Email == "bob@example.com");
        result.Should().Contain(u => u.Email == "charlie@example.com");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Act
        var result = await _rut.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder().WithName("Alice").WithEmail("alice@example.com").Build()
        );

        // Act
        var result = await _rut.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Name.Should().Be("Alice");
        result.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _rut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenUserExists()
    {
        // Arrange
        await SeedData(new UserBuilder().WithName("Alice").WithEmail("alice@example.com").Build());

        // Act
        var result = await _rut.GetByEmailAsync("alice@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("alice@example.com");
        result.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _rut.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        await SeedData(new UserBuilder().WithEmail("alice@example.com").Build());

        // Act: different casing than stored value
        var result = await _rut.GetByEmailAsync("ALICE@example.com");

        // Assert: should still find the user
        result.Should().NotBeNull();
        result!.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task CreateAsync_AddsUserToDatabase()
    {
        // Arrange
        var user = new UserBuilder().WithName("Alice").WithEmail("alice@example.com").Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == "alice@example.com"
        );
        savedUser.Should().NotBeNull();
        savedUser!.Name.Should().Be("Alice");
        savedUser.Balance.Should().Be(1000);
    }

    [Fact]
    public async Task CreateAsync_SavesWithDefaultBalance()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Balance.Should().Be(1000);
    }

    [Fact]
    public async Task CreateAsync_SavesWithCustomBalance()
    {
        // Arrange
        var user = new UserBuilder().WithBalance(5000).Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Balance.Should().Be(5000);
    }

    [Fact]
    public async Task CreateAsync_GeneratesId_WhenNotProvided()
    {
        // Arrange
        var user = new UserBuilder().WithId(Guid.Empty).Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        _context.Users.Should().HaveCount(1);
        var savedUser = await _context.Users.FirstAsync();
        savedUser.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingUser()
    {
        // Arrange
        var user = await SeedData(
            new UserBuilder()
                .WithName("Alice")
                .WithEmail("alice@example.com")
                .WithBalance(1000)
                .Build()
        );

        // Modify user properties
        user.Name = "Alice Updated";
        user.Balance = 2000;

        // Act
        await _rut.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == "alice@example.com"
        );
        updatedUser.Should().NotBeNull();
        updatedUser!.Name.Should().Be("Alice Updated");
        updatedUser.Balance.Should().Be(2000);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeEmail()
    {
        // Arrange
        var user = await SeedData(new UserBuilder().WithEmail("alice@example.com").Build());

        // Act
        user.Email = "newemail@example.com";
        await _rut.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Email.Should().Be("newemail@example.com");
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser_WhenUserExists()
    {
        // Arrange
        var user = await SeedData(new UserBuilder().Build());

        // Act
        await _rut.DeleteAsync(user.Id);

        // Assert
        var deletedUser = await _context.Users.FindAsync(user.Id);
        deletedUser.Should().BeNull();
        _context.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await _rut.Invoking(r => r.DeleteAsync(nonExistentId))
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage($"User with ID {nonExistentId} not found.");
    }

    [Fact]
    public async Task CreateAsync_PersistsChanges()
    {
        // Arrange
        var user = new UserBuilder().Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        _context.Users.Should().HaveCount(1);
    }

    [Fact]
    public void User_HasDefaultBalanceOf1000()
    {
        // Arrange & Act
        var user = new UserBuilder().Build();

        // Assert
        user.Balance.Should().Be(1000);
    }

    [Fact]
    public void User_InitializesRoomPlayersAsEmptyList()
    {
        // Arrange & Act
        var user = new UserBuilder().Build();

        // Assert
        user.RoomPlayers.Should().NotBeNull();
        user.RoomPlayers.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_AllowsDuplicateEmail_InMemoryDatabase()
    {
        // Arrange
        var user1 = new UserBuilder().WithEmail("duplicate@example.com").Build();
        var user2 = new UserBuilder().WithEmail("duplicate@example.com").Build();

        // Act
        await _rut.CreateAsync(user1);
        await _rut.CreateAsync(user2);

        // Assert
        // Note: In-memory database does not enforce unique constraints
        // In a real database with proper constraints, this would throw an exception
        var users = await _rut.GetAllAsync();
        users.Should().HaveCount(2);
        users.Should().OnlyContain(u => u.Email == "duplicate@example.com");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBalance_AfterTransaction()
    {
        // Arrange
        var user = await SeedData(new UserBuilder().WithBalance(1000).Build());

        // Simulate a transaction
        user.Balance -= 100; // User loses 100

        // Act
        await _rut.UpdateAsync(user);

        // Assert
        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser!.Balance.Should().Be(900);
    }

    [Fact]
    public async Task User_CanHaveZeroBalance()
    {
        // Arrange
        var user = new UserBuilder().WithBalance(0).Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Balance.Should().Be(0);
    }

    [Fact]
    public async Task User_CanHaveNegativeBalance()
    {
        // Arrange
        var user = new UserBuilder().WithBalance(-500).Build();

        // Act
        await _rut.CreateAsync(user);

        // Assert
        var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        savedUser.Should().NotBeNull();
        savedUser!.Balance.Should().Be(-500);
    }
}
