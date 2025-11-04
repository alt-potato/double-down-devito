using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Api.Models;

public class GamePlayer
{
    [Required]
    public Guid GameId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("GameId")]
    public virtual Game? Game { get; set; }

    [ForeignKey("UserId")]
    public virtual User? User { get; set; }

    [Required]
    public long Balance { get; set; }

    [Required]
    public long BalanceDelta { get; set; } = 0;

    public virtual ICollection<Hand> Hands { get; set; } = [];

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.HasKey(gp => new { gp.GameId, gp.UserId });

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
