using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Mvc;
using IResult = Core.Utilities.Result.Abstract.IResult;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserFollowController : ControllerBase
    {
        private readonly IUserFollowService _userFollowService;

        public UserFollowController(IUserFollowService userFollowService)
        {
            _userFollowService = userFollowService;
        }

        [HttpGet("getfollowers")]
        public IActionResult GetFollowers(int userId)
        {
            var result = _userFollowService.GetFollowers(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getfollowing")]
        public IActionResult GetFollowing(int userId)
        {
            var result = _userFollowService.GetFollowing(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getfollowercount")]
        public IActionResult GetFollowerCount(int userId)
        {
            var result = _userFollowService.GetFollowerCount(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getfollowingcount")]
        public IActionResult GetFollowingCount(int userId)
        {
            var result = _userFollowService.GetFollowingCount(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("isfollowing")]
        public IActionResult IsFollowing(int followerId, int followedId)
        {
            var result = _userFollowService.IsFollowing(followerId, followedId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("follow")]
        public IActionResult Follow([FromBody] UserFollow userFollow)
        {
            // Kontrol et - takip eden ve takip edilen kullanıcılar aynı olmamalı
            if (userFollow.FollowerId == userFollow.FollowedId)
            {
                return BadRequest(new { Success = false, Message = "Kendinizi takip edemezsiniz." });
            }

            // Zaten takip ediliyor mu kontrol et
            var isFollowingResult = _userFollowService.IsFollowing(userFollow.FollowerId, userFollow.FollowedId);
            if (isFollowingResult.Success && isFollowingResult.Data)
            {
                return Ok(new { Success = true, Message = "Bu kullanıcıyı zaten takip ediyorsunuz." });
            }

            // Takip tarihini ekle
            userFollow.FollowDate = DateTime.Now;

            var result = _userFollowService.Follow(userFollow);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("unfollow")]
        public IActionResult Unfollow([FromBody] UserFollow userFollow)
        {
            var result = _userFollowService.Unfollow(userFollow.FollowerId, userFollow.FollowedId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
} 