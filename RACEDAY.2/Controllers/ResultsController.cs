using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the results for the current logged-in Participant.
        /// Participants can only see their own results.
        /// </summary>
        [HttpGet("my")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Category)
                .Where(r => r.Enrolment!.ParticipantId == userId)
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Gets all results for the current Organiser's events.
        /// </summary>
        [HttpGet("for-my-events")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> GetForMyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                .Where(r => r.Enrolment!.Event!.OrganiserId == userId)
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>
        /// Captures a result for an enrolment.
        /// Only the Organiser who owns the event can do this.
        /// </summary>
        /// <param name="result">The EnrolmentId, FinishTime, and Position.</param>
        [HttpPost]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] Result result)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Find the enrolment
            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.Id == result.EnrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            // Make sure this organiser owns the event
            if (enrolment.Event!.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only add results for your own events." });

            // Check no result already exists for this enrolment
            var existing = await _context.Results
                .AnyAsync(r => r.EnrolmentId == result.EnrolmentId);
            if (existing)
                return BadRequest(new { message = "A result for this enrolment already exists." });

            result.Id = 0;
            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return StatusCode(201, result);
        }

        /// <summary>
        /// Updates an existing result. Only the owning Organiser can update.
        /// </summary>
        /// <param name="id">The result ID.</param>
        /// <param name="updated">The new FinishTime and Position.</param>
        [HttpPut("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] Result updated)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(en => en!.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });
            if (result.Enrolment!.Event!.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only update results for your own events." });

            result.FinishTime = updated.FinishTime;
            result.Position = updated.Position;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}