using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constants;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.Autofac.Caching;
using Core.Aspects.Autofac.Logging;
using Core.Aspects.Autofac.Validation;
using Core.CrossCuttingconcerns.Logging.Log4Net.Loggers;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.Entityframework;
using Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CommentManager : ICommentService
    {
        private readonly ICommentDal _commentDal;

        public CommentManager(ICommentDal commentDal) => _commentDal = commentDal;

        //[LogAspect(typeof(FileLogger))]
        //[SecuredOperation("admin,user")]
        [ValidationAspect(typeof(CommentValidator))]
        [CacheRemoveAspect("ICommentService.GetAll")]
        [CacheRemoveAspect("ICommentService.GetbyArticleId")]
        [CacheRemoveAspect("IArticleService.GetArticleDetails")]
        [CacheRemoveAspect("IArticleService.GetArticleDetailsById")]
        public IResult Add(Comment entity)
        {
            try
            {
                _commentDal.Add(entity);
                return new SuccessResult(Messages.Comment_Add);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Yorum eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        [LogAspect(typeof(FileLogger))]
        //[SecuredOperation("admin,user")]
        [CacheRemoveAspect("ICommentService.GetAll")]
        [CacheRemoveAspect("ICommentService.GetbyArticleId")]
        [CacheRemoveAspect("IArticleService.GetArticleDetails")]
        [CacheRemoveAspect("IArticleService.GetArticleDetailsById")]
        public IResult AllCommentDeleteByUserId(int id)
        {
            var deleteComments = _commentDal.GetAll(x => x.UserId == id);
            if (deleteComments != null)
            {
                foreach (var item in deleteComments)
                {
                    var comment = new Comment
                    {
                        UserId = item.UserId,
                        ArticleId = item.ArticleId,
                        CommentDate = item.CommentDate,
                        CommentText = item.CommentText,
                        Id = item.Id,
                        Status = item.Status,
                    };
                    _commentDal.Delete(comment);

                }
                return new SuccessResult(Messages.Comment_Delete);
            }
            return new SuccessResult(Messages.Comment_Delete);
        }

        [LogAspect(typeof(FileLogger))]
        //[SecuredOperation("admin,user")]
        [CacheRemoveAspect("ICommentService.GetAll")]
        [CacheRemoveAspect("ICommentService.GetbyArticleId")]
        [CacheRemoveAspect("IArticleService.GetArticleDetails")]
        [CacheRemoveAspect("IArticleService.GetArticleDetailsById")]
        public IResult Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ErrorResult("Geçersiz yorum ID'si.");
                }

                var deleteComment = _commentDal.Get(x => x.Id == id);
                if (deleteComment == null)
                {
                    return new ErrorResult(Messages.CommentNotFound);
                }

                _commentDal.Delete(deleteComment);
                return new SuccessResult(Messages.Comment_Delete);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Yorum silinirken bir hata oluştu: {ex.Message}");
            }
        }
        [CacheAspect(100)]
        public IDataResult<List<Comment>> FalseComment()
        {
            return new SuccessDataResult<List<Comment>>(_commentDal.GetAll(x=>x.Status==false), Messages.FalseComment);
        }
        [CacheAspect(duration: 100)]
        public IDataResult<List<Comment>> GetAll()
        {
            return new SuccessDataResult<List<Comment>>(_commentDal.GetAll(), Messages.Comments_Listed);
        }

        [CacheAspect(duration: 100)]
        public IDataResult<Comment> GetEntityById(int id)
        {
            return new SuccessDataResult<Comment>(_commentDal.Get(x => x.Id == id), Messages.Comment_Listed);
        }

        [CacheAspect(duration: 100)]
        public IDataResult<List<Comment>> GetbyArticleId(int id)
        {
            return new SuccessDataResult<List<Comment>>(_commentDal.GetAll(x => x.ArticleId == id), Messages.Comments_List);
        }

        public IDataResult<List<Comment>> NotSeen(int id)
        {
            throw new NotImplementedException();
        }
        [CacheAspect(100)]
        public IDataResult<List<Comment>> TrueComment()
        {
            return new SuccessDataResult<List<Comment>>(_commentDal.GetAll(x => x.Status == true), Messages.FalseComment);
        }

        [LogAspect(typeof(FileLogger))]
        //[SecuredOperation("admin,user")]
        [ValidationAspect(typeof(CommentValidator))]
        [CacheRemoveAspect("ICommentService.GetAll")]
        [CacheRemoveAspect("ICommentService.GetbyArticleId")]
        [CacheRemoveAspect("IArticleService.GetArticleDetails")]
        [CacheRemoveAspect("IArticleService.GetArticleDetailsById")]
        public IResult Update(Comment entity)
        {
            var updatedComment = _commentDal.Get(x => x.Id == entity.Id);
            if (updatedComment != null)
            {
                _commentDal.Update(entity);
                return new SuccessResult(Messages.Comment_Update);
            }
            return new ErrorResult(Messages.CommentNotFound);
        }
    }
}
