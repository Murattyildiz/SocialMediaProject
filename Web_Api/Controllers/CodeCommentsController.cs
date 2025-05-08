using Business.Abstract;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeCommentsController : ControllerBase
    {
        private readonly ICodeCommentService _codeCommentService;

        public CodeCommentsController(ICodeCommentService codeCommentService)
        {
            _codeCommentService = codeCommentService;
        }

        [HttpGet("getbycodeshareid")]
        public IActionResult GetByCodeShareId(int codeShareId)
        {
            var result = _codeCommentService.GetAllByCodeShareId(codeShareId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public IActionResult Add(CodeComment codeComment)
        {
            var result = _codeCommentService.Add(codeComment);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("update")]
        public IActionResult Update(CodeComment codeComment)
        {
            var result = _codeCommentService.Update(codeComment);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(CodeComment codeComment)
        {
            var result = _codeCommentService.Delete(codeComment);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
} 