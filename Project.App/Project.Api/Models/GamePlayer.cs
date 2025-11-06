using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Models.Interface;

namespace Project.Api.Models;

public class GamePlayer : ICompositeEntity<Guid, Guid>
{
    public Guid Key1 => GameId; // for ICompositeEntity
    public Guid GameId { get; set; }

    public Guid Key2 => UserId; // for ICompositeEntity
    public Guid UserId { get; set; }

    public virtual Game? Game { get; set; } // navigation
    public virtual User? User { get; set; } // navigation

    [Required]
    public long Balance { get; set; }

    public long BalanceDelta { get; set; } = 0;

    public PlayerStatus Status { get; set; } = PlayerStatus.Active;

    public virtual ICollection<Hand> Hands { get; set; } = [];

    public byte[] RowVersion { get; set; } = []; // concurrency

    public enum PlayerStatus
    {
        Active, // currently playing the game
        Inactive, // did not move last turn, will be kicked next turn
        Away, // in the room, but not playing
        Left, // left the room
    }
}

public class GamePlayerConfiguration : IEntityTypeConfiguration<GamePlayer>
{
    public void Configure(EntityTypeBuilder<GamePlayer> builder)
    {
        builder.HasKey(gp => new { gp.GameId, gp.UserId });

        builder
            .HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlayers)
            .HasForeignKey(gp => gp.GameId)
            .IsRequired();

        builder
            .HasOne(gp => gp.User)
            .WithMany(u => u.GamePlayers)
            .HasForeignKey(gp => gp.UserId)
            .IsRequired();

        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
