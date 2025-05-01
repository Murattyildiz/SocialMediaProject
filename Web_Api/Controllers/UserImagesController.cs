using Business.Abstract;
using Core.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserImagesController : ControllerBase
    {
        private readonly IUserImageService _userImageService;

        public UserImagesController(IUserImageService userImageService)
        {
            _userImageService = userImageService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _userImageService.GetAll();
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("getallbyuserid")]
        public IActionResult GetAllByUserId(int userId)
        {
            var result = _userImageService.GetUserImages(userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("add")]
        [Consumes("multipart/form-data")]
        public IActionResult Add([FromForm] int userId, [FromForm] IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "Dosya seçilmedi." });
            }

            var result = _userImageService.Add(imageFile, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("update")]
        [Consumes("multipart/form-data")]
        public IActionResult Update([FromForm] int Id, [FromForm] int UserId, [FromForm] IFormFile ImageFile, [FromForm] string ImagePath)
        {
            if (ImageFile == null || ImageFile.Length == 0)
            {
                return BadRequest(new { Success = false, Message = "Dosya seçilmedi." });
            }

            var userImage = new UserImage
            {
                Id = Id,
                UserId = UserId,
                ImagePath = ImagePath
            };

            var result = _userImageService.Update(userImage, ImageFile);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete("delete")]
        public IActionResult Delete(UserImage userImage)
        {
            var result = _userImageService.Delete(userImage);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
