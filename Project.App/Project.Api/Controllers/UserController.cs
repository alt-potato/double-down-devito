using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Api.DTOs;
using Project.Api.Services.Interface;

namespace Project.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        // GET: api/user

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound($"User with ID {id} not found.");
            return Ok(user);
        }

        // POST: api/user
        [HttpPost]
        public async Task<ActionResult<UserDTO>> CreateUser([FromBody] CreateUserDTO createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdUser = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDTO>> UpdateUser(
            Guid id,
            [FromBody] UpdateUserDTO updateUserDto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedUser = await _userService.UpdateUserAsync(id, updateUserDto);
            if (updatedUser == null)
                return NotFound($"User with ID {id} not found.");

            return Ok(updatedUser);
        }

        // PATCH: api/user/{id}
        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBalance(
            Guid id,
            [FromBody] UpdateBalanceRequest body
        )
        {
            if (body is null)
                return BadRequest("Missing body.");

            // only allow the logged-in user to change their own balance
            var emailClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(emailClaim))
                return Unauthorized();

            var me = await _userService.GetUserByEmailAsync(emailClaim);
            if (me is null)
                return Unauthorized();

            if (me.Id != id)
                return Forbid(); // 403 if trying to edit someone else

            await _userService.UpdateUserBalanceAsync(id, body.Balance);
            return NoContent();
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (Project.Api.Utilities.NotFoundException)
            {
                return NotFound($"User with ID {id} not found.");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me([FromServices] IUserService users)
        {
            // Identity comes from the auth cookie
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email);
            if (email is null)
                return Unauthorized();

            var u = await users.GetUserByEmailAsync(email.Value);
            if (u is null)
                return NotFound();

            return Ok(u);
        }
    }

    public record UpdateBalanceRequest(double Balance);
}
