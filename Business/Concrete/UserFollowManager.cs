using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;

namespace Business.Concrete
{
    public class UserFollowManager : IUserFollowService
    {
        private readonly IUserFollowDal _userFollowDal;

        public UserFollowManager(IUserFollowDal userFollowDal)
        {
            _userFollowDal = userFollowDal;
        }

        public IResult Follow(UserFollow userFollow)
        {
            try
            {
                if (_userFollowDal.IsFollowing(userFollow.FollowerId, userFollow.FollowedId))
                {
                    return new ErrorResult("Zaten bu kullanıcıyı takip ediyorsunuz.");
                }

                userFollow.FollowDate = DateTime.Now;
                _userFollowDal.Add(userFollow);
                return new SuccessResult("Kullanıcı başarıyla takip edildi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Takip işlemi sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<UserFollowDto>> GetFollowers(int userId)
        {
            try
            {
                var followers = _userFollowDal.GetFollowerDetails(userId);
                return new SuccessDataResult<List<UserFollowDto>>(followers);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<UserFollowDto>>(null, $"Takipçiler getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<List<UserFollowDto>> GetFollowing(int userId)
        {
            try
            {
                var following = _userFollowDal.GetFollowingDetails(userId);
                return new SuccessDataResult<List<UserFollowDto>>(following);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<UserFollowDto>>(null, $"Takip edilenler getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        public IResult Unfollow(int followerId, int followedId)
        {
            try
            {
                var follow = _userFollowDal.Get(f => f.FollowerId == followerId && f.FollowedId == followedId);
                if (follow == null)
                {
                    return new ErrorResult("Takip ilişkisi bulunamadı.");
                }

                _userFollowDal.Delete(follow);
                return new SuccessResult("Takipten çıkma işlemi başarılı.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Takipten çıkma işlemi sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<bool> IsFollowing(int followerId, int followedId)
        {
            try
            {
                var isFollowing = _userFollowDal.IsFollowing(followerId, followedId);
                return new SuccessDataResult<bool>(isFollowing);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<bool>(false, $"Takip durumu kontrol edilirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<int> GetFollowerCount(int userId)
        {
            try
            {
                var count = _userFollowDal.GetAll(f => f.FollowedId == userId).Count();
                return new SuccessDataResult<int>(count);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<int>(0, $"Takipçi sayısı alınırken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<int> GetFollowingCount(int userId)
        {
            try
            {
                var count = _userFollowDal.GetAll(f => f.FollowerId == userId).Count();
                return new SuccessDataResult<int>(count);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<int>(0, $"Takip edilen sayısı alınırken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 