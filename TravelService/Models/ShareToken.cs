using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace TravelService.Models
{
    public class ShareToken
    {
        public const string ACCESS_TYPE_VIEW = "VIEW";
        public const string ACCESS_TYPE_EDIT = "EDIT";

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID putnog plana je obavezan")]
        public int TravelPlanId { get; set; }

        [Required(ErrorMessage = "Token je obavezan")]
        [StringLength(500, MinimumLength = 32,
            ErrorMessage = "Token mora sadržati između 32 i 500 karaktera")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tip pristupa je obavezan")]
        [StringLength(50, ErrorMessage = "Tip pristupa može biti maksimalno 50 karaktera")]
        public string AccessType { get; set; } = ACCESS_TYPE_VIEW;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Vreme isteka je obavezno")]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [Required]
        public TravelPlan TravelPlan { get; set; } = null!;

        // Check if token is still valid
        public bool IsValid()
        {
            return IsActive &&
                   !IsDeleted &&
                   DateTime.UtcNow <= ExpiresAt &&
                   !string.IsNullOrWhiteSpace(Token) &&
                   (AccessType == ACCESS_TYPE_VIEW || AccessType == ACCESS_TYPE_EDIT);
        }

        // Check if token has expired
        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }

        // Check if access type is valid
        public static bool IsValidAccessType(string accessType)
        {
            return accessType == ACCESS_TYPE_VIEW || accessType == ACCESS_TYPE_EDIT;
        }

        // Generate a secure token
        public static string GenerateToken()
        {
            byte[] tokenData = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenData);
            }
            return Convert.ToBase64String(tokenData);
        }
    }
}
