namespace RaceDayAPI.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Age"; // "Age" or "Distance"

        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}