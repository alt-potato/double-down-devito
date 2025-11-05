using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Models.Interfaces;

namespace Project.Api.Models;

public class Room : IEntity<Guid>, ITimestamped
{
    public Room()
    {
        Id = Guid.CreateVersion7();
    }

    [Key]
    public Guid Id { get; set; }

    public Guid HostId { get; set; }

    public virtual User? Host { get; set; } // navigation property

    public bool IsPublic { get; set; }

    public bool IsActive { get; set; }

    [MaxLength(512)]
    public string Description { get; set; } = "";

    public int MaxPlayers { get; set; }

    public int MinPlayers { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public Guid? GameId { get; set; }

    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    public virtual ICollection<RoomPlayer> RoomPlayers { get; set; } = [];

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
