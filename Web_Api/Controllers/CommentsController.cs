using Business.Abstract;
using Core.Utilities.Result.Abstract;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using IResult = Core.Utilities.Result.Abstract.IResult;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentsController(ICommentService commentService) => _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));

        [HttpGet("getall")]
        public ActionResult GetAll()
        {
            IDataResult<List<Comment>> result = _commentService.GetAll();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("getbyid")]
        public ActionResult GetById(int id)
        {
            IDataResult<Comment> result = _commentService.GetEntityById(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("add")]
        public ActionResult Add(Comment comment)
        {
            IResult result = _commentService.Add(comment);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("update")]
        public ActionResult Update(Comment comment)
        {
            IResult result = _commentService.Update(comment);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("delete")]
        public ActionResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { success = false, message = "Geçersiz yorum ID'si." });
                }

                var comment = _commentService.GetEntityById(id);
                if (!comment.Success || comment.Data == null)
                {
                    return BadRequest(new { success = false, message = "Yorum bulunamadı." });
                }

                var result = _commentService.Delete(id);
                if (result.Success)
                {
                    // Başarılı silme işlemi
                    Console.WriteLine($"Yorum başarıyla silindi. ID: {id}");
                    return Ok(new { success = true, message = "Yorum başarıyla silindi." });
                }
                else
                {
                    // Silme işlemi başarısız
                    Console.WriteLine($"Yorum silme başarısız. ID: {id}, Hata: {result.Message}");
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda
                Console.WriteLine($"Yorum silme hatası. ID: {id}, Hata: {ex.Message}");
                return BadRequest(new { success = false, message = $"Yorum silinirken bir hata oluştu: {ex.Message}" });
            }
        }
    }
}
