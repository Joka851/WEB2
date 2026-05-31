namespace TravelService.DTOs
{
    public class ChecklistItemDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class CreateChecklistItemDto
    {
        public string Name { get; set; } = string.Empty;
    }
}