using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Api.Repositories.Interface;

namespace Project.Test.Helpers;

public class ServiceTestBase<T>
    where T : class
{
    protected readonly Mock<IUnitOfWork> _mockUoW = new();
    protected readonly Mock<IMapper> _mockMapper = new();
    protected readonly Mock<ILogger<T>> _mockLogger = new();

    protected readonly Mock<IUserRepository> _mockUserRepository = new();
    protected readonly Mock<IRoomRepository> _mockRoomRepository = new();
    protected readonly Mock<IRoomPlayerRepository> _mockRoomPlayerRepository = new();
    protected readonly Mock<IGameRepository> _mockGameRepository = new();
    protected readonly Mock<IGamePlayerRepository> _mockGamePlayerRepository = new();
    protected readonly Mock<IHandRepository> _mockHandRepository = new();

    protected ServiceTestBase()
    {
        // wire up mock repositories to mock uow
        _mockUoW.Setup(x => x.Users).Returns(_mockUserRepository.Object);
        _mockUoW.Setup(x => x.Rooms).Returns(_mockRoomRepository.Object);
        _mockUoW.Setup(x => x.RoomPlayers).Returns(_mockRoomPlayerRepository.Object);
        _mockUoW.Setup(x => x.Games).Returns(_mockGameRepository.Object);
        _mockUoW.Setup(x => x.GamePlayers).Returns(_mockGamePlayerRepository.Object);
        _mockUoW.Setup(x => x.Hands).Returns(_mockHandRepository.Object);

        // default setup for CommitAsync, can be overridden for failure scenarios
        _mockUoW.Setup(uow => uow.CommitAsync()).ReturnsAsync(1).Verifiable();
    }
}
