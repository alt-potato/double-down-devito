using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Utilities.Enums;

namespace Project.Api.Models;

public class RoomPlayer
{
    [Required]
    public Guid RoomId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room? Room { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    public Status Status { get; set; } = Status.Active;

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class RoomPlayerConfiguration : IEntityTypeConfiguration<RoomPlayer>
{
    public void Configure(EntityTypeBuilder<RoomPlayer> builder)
    {
        builder.HasKey(rp => new { rp.RoomId, rp.UserId });

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
