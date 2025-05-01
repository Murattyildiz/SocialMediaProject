namespace SosyalMedya_Web.Models
{
    public class Comment
    {
        public int ArticleId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
        public bool Status { get; set; } = true;  // Varsayılan olarak true
    }
}
