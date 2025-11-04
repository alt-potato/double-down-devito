using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Api.Models;

public class Hand
{
    public Hand()
    {
        Id = Guid.CreateVersion7();
    }

    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid GameId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public int Order { get; set; } // order of the player in the game

    [Required]
    public int HandNumber { get; set; } = 0; // number of the hand for the player

    [Required]
    public long Bet { get; set; }

    public virtual GamePlayer? GamePlayer { get; set; }

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class HandConfiguration : IEntityTypeConfiguration<Hand>
{
    public void Configure(EntityTypeBuilder<Hand> builder)
    {
        builder.Property(r => r.RowVersion).IsRowVersion();

        builder
            .HasOne(h => h.GamePlayer)
            .WithMany(gp => gp.Hands)
            .HasForeignKey(h => new { h.GameId, h.UserId });
    }
}
