using System.Collections.Generic;

namespace SosyalMedya_Web.Models
{
    public class ProfileViewModel
    {
        public List<ArticleDetail> Articles { get; set; } = new List<ArticleDetail>();
        public List<CodeShareViewModel> CodeShares { get; set; } = new List<CodeShareViewModel>();
    }
} 