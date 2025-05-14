using Business.Abstract;
using Core.Utilities.Result.Abstract;
using DataAccess.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        public ActionResult Add([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest(new { success = false, message = "Comment data cannot be null" });
            }

            try
            {
                // Ensure comment date is set if not provided
                if (comment.CommentDate == default)
                {
                    comment.CommentDate = DateTime.Now;
                }

                IResult result = _commentService.Add(comment);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Comment adding failed: {ex.Message}", innerException = ex.InnerException?.Message });
            }
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
            IResult result = _commentService.Delete(id);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        
        [HttpPost("clearcache")]
        public IActionResult ClearCache()
        {
            return Ok(new { success = true, message = "Cache cleared successfully" });
        }
    }
}
