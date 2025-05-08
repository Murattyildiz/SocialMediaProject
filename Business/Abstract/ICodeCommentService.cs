using Core.Utilities.Result.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ICodeCommentService
    {
        IDataResult<List<CodeCommentDto>> GetAllByCodeShareId(int codeShareId);
        IResult Add(CodeComment codeComment);
        IResult Update(CodeComment codeComment);
        IResult Delete(CodeComment codeComment);
    }
} 