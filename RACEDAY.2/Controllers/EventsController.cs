using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all events. Accessible to both Organisers and Participants.
        /// </summary>
        /// <response code="200">Returns list of events.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .ToListAsync();
            return Ok(events);
        }

        /// <summary>
        /// Gets a specific event by ID. Accessible to both roles.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <response code="200">Returns the event.</response>
        /// <response code="404">Event not found.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            return Ok(ev);
        }

        /// <summary>
        /// Creates a new event. Only Organisers can do this.
        /// </summary>
        /// <param name="ev">Event details: Name, Description, Date, Location, Distance, EventType.</param>
        /// <response code="201">Event created.</response>
        /// <response code="401">Not logged in.</response>
        /// <response code="403">Not an Organiser.</response>
        [HttpPost]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] Event ev)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            ev.OrganiserId = userId.Value;
            ev.Id = 0; // ensure EF generates new ID

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ev);
        }

        /// <summary>
        /// Updates an event. Only the Organiser who created it can update.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <param name="ev">Updated event details.</param>
        /// <response code="204">Updated successfully.</response>
        /// <response code="403">Not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpPut("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Update(int id, [FromBody] Event ev)
        {
            if (id != ev.Id) return BadRequest(new { message = "ID mismatch." });

            var userId = HttpContext.Session.GetInt32("UserId");
            var existing = await _context.Events.FindAsync(id);

            if (existing == null) return NotFound(new { message = "Event not found." });
            if (existing.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only update your own events." });

            existing.Name = ev.Name;
            existing.Description = ev.Description;
            existing.Date = ev.Date;
            existing.Location = ev.Location;
            existing.Distance = ev.Distance;
            existing.EventType = ev.EventType;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Deletes an event. Only the Organiser who created it can delete.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="403">Not your event.</response>
        /// <response code="404">Event not found.</response>
        [HttpDelete("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var ev = await _context.Events.FindAsync(id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (ev.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only delete your own events." });

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}