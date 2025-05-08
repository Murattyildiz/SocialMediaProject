using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete
{
    public class CodeShare : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CodeContent { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }
        public DateTime SharingDate { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public string FileName { get; set; }
    }
} 