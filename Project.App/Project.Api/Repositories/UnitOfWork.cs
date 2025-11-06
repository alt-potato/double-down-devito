using Project.Api.Data;
using Project.Api.Repositories.Interface;

namespace Project.Api.Repositories;

public class UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory) : IUnitOfWork
{
    private readonly ILogger<UnitOfWork> _logger = loggerFactory.CreateLogger<UnitOfWork>();

    public IUserRepository Users { get; private set; } =
        new UserRepository(context, loggerFactory.CreateLogger<UserRepository>());
    public IRoomRepository Rooms { get; private set; } =
        new RoomRepository(context, loggerFactory.CreateLogger<RoomRepository>());
    public IRoomPlayerRepository RoomPlayers { get; private set; } =
        new RoomPlayerRepository(context, loggerFactory.CreateLogger<RoomPlayerRepository>());
    public IGameRepository Games { get; private set; } =
        new GameRepository(context, loggerFactory.CreateLogger<GameRepository>());
    public IGamePlayerRepository GamePlayers { get; private set; } =
        new GamePlayerRepository(context, loggerFactory.CreateLogger<GamePlayerRepository>());
    public IHandRepository Hands { get; private set; } =
        new HandRepository(context, loggerFactory.CreateLogger<HandRepository>());

    public async Task<int> CommitAsync()
    {
        _logger.LogDebug("Committing transaction...");

        int result = await context.SaveChangesAsync();
        _logger.LogInformation("{Result} changes committed!", result);

        return result;
    }

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            context.Dispose();
        }
        disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
