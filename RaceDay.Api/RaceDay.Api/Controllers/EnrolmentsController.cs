using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class EnrolmentsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST /api/events/{eventId}/enrolments - Participant only
        [HttpPost("api/events/{eventId}/enrolments")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(int eventId, CreateEnrolmentDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });

            var categoryValid = await _context.Categories
                .AnyAsync(c => c.CategoryId == dto.CategoryId && c.EventId == eventId);
            if (!categoryValid)
                return BadRequest(new { message = "Invalid category for this event." });

            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(en => en.ParticipantId == CurrentUserId && en.EventId == eventId);
            if (alreadyEnrolled)
                return Conflict(new { message = "You are already enrolled in this event." });

            var enrolment = new Enrolment
            {
                ParticipantId = CurrentUserId!.Value,
                EventId = eventId,
                CategoryId = dto.CategoryId,
                Status = "Pending",
                EnrolledAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMine), null, new { enrolment.EnrolmentId, enrolment.Status });
        }

        // GET /api/enrolments/me - Participant only
        [HttpGet("api/enrolments/me")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMine()
        {
            var enrolments = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Category)
                .Where(en => en.ParticipantId == CurrentUserId)
                .Select(en => new
                {
                    en.EnrolmentId,
                    EventName = en.Event!.Name,
                    en.Event.EventDate,
                    CategoryName = en.Category!.CategoryName,
                    en.Status,
                    en.EnrolledAt
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET /api/events/{eventId}/enrolments - Organiser only, must own the event
        [HttpGet("api/events/{eventId}/enrolments")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> GetForEvent(int eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != CurrentUserId) return Forbid();

            var enrolments = await _context.Enrolments
                .Include(en => en.Participant)
                .Include(en => en.Category)
                .Where(en => en.EventId == eventId)
                .Select(en => new
                {
                    en.EnrolmentId,
                    ParticipantName = en.Participant!.FullName,
                    CategoryName = en.Category!.CategoryName,
                    en.Status,
                    en.EnrolledAt
                })
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}