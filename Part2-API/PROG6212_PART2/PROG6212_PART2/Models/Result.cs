
using System.ComponentModel.DataAnnotations;

namespace PROG6212_PART2.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        [Required]
        public int EnrolmentId { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        public int? Position { get; set; }

        [Required]
        public int CapturedByOrganiserId { get; set; }

        // The enrolment this result belongs to
        public Enrolment? Enrolment { get; set; }

        // The organiser who captured the result
        public User? CapturedByOrganiser { get; set; }
    }
}

