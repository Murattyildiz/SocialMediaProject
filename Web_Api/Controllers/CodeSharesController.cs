using Business.Abstract;
using Core.Entities.Concrete;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Web_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeSharesController : ControllerBase
    {
        private readonly ICodeShareService _codeShareService;

        public CodeSharesController(ICodeShareService codeShareService)
        {
            _codeShareService = codeShareService;
        }

        [HttpGet("getall")]
        public IActionResult GetAll()
        {
            var result = _codeShareService.GetAll();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyid")]
        public IActionResult GetById(int id)
        {
            var result = _codeShareService.GetById(id);
            if (result.Success)
            {
                _codeShareService.UpdateViewCount(id);
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("getbyuserid")]
        public IActionResult GetByUserId(int userId)
        {
            var result = _codeShareService.GetAllByUserId(userId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("add")]
        public IActionResult Add([FromBody] CodeShare codeShare)
        {
            // Debug info to see what we're receiving
            Console.WriteLine($"Received code share: UserId={codeShare?.UserId}, Title={codeShare?.Title}");
            Console.WriteLine($"Model State Valid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                    }
                }
                return BadRequest(new { Success = false, Message = "Model validation failed", Errors = ModelState });
            }
            
            if (codeShare == null)
            {
                return BadRequest(new { Success = false, Message = "No data received" });
            }
            
            try
            {
                // Make sure required fields are not null
                codeShare.Title = codeShare.Title ?? "";
                codeShare.Description = codeShare.Description ?? "";
                codeShare.CodeContent = codeShare.CodeContent ?? "";
                codeShare.Language = codeShare.Language ?? "";
                codeShare.Tags = codeShare.Tags ?? "";
                codeShare.FileName = codeShare.FileName ?? "";
                
                // Identity sütunu için Id değerini 0 yaparak EF Core'un otomatik Id atamasını sağlayalım
                codeShare.Id = 0;
                
                var result = _codeShareService.Add(codeShare);
                Console.WriteLine($"Add result success: {result.Success}, Message: {result.Message}");
                
                if (result.Success)
                {
                    return Ok(result);
                }
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Add: {ex.Message}");
                return BadRequest(new { Success = false, Message = $"Error: {ex.Message}" });
            }
        }

        [HttpPost("update")]
        public IActionResult Update(CodeShare codeShare)
        {
            var result = _codeShareService.Update(codeShare);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete(CodeShare codeShare)
        {
            var result = _codeShareService.Delete(codeShare);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("download/{id}")]
        public IActionResult DownloadCodeAsZip(int id)
        {
            var result = _codeShareService.GetById(id);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            var codeShare = result.Data;
            _codeShareService.UpdateDownloadCount(id);

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var fileName = !string.IsNullOrEmpty(codeShare.FileName) 
                        ? codeShare.FileName 
                        : $"{codeShare.Title}.{GetFileExtensionFromLanguage(codeShare.Language)}";
                    
                    var readmeFile = archive.CreateEntry("README.txt");
                    using (var entryStream = readmeFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write($"Title: {codeShare.Title}\n");
                        streamWriter.Write($"Description: {codeShare.Description}\n");
                        streamWriter.Write($"Language: {codeShare.Language}\n");
                        streamWriter.Write($"Tags: {codeShare.Tags}\n");
                        streamWriter.Write($"Author: {codeShare.UserFirstName} {codeShare.UserLastName}\n");
                        streamWriter.Write($"Date: {codeShare.SharingDate}\n");
                    }

                    var codeFile = archive.CreateEntry(fileName);
                    using (var entryStream = codeFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(codeShare.CodeContent);
                    }
                }

                memoryStream.Position = 0;
                return File(memoryStream.ToArray(), "application/zip", $"{codeShare.Title}.zip");
            }
        }

        [HttpGet("analyze/{id}")]
        public IActionResult AnalyzeCode(int id)
        {
            var result = _codeShareService.AnalyzeCodePurpose(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        private string GetFileExtensionFromLanguage(string language)
        {
            return language?.ToLower() switch
            {
                "c#" => "cs",
                "javascript" => "js",
                "typescript" => "ts",
                "python" => "py",
                "java" => "java",
                "html" => "html",
                "css" => "css",
                "php" => "php",
                "ruby" => "rb",
                "go" => "go",
                "rust" => "rs",
                "swift" => "swift",
                "kotlin" => "kt",
                "c++" => "cpp",
                "c" => "c",
                _ => "txt",
            };
        }
    }
} 