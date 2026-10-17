
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentId { get; set; }

        [Required]
        public int ParticipantId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; }
            = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Confirmed";

        // The participant who enrolled
        public User? Participant { get; set; }

        // The category selected
        public Category? Category { get; set; }

        // The result recorded for this enrolment
        public Result? Result { get; set; }
    }
}

