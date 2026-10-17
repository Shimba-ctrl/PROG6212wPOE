
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Distance { get; set; }

        public int? MaxParticipants { get; set; }

        // The event this category belongs to
        public Event? Event { get; set; }

        // Participants enrolled in this category
        public ICollection<Enrolment> Enrolments { get; set; }
            = new List<Enrolment>();
    }
}

