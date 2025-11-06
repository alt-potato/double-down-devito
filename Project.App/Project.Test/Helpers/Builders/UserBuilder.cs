using Project.Api.Models;

namespace Project.Test.Helpers.Builders;

public class UserBuilder
{
    private readonly User _user;

    public UserBuilder()
    {
        // start with a valid, empty user
        _user = new User
        {
            Id = Guid.CreateVersion7(),
            Name = "Test User",
            Email = "test@example.com",
            Balance = 1000.0,
            AvatarUrl = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
    }

    public UserBuilder WithId(Guid id)
    {
        _user.Id = id;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _user.Name = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _user.Email = email;
        return this;
    }

    public UserBuilder WithBalance(double balance)
    {
        _user.Balance = balance;
        return this;
    }

    public UserBuilder WithAvatarUrl(string? avatarUrl)
    {
        _user.AvatarUrl = avatarUrl;
        return this;
    }

    public UserBuilder CreatedAt(DateTimeOffset createdAt)
    {
        _user.CreatedAt = createdAt;
        return this;
    }

    public UserBuilder UpdatedAt(DateTimeOffset updatedAt)
    {
        _user.UpdatedAt = updatedAt;
        return this;
    }

    public User Build()
    {
        return _user;
    }

    /// <summary>
    /// Allows implicit conversion from UserBuilder to User without an explicit call to Build().
    /// </summary>
    public static implicit operator User(UserBuilder builder) => builder.Build();
}
