using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public UsersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/users/me - any logged-in user views their own profile
        [HttpGet("me")]
        [SessionAuthorize]
        public async Task<IActionResult> GetMe()
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == CurrentUserId);

            if (user == null) return NotFound(new { message = "User not found." });

            return Ok(new
            {
                user.UserId,
                user.FullName,
                user.Email,
                user.PhoneNumber,
                Role = user.Role!.RoleName,
                user.CreatedAt
            });
        }

        // PUT /api/users/me - any logged-in user updates their own name and phone number
        [HttpPut("me")]
        [SessionAuthorize]
        public async Task<IActionResult> UpdateMe(UpdateProfileDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return NotFound(new { message = "User not found." });

            user.FullName = dto.FullName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated.", user.FullName, user.PhoneNumber });
        }
    }
}