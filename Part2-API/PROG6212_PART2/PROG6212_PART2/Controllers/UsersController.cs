
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using PROG6212_PART2.Authorization;
using PROG6212_PART2.Data;
using PROG6212_PART2.Models;

namespace PROG6212_PART2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/Users/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId.Value)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        // PUT: api/Users/me
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            UpdateProfileRequest request)
        {
            if (!SessionAuthorization.IsAuthenticated(HttpContext))
                return Unauthorized(new { message = "Please log in." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return Unauthorized();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
                return NotFound(new { message = "User not found." });

            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new
                {
                    message = "Full name and email are required."
                });
            }

            string email = request.Email.Trim().ToLowerInvariant();

            bool emailUsed = await _context.Users.AnyAsync(
                u => u.Email == email && u.UserId != user.UserId);

            if (emailUsed)
                return Conflict(new { message = "That email is already registered." });

            user.FullName = request.FullName.Trim();
            user.Email = email;

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (request.NewPassword.Length < 8)
                    return BadRequest(new
                    {
                        message = "The new password must contain at least 8 characters."
                    });

                user.PasswordHash = _passwordHasher.HashPassword(
                    user, request.NewPassword);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully.",
                user.UserId,
                user.FullName,
                user.Email,
                user.Role
            });
        }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
    }
}
