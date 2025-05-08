using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using DataAccess.Concrete.Context;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete.EntityFramework
{
    public class EfCodeShareDal : EfEntityRepositoryBase<CodeShare, SocialMediaContext>, ICodeShareDal
    {
        public List<CodeShareDto> GetCodeSharesWithDetails()
        {
            using (var context = new SocialMediaContext())
            {
                var result = from c in context.CodeShares
                             join u in context.Users on c.UserId equals u.Id
                             select new CodeShareDto
                             {
                                 Id = c.Id,
                                 UserId = c.UserId,
                                 UserFirstName = u.FirstName,
                                 UserLastName = u.LastName,
                                 UserImage = (from ui in context.UserImages where ui.UserId == u.Id select ui.ImagePath).FirstOrDefault(),
                                 Title = c.Title,
                                 Description = c.Description,
                                 CodeContent = c.CodeContent,
                                 Language = c.Language,
                                 Tags = c.Tags,
                                 SharingDate = c.SharingDate,
                                 ViewCount = c.ViewCount,
                                 DownloadCount = c.DownloadCount,
                                 FileName = c.FileName,
                                 Comments = (from cc in context.CodeComments
                                            join cu in context.Users on cc.UserId equals cu.Id
                                            where cc.CodeShareId == c.Id
                                            select new CodeCommentDto
                                            {
                                                Id = cc.Id,
                                                CodeShareId = cc.CodeShareId,
                                                UserId = cc.UserId,
                                                UserName = cu.FirstName + " " + cu.LastName,
                                                UserImage = (from cui in context.UserImages where cui.UserId == cu.Id select cui.ImagePath).FirstOrDefault(),
                                                CommentText = cc.CommentText,
                                                CommentDate = cc.CommentDate
                                            }).ToList()
                             };
                return result.OrderByDescending(x => x.SharingDate).ToList();
            }
        }

        public List<CodeShareDto> GetCodeSharesWithDetailsByUserId(int userId)
        {
            using (var context = new SocialMediaContext())
            {
                var result = from c in context.CodeShares
                             join u in context.Users on c.UserId equals u.Id
                             where c.UserId == userId
                             select new CodeShareDto
                             {
                                 Id = c.Id,
                                 UserId = c.UserId,
                                 UserFirstName = u.FirstName,
                                 UserLastName = u.LastName,
                                 UserImage = (from ui in context.UserImages where ui.UserId == u.Id select ui.ImagePath).FirstOrDefault(),
                                 Title = c.Title,
                                 Description = c.Description,
                                 CodeContent = c.CodeContent,
                                 Language = c.Language,
                                 Tags = c.Tags,
                                 SharingDate = c.SharingDate,
                                 ViewCount = c.ViewCount,
                                 DownloadCount = c.DownloadCount,
                                 FileName = c.FileName,
                                 Comments = (from cc in context.CodeComments
                                            join cu in context.Users on cc.UserId equals cu.Id
                                            where cc.CodeShareId == c.Id
                                            select new CodeCommentDto
                                            {
                                                Id = cc.Id,
                                                CodeShareId = cc.CodeShareId,
                                                UserId = cc.UserId,
                                                UserName = cu.FirstName + " " + cu.LastName,
                                                UserImage = (from cui in context.UserImages where cui.UserId == cu.Id select cui.ImagePath).FirstOrDefault(),
                                                CommentText = cc.CommentText,
                                                CommentDate = cc.CommentDate
                                            }).ToList()
                             };
                return result.OrderByDescending(x => x.SharingDate).ToList();
            }
        }

        public CodeShareDto GetCodeShareWithDetailsById(int codeShareId)
        {
            using (var context = new SocialMediaContext())
            {
                var result = from c in context.CodeShares
                             join u in context.Users on c.UserId equals u.Id
                             where c.Id == codeShareId
                             select new CodeShareDto
                             {
                                 Id = c.Id,
                                 UserId = c.UserId,
                                 UserFirstName = u.FirstName,
                                 UserLastName = u.LastName,
                                 UserImage = (from ui in context.UserImages where ui.UserId == u.Id select ui.ImagePath).FirstOrDefault(),
                                 Title = c.Title,
                                 Description = c.Description,
                                 CodeContent = c.CodeContent,
                                 Language = c.Language,
                                 Tags = c.Tags,
                                 SharingDate = c.SharingDate,
                                 ViewCount = c.ViewCount,
                                 DownloadCount = c.DownloadCount,
                                 FileName = c.FileName,
                                 Comments = (from cc in context.CodeComments
                                            join cu in context.Users on cc.UserId equals cu.Id
                                            where cc.CodeShareId == c.Id
                                            select new CodeCommentDto
                                            {
                                                Id = cc.Id,
                                                CodeShareId = cc.CodeShareId,
                                                UserId = cc.UserId,
                                                UserName = cu.FirstName + " " + cu.LastName,
                                                UserImage = (from cui in context.UserImages where cui.UserId == cu.Id select cui.ImagePath).FirstOrDefault(),
                                                CommentText = cc.CommentText,
                                                CommentDate = cc.CommentDate
                                            }).ToList()
                             };
                return result.FirstOrDefault();
            }
        }

        public void UpdateViewCount(int codeShareId)
        {
            using (var context = new SocialMediaContext())
            {
                var codeShare = context.CodeShares.FirstOrDefault(c => c.Id == codeShareId);
                if (codeShare != null)
                {
                    codeShare.ViewCount += 1;
                    context.SaveChanges();
                }
            }
        }

        public void UpdateDownloadCount(int codeShareId)
        {
            using (var context = new SocialMediaContext())
            {
                var codeShare = context.CodeShares.FirstOrDefault(c => c.Id == codeShareId);
                if (codeShare != null)
                {
                    codeShare.DownloadCount += 1;
                    context.SaveChanges();
                }
            }
        }
    }
} 