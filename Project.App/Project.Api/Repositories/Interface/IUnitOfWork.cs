namespace Project.Api.Repositories.Interface;

/// <summary>
/// Represents a unit of work for the application, encapsulating a single transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoomRepository Rooms { get; }
    IRoomPlayerRepository RoomPlayers { get; }
    IGameRepository Games { get; }
    IGamePlayerRepository GamePlayers { get; }
    IHandRepository Hands { get; }

    /// <summary>
    /// Commit the changes using a single transaction.
    /// </summary>
    /// <remarks>
    /// This method should only be called at the highest level of the unit of work, likely the controller. This ensures
    /// that if the application throws an exception, all changes are rolled back.
    /// REMEMBER TO ACTUALLY CALL THIS METHOD, OR THE CHANGES WILL NOT BE SAVED TO THE DATABASE.
    /// </remarks>
    Task<int> CommitAsync();
}
