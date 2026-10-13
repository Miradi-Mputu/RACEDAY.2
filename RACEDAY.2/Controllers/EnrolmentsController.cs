using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all enrolments for the current logged-in Participant.
        /// </summary>
        [HttpGet("my")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var enrolments = await _context.Enrolments
                .Where(e => e.ParticipantId == userId)
                .Include(e => e.Event)
                .Include(e => e.Category)
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Gets all enrolments for the current Organiser's events.
        /// </summary>
        [HttpGet("for-my-events")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> GetForMyEvents()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Participant)
                .Where(e => e.Event!.OrganiserId == userId)
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>
        /// Enrols the logged-in Participant into an event with a chosen category.
        /// </summary>
        /// <param name="enrolment">The EventId and CategoryId to enrol in.</param>
        [HttpPost]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> Create([FromBody] Enrolment enrolment)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Check the event exists
            var eventItem = await _context.Events.FindAsync(enrolment.EventId);
            if (eventItem == null)
                return NotFound(new { message = "Event not found." });

            // Check the category exists and belongs to that event
            var category = await _context.Categories.FindAsync(enrolment.CategoryId);
            if (category == null || category.EventId != enrolment.EventId)
                return BadRequest(new { message = "Category does not belong to the selected event." });

            // Check the Participant hasn't already enrolled
            var alreadyEnrolled = await _context.Enrolments
                .AnyAsync(e => e.ParticipantId == userId && e.EventId == enrolment.EventId);
            if (alreadyEnrolled)
                return BadRequest(new { message = "You are already enrolled in this event." });

            // Force the ParticipantId to be the logged-in user
            enrolment.Id = 0;
            enrolment.ParticipantId = userId.Value;
            enrolment.EnrolledAt = DateTime.UtcNow;

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return StatusCode(201, enrolment);
        }

        /// <summary>
        /// Deletes (cancels) an enrolment. Only the Participant who created it can delete.
        /// </summary>
        /// <param name="id">The enrolment ID to delete.</param>
        [HttpDelete("{id}")]
        [RoleAuthorize("Participant")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var enrolment = await _context.Enrolments.FindAsync(id);
            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });
            if (enrolment.ParticipantId != userId)
                return StatusCode(403, new { message = "You can only delete your own enrolments." });

            _context.Enrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}