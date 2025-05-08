using Business.Abstract;
using Business.Constants;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CodeCommentManager : ICodeCommentService
    {
        private readonly ICodeCommentDal _codeCommentDal;

        public CodeCommentManager(ICodeCommentDal codeCommentDal)
        {
            _codeCommentDal = codeCommentDal;
        }

        public IResult Add(CodeComment codeComment)
        {
            codeComment.CommentDate = DateTime.Now;
            _codeCommentDal.Add(codeComment);
            return new SuccessResult(Messages.CodeCommentAdded);
        }

        public IResult Delete(CodeComment codeComment)
        {
            _codeCommentDal.Delete(codeComment);
            return new SuccessResult(Messages.CodeCommentDeleted);
        }

        public IDataResult<List<CodeCommentDto>> GetAllByCodeShareId(int codeShareId)
        {
            return new SuccessDataResult<List<CodeCommentDto>>(_codeCommentDal.GetCodeCommentsByCodeShareId(codeShareId), Messages.CodeCommentsListed);
        }

        public IResult Update(CodeComment codeComment)
        {
            _codeCommentDal.Update(codeComment);
            return new SuccessResult(Messages.CodeCommentUpdated);
        }
    }
} 