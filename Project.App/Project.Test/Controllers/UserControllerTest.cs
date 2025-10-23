using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Project.Api.Controllers;
using Project.Api.Models;
using Project.Api.Services.Interface;

namespace Project.Api.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOkResult_WithUsers()
        {
            // Arrange
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Sneha",
                    Email = "sneha@example.com",
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Leo",
                    Email = "leo@example.com",
                },
            };
            _mockService.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUsers = Assert.IsAssignableFrom<IEnumerable<User>>(okResult.Value);
            Assert.Equal(2, ((List<User>)returnUsers).Count);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Name = "Sneha",
                Email = "sneha@example.com",
            };
            _mockService.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(userId, returnUser.Id);
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockService.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserById(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateUser_ReturnsCreatedAtAction()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Sneha",
                Email = "sneha@example.com",
            };
            _mockService.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnUser = Assert.IsType<User>(createdResult.Value);
            Assert.Equal(user.Id, returnUser.Id);
        }

        [Fact]
        public async Task UpdateUser_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var existingUser = new User { Id = userId, Name = "Old Name" };
            var updatedUser = new User { Id = userId, Name = "New Name" };

            _mockService.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(existingUser);
            _mockService
                .Setup(r => r.UpdateUserAsync(userId, It.IsAny<User>()))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateUser(userId, updatedUser);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUser_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, Name = "Sneha" };

            _mockService.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockService.Setup(r => r.DeleteUserAsync(userId)).ReturnsAsync(true);

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
            _mockService.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsBadRequest_WhenBodyIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            UpdateBalanceRequest? body = null;

            // Act
            var result = await _controller.UpdateBalance(userId, body);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUnauthorized_WhenNoEmailClaim()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var body = new UpdateBalanceRequest(100.0);

            // Setup empty claims
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            // Act
            var result = await _controller.UpdateBalance(userId, body);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUnauthorized_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var body = new UpdateBalanceRequest(100.0);
            var email = "test@example.com";

            // Setup claims with email
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.UpdateBalance(userId, body);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsForbidden_WhenDifferentUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var differentUserId = Guid.NewGuid();
            var body = new UpdateBalanceRequest(100.0);
            var email = "test@example.com";

            // Setup claims with email
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            var user = new User { Id = differentUserId, Email = email };
            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _controller.UpdateBalance(userId, body);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var body = new UpdateBalanceRequest(100.0);
            var email = "test@example.com";

            // Setup claims with email
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            var user = new User { Id = userId, Email = email };
            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);
            var updatedUser = new User
            {
                Id = userId,
                Email = email,
                Balance = body.Balance,
            };
            _mockService
                .Setup(s => s.UpdateUserBalanceAsync(userId, body.Balance))
                .ReturnsAsync(updatedUser);

            // Act
            var result = await _controller.UpdateBalance(userId, body);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Me_ReturnsUnauthorized_WhenNoEmailClaim()
        {
            // Arrange
            var claims = new List<Claim>();
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            // Act
            var result = await _controller.Me(_mockService.Object);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Me_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            var email = "test@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync((User?)null);

            // Act
            var result = await _controller.Me(_mockService.Object);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Me_ReturnsOk_WhenUserFound()
        {
            // Arrange
            var email = "test@example.com";
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal },
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = "Test User",
                Balance = 100.0,
                AvatarUrl = "https://example.com/avatar.jpg",
            };
            _mockService.Setup(s => s.GetUserByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _controller.Me(_mockService.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var expected = new
            {
                id = user.Id,
                name = user.Name,
                email = user.Email,
                balance = user.Balance,
                avatarUrl = user.AvatarUrl,
            };
            okResult.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = Guid.NewGuid() }; // Different ID

            // Act
            var result = await _controller.UpdateUser(userId, user);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateUser_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Name", "Name is required");
            var user = new User();

            // Act
            var result = await _controller.CreateUser(user);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }
}
