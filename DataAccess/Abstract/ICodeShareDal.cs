using Core.DataAccess;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Abstract
{
    public interface ICodeShareDal : IEntityRepository<CodeShare>
    {
        List<CodeShareDto> GetCodeSharesWithDetails();
        List<CodeShareDto> GetCodeSharesWithDetailsByUserId(int userId);
        CodeShareDto GetCodeShareWithDetailsById(int codeShareId);
        void UpdateViewCount(int codeShareId);
        void UpdateDownloadCount(int codeShareId);
    }
} 