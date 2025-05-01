namespace SosyalMedya_Web.Models
{
    public class UserFollowDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ImagePath { get; set; }
        public bool IsFollowing { get; set; }
    }
} 