namespace TravelService.DTOs
{
    public class ShareTokenDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class CreateShareTokenDto
    {
        public string AccessType { get; set; } = "View";
    }
}