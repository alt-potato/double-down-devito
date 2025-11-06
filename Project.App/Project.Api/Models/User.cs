using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Models.Interface;

namespace Project.Api.Models;

public class User : IEntity<Guid>, ITimestamped
{
    public User()
    {
        Id = Guid.CreateVersion7();
    }

    [Key]
    public Guid Id { get; set; }

    [Required, MaxLength(256)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(256)]
    public string Email { get; set; } = null!;

    public double Balance { get; set; } = 1000;

    [MaxLength(512)]
    public string? AvatarUrl { get; set; } // we will send this to the front for our pfp

    public ICollection<RoomPlayer> RoomPlayers { get; set; } = [];

    public ICollection<GamePlayer> GamePlayers { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
