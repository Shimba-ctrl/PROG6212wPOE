
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.Models
{
    public class Route
    {
        [Key]
        public int RouteId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [MaxLength(150)]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string EndLocation { get; set; } = string.Empty;

        [Required]
        public decimal DistanceKm { get; set; }

        public decimal? ElevationGain { get; set; }

        [MaxLength(1000)]
        public string? RouteDescription { get; set; }

        // The event this route belongs to
        public Event? Event { get; set; }
    }
}
