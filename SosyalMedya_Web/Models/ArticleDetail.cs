using System.Globalization;

namespace SosyalMedya_Web.Models
{
    public class ArticleDetail
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string TopicTitle { get; set; }
        public string Content { get; set; }

        // JSON'dan gelen tarih string olarak alınır
        public string SharingDate { get; set; }

        // Uygulama içinde tarih olarak kullanmak için
        public DateTime SharingDateParsed
        {
            get
            {
                DateTime.TryParseExact(SharingDate, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate);
                return parsedDate;
            }
        }

        public List<CommentDetail> CommentDetails { get; set; }
    }
}
