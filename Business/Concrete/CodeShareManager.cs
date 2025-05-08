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
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CodeShareManager : ICodeShareService
    {
        private readonly ICodeShareDal _codeShareDal;
        private readonly IHttpClientFactory _httpClientFactory;

        public CodeShareManager(ICodeShareDal codeShareDal, IHttpClientFactory httpClientFactory)
        {
            _codeShareDal = codeShareDal;
            _httpClientFactory = httpClientFactory;
        }

        public IResult Add(CodeShare codeShare)
        {
            codeShare.SharingDate = DateTime.Now;
            codeShare.ViewCount = 0;
            codeShare.DownloadCount = 0;
            _codeShareDal.Add(codeShare);
            return new SuccessResult(Messages.CodeShareAdded);
        }

        public IResult Delete(CodeShare codeShare)
        {
            _codeShareDal.Delete(codeShare);
            return new SuccessResult(Messages.CodeShareDeleted);
        }

        public IDataResult<List<CodeShareDto>> GetAll()
        {
            return new SuccessDataResult<List<CodeShareDto>>(_codeShareDal.GetCodeSharesWithDetails(), Messages.CodeSharesListed);
        }

        public IDataResult<List<CodeShareDto>> GetAllByUserId(int userId)
        {
            return new SuccessDataResult<List<CodeShareDto>>(_codeShareDal.GetCodeSharesWithDetailsByUserId(userId), Messages.CodeSharesListed);
        }

        public IDataResult<CodeShareDto> GetById(int codeShareId)
        {
            return new SuccessDataResult<CodeShareDto>(_codeShareDal.GetCodeShareWithDetailsById(codeShareId), Messages.CodeShareListed);
        }

        public IResult Update(CodeShare codeShare)
        {
            _codeShareDal.Update(codeShare);
            return new SuccessResult(Messages.CodeShareUpdated);
        }

        public IResult UpdateViewCount(int codeShareId)
        {
            _codeShareDal.UpdateViewCount(codeShareId);
            return new SuccessResult();
        }

        public IResult UpdateDownloadCount(int codeShareId)
        {
            _codeShareDal.UpdateDownloadCount(codeShareId);
            return new SuccessResult();
        }

        public async Task<IDataResult<string>> AnalyzeCodePurposeAsync(int codeShareId)
        {
            try
            {
                var codeShare = _codeShareDal.GetCodeShareWithDetailsById(codeShareId);
                if (codeShare == null)
                {
                    return new ErrorDataResult<string>("Kod paylaşımı bulunamadı.");
                }

                var client = _httpClientFactory.CreateClient();
                
                // Using a mock response for now. In production, you'd connect to an actual AI API
                // This can be replaced with an actual OpenAI or similar service call 
                var mockResponse = $"Bu kod {codeShare.Language} dilinde yazılmış ve {(codeShare.Tags?.Split(',').Length ?? 0)} farklı teknoloji/kütüphane kullanıyor. " +
                    $"Kodun amacı: {codeShare.Title} ile ilgili işlevleri gerçekleştirmek olup, temel olarak {codeShare.Description} işini yapmaktadır.";
                
                return new SuccessDataResult<string>(mockResponse);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<string>($"Kod analiz edilirken bir hata oluştu: {ex.Message}");
            }
        }

        public IDataResult<string> AnalyzeCodePurpose(int codeShareId)
        {
            return AnalyzeCodePurposeAsync(codeShareId).Result;
        }
    }
} 