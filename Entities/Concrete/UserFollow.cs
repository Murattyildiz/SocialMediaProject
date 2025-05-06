using Core.Entities;
using System;

namespace Entities.Concrete
{
    public class UserFollow : IEntity
    {
        public int Id { get; set; }
        public int FollowerId { get; set; }  
        public int FollowedId { get; set; }
        public DateTime FollowDate { get; set; }
    }
} 