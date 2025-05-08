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
    public interface ICodeShareService
    {
        IDataResult<List<CodeShareDto>> GetAll();
        IDataResult<List<CodeShareDto>> GetAllByUserId(int userId);
        IDataResult<CodeShareDto> GetById(int codeShareId);
        IResult Add(CodeShare codeShare);
        IResult Update(CodeShare codeShare);
        IResult Delete(CodeShare codeShare);
        IResult UpdateViewCount(int codeShareId);
        IResult UpdateDownloadCount(int codeShareId);
        IDataResult<string> AnalyzeCodePurpose(int codeShareId);
    }
} 