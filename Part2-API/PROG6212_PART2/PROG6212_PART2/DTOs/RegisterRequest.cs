
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression(
            "^(Organiser|Participant)$",
            ErrorMessage = "Role must be Organiser or Participant.")]
        public string Role { get; set; } = string.Empty;
    }
}

