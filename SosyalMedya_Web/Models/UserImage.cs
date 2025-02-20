namespace SosyalMedya_Web.Models
{
    public class UserImage
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public IFormFile ImagePath { get; set; }

        public DateTime Date { get; set; }

    }
}
