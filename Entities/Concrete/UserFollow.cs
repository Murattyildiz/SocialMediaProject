using Core.Entities;
using System;

namespace Entities.Concrete
{
    public class UserFollow : IEntity
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }  // The user who is following
        public int FollowedId { get; set; }  // The user being followed
        public DateTime FollowDate { get; set; }
    }
} 