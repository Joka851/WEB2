namespace TravelService.Models
{
    public class ShareToken
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string AccessType { get; set; } = "View";
        public DateTime ExpiresAt { get; set; }
        public TravelPlan TravelPlan { get; set; } = null!;
    }
}