using Core.Service;
using Core.Utilities.Result.Abstract;
using Entities.Concrete;
using Entities.DTOs;

namespace Business.Abstract
{
    public interface IUserFollowService
    {
        IDataResult<List<UserFollowDto>> GetFollowers(int userId);
        IDataResult<List<UserFollowDto>> GetFollowing(int userId);
        IResult Follow(UserFollow userFollow);
        IResult Unfollow(int followerId, int followedId);
        IDataResult<bool> IsFollowing(int followerId, int followedId);
        IDataResult<int> GetFollowerCount(int userId);
        IDataResult<int> GetFollowingCount(int userId);
    }
} 