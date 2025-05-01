using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;
using System.Collections.Generic;

namespace DataAccess.Abstract
{
    public interface IUserFollowDal : IEntityRepository<UserFollow>
    {
        List<UserFollowDto> GetFollowerDetails(int userId);
        List<UserFollowDto> GetFollowingDetails(int userId);
        bool IsFollowing(int followerId, int followedId);
    }
} 