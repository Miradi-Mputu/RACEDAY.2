using System.ComponentModel.DataAnnotations;

namespace RaceDayAPI.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}