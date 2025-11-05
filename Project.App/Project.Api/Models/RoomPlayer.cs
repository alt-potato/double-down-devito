using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Models.Interface;
using Project.Api.Utilities.Enums;

namespace Project.Api.Models;

public class RoomPlayer : ICompositeEntity<Guid, Guid>
{
    public Guid Key1 => RoomId; // for ICompositeEntity
    public Guid RoomId { get; set; }

    public Guid Key2 => UserId; // for ICompositeEntity
    public Guid UserId { get; set; }

    public virtual Room? Room { get; set; } // navigation
    public virtual User? User { get; set; } // navigation

    public Status Status { get; set; } = Status.Active;

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class RoomPlayerConfiguration : IEntityTypeConfiguration<RoomPlayer>
{
    public void Configure(EntityTypeBuilder<RoomPlayer> builder)
    {
        builder.HasKey(rp => new { rp.RoomId, rp.UserId });

        builder
            .HasOne(rp => rp.Room)
            .WithMany(r => r.RoomPlayers)
            .HasForeignKey(rp => rp.RoomId)
            .IsRequired();

        builder
            .HasOne(gp => gp.User)
            .WithMany(u => u.RoomPlayers)
            .HasForeignKey(rp => rp.UserId)
            .IsRequired();

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
