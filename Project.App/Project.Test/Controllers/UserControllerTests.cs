using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Api.Controllers;
using Project.Api.DTOs;
using Project.Api.Services.Interface;
using Project.Api.Utilities;

namespace Project.Test.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockSvc;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockSvc = new Mock<IUserService>();
            _controller = new UserController(_mockSvc.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<UserDTO>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Sneha",
                    Email = "sneha@example.com",
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Leo",
                    Email = "leo@example.com",
                },
            };
            _mockSvc.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUsers = Assert.IsAssignableFrom<IEnumerable<UserDTO>>(okResult.Value);
            Assert.Equal(2, ((List<UserDTO>)returnUsers).Count);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserDTO
            {
                Id = userId,
                Name = "Sneha",
                Email = "sneha@example.com",
            };
            _mockSvc.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(userId, returnUser.Id);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockSvc.Setup(s => s.GetUserByIdAsync(userId)).ReturnsAsync((UserDTO?)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction()
        {
            // Arrange
            var createUserDto = new CreateUserDTO { Name = "Sneha", Email = "sneha@example.com" };
            var createdUser = new UserDTO
            {
                Id = Guid.NewGuid(),
                Name = "Sneha",
                Email = "sneha@example.com",
            };
            _mockSvc.Setup(s => s.CreateUserAsync(createUserDto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(createUserDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnUser = Assert.IsType<UserDTO>(createdResult.Value);
            Assert.Equal(createdUser.Id, returnUser.Id);
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDTO { Name = "New Name" };
            var updatedUser = new UserDTO { Id = userId, Name = "New Name" };

            _mockSvc.Setup(s => s.UpdateUserAsync(userId, updateUserDto)).ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUser = Assert.IsType<UserDTO>(okResult.Value);
            Assert.Equal(userId, returnUser.Id);
            Assert.Equal("New Name", returnUser.Name);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var updateUserDto = new UpdateUserDTO { Name = "New Name" };

            _mockSvc
                .Setup(s => s.UpdateUserAsync(userId, updateUserDto))
                .ReturnsAsync((UserDTO)null!);

            // Act
            var result = await _controller.UpdateUser(userId, updateUserDto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockSvc.Setup(s => s.DeleteUserAsync(userId)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockSvc
                .Setup(s => s.DeleteUserAsync(userId))
                .ThrowsAsync(new NotFoundException($"User with ID {userId} not found."));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
