using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.Context;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfCodeCommentDal : EfEntityRepositoryBase<CodeComment, SocialMediaContext>, ICodeCommentDal
    {
        public List<CodeCommentDto> GetCodeCommentsByCodeShareId(int codeShareId)
        {
            using (var context = new SocialMediaContext())
            {
                var result = from cc in context.CodeComments
                            join u in context.Users on cc.UserId equals u.Id
                            where cc.CodeShareId == codeShareId
                            select new CodeCommentDto
                            {
                                Id = cc.Id,
                                CodeShareId = cc.CodeShareId,
                                UserId = cc.UserId,
                                UserName = u.FirstName + " " + u.LastName,
                                UserImage = (from ui in context.UserImages where ui.UserId == u.Id select ui.ImagePath).FirstOrDefault(),
                                CommentText = cc.CommentText,
                                CommentDate = cc.CommentDate
                            };
                return result.OrderByDescending(x => x.CommentDate).ToList();
            }
        }
    }
} 