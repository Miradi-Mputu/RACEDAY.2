using Microsoft.AspNetCore.Mvc;
using RaceDayAPI.Data;
using RaceDayAPI.DTOs;

namespace RaceDayAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ProfileController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the profile of the currently logged-in user.
        /// Works for both Organisers and Participants.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            // Check that someone is logged in
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Return profile info, but never the password hash
            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }

        /// <summary>
        /// Updates the current user's profile (FullName and/or Email).
        /// Works for both Organisers and Participants.
        /// </summary>
        /// <param name="dto">The new profile details.</param>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized(new { message = "Not logged in." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // If they are changing email, check it's not already used by someone else
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var emailTaken = _context.Users.Any(u => u.Email == dto.Email && u.Id != user.Id);
                if (emailTaken)
                    return BadRequest(new { message = "That email is already in use." });

                user.Email = dto.Email;
            }

            // Update the name if a new one was provided
            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            await _context.SaveChangesAsync();

            // Update the session so the new name is used in future requests
            HttpContext.Session.SetString("UserName", user.FullName);

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role
            });
        }
    }
}