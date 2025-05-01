using System;

namespace SosyalMedya_Web.Models
{
    public class UserFollow
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public int FollowedId { get; set; }
        public DateTime FollowDate { get; set; }
    }
} 