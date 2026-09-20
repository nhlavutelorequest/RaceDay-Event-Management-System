using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class CategoriesController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/events/{eventId}/categories - public
        [HttpGet("api/events/{eventId}/categories")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new { c.CategoryId, c.CategoryName, c.Description })
                .ToListAsync();

            return Ok(categories);
        }

        // POST /api/events/{eventId}/categories - Organiser only, must own the event
        [HttpPost("api/events/{eventId}/categories")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(int eventId, CreateCategoryDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var category = new Category
            {
                EventId = eventId,
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEvent), new { eventId }, new { category.CategoryId, category.CategoryName });
        }

        // DELETE /api/categories/{id} - Organiser only, must own the parent event
        [HttpDelete("api/categories/{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound(new { message = "Category not found." });
            if (category.Event!.OrganiserId != CurrentUserId) return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Category deleted successfully." });
        }
    }
}