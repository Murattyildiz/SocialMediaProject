using Core.Entities;
using System;

namespace Entities.DTOs
{
    public class UserFollowDto : IDto
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }
        public int FollowedId { get; set; }
        public string FollowerName { get; set; }
        public string FollowedName { get; set; }
        public string FollowerImage { get; set; }
        public string FollowedImage { get; set; }
        public DateTime FollowDate { get; set; }
    }
} 