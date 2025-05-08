using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SosyalMedya_Web.Models
{
    public class CodeShareViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CodeContent { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }
        public DateTime SharingDate { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public string FileName { get; set; }
        public List<CodeCommentViewModel> Comments { get; set; }
    }

    public class CreateCodeShareViewModel
    {
        [Required(ErrorMessage = "Başlık gereklidir.")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Açıklama gereklidir.")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Kod içeriği gereklidir.")]
        public string CodeContent { get; set; }
        
        [Required(ErrorMessage = "Programlama dili seçimi gereklidir.")]
        public string Language { get; set; }
        
        public string Tags { get; set; }
        public string FileName { get; set; }
    }

    public class CodeCommentViewModel
    {
        public int Id { get; set; }
        public int CodeShareId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
    }
} 