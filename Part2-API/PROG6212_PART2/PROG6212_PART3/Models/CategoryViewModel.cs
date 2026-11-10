namespace PROG6212_PART3.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int? MaxParticipants { get; set; }
    }
}