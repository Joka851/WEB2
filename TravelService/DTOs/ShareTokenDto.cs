using System.ComponentModel.DataAnnotations;

namespace TravelService.DTOs
{
    public class ShareTokenDto
    {
        public int Id { get; set; }
        public int TravelPlanId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string AccessType { get; set; } = string.Empty; // VIEW or EDIT
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateShareTokenDto
    {
        [Required(ErrorMessage = "Tip pristupa je obavezan")]
        [RegularExpression("^(VIEW|EDIT)$", ErrorMessage = "Tip pristupa mora biti VIEW ili EDIT")]
        public string AccessType { get; set; } = "VIEW";

        [Required(ErrorMessage = "Vreme isteka je obavezno")]
        public DateTime ExpiresAt { get; set; }
    }

    public class ValidateShareTokenDto
    {
        [Required(ErrorMessage = "Token je obavezan")]
        public string Token { get; set; } = string.Empty;
    }

    public class ShareTokenResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string QrCode { get; set; } = string.Empty; // Base64 encoded QR code
        public DateTime ExpiresAt { get; set; }
    }
}
