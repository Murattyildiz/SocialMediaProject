namespace SosyalMedya_Web.Models
{
    public class VerificationCodeDto
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
    }
}
