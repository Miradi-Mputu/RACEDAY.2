using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayAPI.Attributes;
using RaceDayAPI.Data;
using RaceDayAPI.Models;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all categories.
        /// Both Organisers and Participants can view this.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Gets one category by its ID.
        /// </summary>
        /// <param name="id">The category ID.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound(new { message = "Category not found." });
            return Ok(category);
        }

        /// <summary>
        /// Gets all categories for a specific event.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .ToListAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Creates a new category for an event.
        /// Only Organisers can do this, and only for their own events.
        /// </summary>
        /// <param name="category">The category details.</param>
        [HttpPost]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Make sure the event exists and belongs to this organiser
            var eventItem = await _context.Events.FindAsync(category.EventId);
            if (eventItem == null)
                return NotFound(new { message = "Event not found." });
            if (eventItem.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only add categories to your own events." });

            category.Id = 0;
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return StatusCode(201, category);
        }

        /// <summary>
        /// Deletes a category.
        /// Only Organisers can do this, and only for their own events.
        /// </summary>
        /// <param name="id">The category ID to delete.</param>
        [HttpDelete("{id}")]
        [RoleAuthorize("Organiser")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { message = "Category not found." });

            // Check that the parent event belongs to this organiser
            var eventItem = await _context.Events.FindAsync(category.EventId);
            if (eventItem == null || eventItem.OrganiserId != userId)
                return StatusCode(403, new { message = "You can only delete categories from your own events." });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}