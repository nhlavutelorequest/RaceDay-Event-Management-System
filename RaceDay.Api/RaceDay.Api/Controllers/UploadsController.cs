using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class UploadsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _storage;
        private readonly ILogger<UploadsController> _logger;

        public UploadsController(RaceDayDbContext context, IBlobStorageService storage, ILogger<UploadsController> logger)
        {
            _context = context;
            _storage = storage;
            _logger = logger;
        }

        // POST /api/events/{id}/banner - Organiser only, must own the event
        [HttpPost("api/events/{id}/banner")]
        [SessionAuthorize("Organiser")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile file)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId)
                return StatusCode(403, new { message = "You can only upload a banner for your own events." });

            var (url, error) = await StoreAsync(file, "events");
            if (error != null) return error;

            ev.BannerImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Banner uploaded.", bannerImageUrl = url });
        }

        // POST /api/users/me/picture - any logged-in user, own profile picture
        [HttpPost("api/users/me/picture")]
        [SessionAuthorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == CurrentUserId);
            if (user == null) return NotFound(new { message = "User not found." });

            var (url, error) = await StoreAsync(file, "profiles");
            if (error != null) return error;

            user.ProfileImageUrl = url;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile picture updated.", profileImageUrl = url });
        }

        private async Task<(string? Url, IActionResult? Error)> StoreAsync(IFormFile file, string folder)
        {
            try
            {
                return (await _storage.UploadImageAsync(file, folder), null);
            }
            catch (ImageUploadException ex)
            {
                return (null, BadRequest(new { message = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed.");
                return (null, StatusCode(503, new { message = "Image storage is unavailable right now. Please try again later." }));
            }
        }
    }
}