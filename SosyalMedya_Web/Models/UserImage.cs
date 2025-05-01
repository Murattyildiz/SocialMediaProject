using Microsoft.AspNetCore.Http;

namespace SosyalMedya_Web.Models
{
    public class UserImage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImagePath { get; set; }
        public IFormFile ImageFile { get; set; }
        public DateTime Date { get; set; }
    }
}
