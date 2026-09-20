using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    public class ResultsController : BaseApiController
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // POST /api/enrolments/{enrolmentId}/result - Organiser only, must own the event
        [HttpPost("api/enrolments/{enrolmentId}/result")]
        [SessionAuthorize("Organiser")]
        public async Task<IActionResult> Create(int enrolmentId, CreateResultDto dto)
        {
            var enrolment = await _context.Enrolments
                .Include(en => en.Event)
                .Include(en => en.Result)
                .FirstOrDefaultAsync(en => en.EnrolmentId == enrolmentId);

            if (enrolment == null) return NotFound(new { message = "Enrolment not found." });
            if (enrolment.Event!.OrganiserId != CurrentUserId) return Forbid();
            if (enrolment.Result != null) return Conflict(new { message = "A result has already been captured for this enrolment." });

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                FinishPosition = dto.FinishPosition,
                CapturedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            enrolment.Status = "Confirmed";
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMine), null, new { result.ResultId });
        }

        // GET /api/results/me - Participant only
        [HttpGet("api/results/me")]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> GetMine()
        {
            var results = await _context.Results
                .Include(r => r.Enrolment)!.ThenInclude(en => en!.Event)
                .Include(r => r.Enrolment)!.ThenInclude(en => en!.Category)
                .Where(r => r.Enrolment!.ParticipantId == CurrentUserId)
                .Select(r => new
                {
                    EventName = r.Enrolment!.Event!.Name,
                    r.Enrolment.Event.EventDate,
                    CategoryName = r.Enrolment.Category!.CategoryName,
                    r.FinishTime,
                    r.FinishPosition
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}