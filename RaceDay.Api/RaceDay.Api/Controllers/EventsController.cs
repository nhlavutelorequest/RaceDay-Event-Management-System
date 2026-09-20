using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET /api/events - public
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.EventType,
                    e.BannerImageUrl,
                    OrganiserName = e.Organiser!.FullName
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET /api/events/{id} - public, includes categories
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .Where(e => e.EventId == id)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location,
                    e.DistanceKm,
                    e.EventType,
                    e.BannerImageUrl,
                    OrganiserName = e.Organiser!.FullName,
                    Categories = e.Categories.Select(c => new { c.CategoryId, c.CategoryName, c.Description })
                })
                .FirstOrDefaultAsync();

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(ev);
        }

        // POST /api/events - Organiser only
        [HttpPost]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(CreateEventDto dto)
        {
            if (!EventTypes.IsValid(dto.EventType))
                return BadRequest(new { message = "EventType must be 'Run', 'Walk', or 'Cycle'." });

            var ev = new Event
            {
                OrganiserId = CurrentUserId!.Value,
                Name = dto.Name,
                Description = dto.Description,
                EventDate = dto.EventDate,
                Location = dto.Location,
                DistanceKm = dto.DistanceKm,
                EventType = dto.EventType,
                BannerImageUrl = dto.BannerImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId, ev.Name });
        }

        // PUT /api/events/{id} - Organiser only, must own the event
        [HttpPut("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, UpdateEventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            if (!EventTypes.IsValid(dto.EventType))
                return BadRequest(new { message = "EventType must be 'Run', 'Walk', or 'Cycle'." });

            ev.Name = dto.Name;
            ev.Description = dto.Description;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.DistanceKm = dto.DistanceKm;
            ev.EventType = dto.EventType;
            ev.BannerImageUrl = dto.BannerImageUrl;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE /api/events/{id} - Organiser only, must own the event
        [HttpDelete("{id}")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Event deleted successfully." });
        }
    }
}