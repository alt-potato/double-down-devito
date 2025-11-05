using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Api.Models.Interfaces;

namespace Project.Api.Models;

public class Game : IEntity<Guid>, ITimestamped
{
    public Game()
    {
        Id = Guid.CreateVersion7();
    }

    public Guid Id { get; set; }

    [MaxLength(64)]
    public required string GameMode { get; set; } // must be one of Utilities.Constants.GameModes

    public string GameConfig { get; set; } = "";

    public required string GameState { get; set; }

    public string DeckId { get; set; } = ""; // TODO: move to gamestate

    public int Round { get; set; }

    public string State { get; set; } = "";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public virtual ICollection<GamePlayer> GamePlayers { get; set; } = [];

    public virtual ICollection<Hand> Hands { get; set; } = [];

    public byte[] RowVersion { get; set; } = []; // concurrency
}

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.Property(r => r.RowVersion).IsRowVersion();
    }
}
