
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int OrganiserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        [Required]
        [MaxLength(20)]
        [RegularExpression("^(Run|Walk|Cycle)$")]
        public string EventType { get; set; } = "Run";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User? Organiser { get; set; }

        public ICollection<Category> Categories { get; set; }
            = new List<Category>();

        public Route? Route { get; set; }
    }
}
