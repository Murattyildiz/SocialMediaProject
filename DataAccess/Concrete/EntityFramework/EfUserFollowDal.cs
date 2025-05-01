using Core.DataAccess.EntityFramework;
using Core.Entities.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.Context;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete.Entityframework
{
    public class EfUserFollowDal : EfEntityRepositoryBase<UserFollow, SocialMediaContext>, IUserFollowDal
    {
        public List<UserFollowDto> GetFollowerDetails(int userId)
        {
            using (var context = new SocialMediaContext())
            {
                var result = from follow in context.UserFollows
                            join user in context.Users
                            on follow.FollowerId equals user.Id
                            where follow.FollowedId == userId
                            select new UserFollowDto
                            {
                                Id = follow.Id,
                                FollowerId = follow.FollowerId,
                                FollowedId = follow.FollowedId,
                                FollowerName = user.FirstName + " " + user.LastName,
                                FollowerImage = "default.jpg", // Varsayılan resim
                                FollowDate = follow.FollowDate
                            };
                return result.ToList();
            }
        }

        public List<UserFollowDto> GetFollowingDetails(int userId)
        {
            using (var context = new SocialMediaContext())
            {
                var result = from follow in context.UserFollows
                            join user in context.Users
                            on follow.FollowedId equals user.Id
                            where follow.FollowerId == userId
                            select new UserFollowDto
                            {
                                Id = follow.Id,
                                FollowerId = follow.FollowerId,
                                FollowedId = follow.FollowedId,
                                FollowedName = user.FirstName + " " + user.LastName,
                                FollowedImage = "default.jpg", // Varsayılan resim
                                FollowDate = follow.FollowDate
                            };
                return result.ToList();
            }
        }

        public bool IsFollowing(int followerId, int followedId)
        {
            using (var context = new SocialMediaContext())
            {
                return context.UserFollows.Any(f => f.FollowerId == followerId && f.FollowedId == followedId);
            }
        }
    }
} 