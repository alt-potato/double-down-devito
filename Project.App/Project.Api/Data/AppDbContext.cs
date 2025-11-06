using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Api.Models;
using Project.Api.Models.Interface;

namespace Project.Api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        ChangeTracker.Tracked += OnEntityTracked;
        ChangeTracker.StateChanged += OnEntityStateChanged;
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomPlayer> RoomPlayers { get; set; }
    public DbSet<Game> Games { get; set; }
    public DbSet<GamePlayer> GamePlayers { get; set; }
    public DbSet<Hand> Hands { get; set; }

    /// <summary>
    /// Provides the configuration for TradeHubContext models.
    /// Any custom configurations should be defined in <see cref="OnModelCreatingPartial"/>.
    /// DO NOT MODIFY THIS METHOD.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    /// <summary>
    /// Sets the CreatedAt and UpdatedAt properties for new entities that implement ITimestamped.
    /// </summary>
    private void OnEntityTracked(object? sender, EntityTrackedEventArgs e)
    {
        if (
            !e.FromQuery
            && e.Entry.State == EntityState.Added
            && e.Entry.Entity is ITimestamped entity
        )
        {
            var now = DateTimeOffset.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
        }
    }

    /// <summary>
    /// Sets the UpdatedAt property for modified entities that implement ITimestamped.
    /// </summary>
    private void OnEntityStateChanged(object? sender, EntityStateChangedEventArgs e)
    {
        if (e.NewState == EntityState.Modified && e.Entry.Entity is ITimestamped entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
