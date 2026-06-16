public class CreateShareTokenDto
{
    public string AccessType { get; set; } = "VIEW";
    public DateTime ExpiresAt { get; set; }
}