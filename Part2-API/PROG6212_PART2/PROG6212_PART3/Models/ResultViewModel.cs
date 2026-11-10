
namespace PROG6212_PART3.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int? Position { get; set; }
        public int CapturedByOrganiserId { get; set; }
    }
}
