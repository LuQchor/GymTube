using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GymTube.API.Repositories;
using GymTube.API.Domain;

namespace GymTube.API.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected Guid? GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var userId))
                return userId;
            return null;
        }

        protected async Task<User?> GetCurrentUserAsync(IUserRepository userRepository)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return null;
            return await userRepository.GetByIdAsync(userId.Value);
        }

        // --- Helper metode za validacije i response ---
        protected IActionResult NotFoundIfNull(object? obj, string message = "Not found.")
        {
            if (obj == null) return NotFound(new { error = message });
            return null!;
        }
        protected IActionResult ForbidIf(bool condition, string message = "Forbidden.")
        {
            if (condition) return Forbid(message);
            return null!;
        }
        protected IActionResult BadRequestError(string message)
        {
            return BadRequest(new { error = message });
        }
        protected IActionResult SuccessMessage(string message)
        {
            return Ok(new { message });
        }
    }
}