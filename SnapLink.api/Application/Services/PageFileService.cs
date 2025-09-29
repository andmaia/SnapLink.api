using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Infra;
using Microsoft.Extensions.Configuration;
using SnapLink.api.Domain;
using SnapLink.api.Crosscutting.Enum;
using Microsoft.EntityFrameworkCore;

namespace SnapLink.api.Application.Services
{
    public class PageFileService : IPageFileService
    {
        private readonly IPageFileRepository _repository;
        private readonly IPageRepository _pageRepository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public PageFileService(
            IPageFileRepository repository,
            IPageRepository pageRepository,
            IConfiguration configuration,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _pageRepository = pageRepository;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> CreatePageFile(CreatePageFileRequest request)
        {
            var page = await _pageRepository.GetByNameAsync(request.PageName);
            if (page == null)
                return Result<bool>.Fail("Page not found.");

            if (request.Data == null || request.Data.Length == 0)
                return Result<bool>.Fail("File is empty.");

            var maxFileSize = _configuration.GetValue<long>("FileSettings:MaxFileSizeInBytes");
            if (request.Data.Length > maxFileSize)
                return Result<bool>.Fail($"File size exceeds the {maxFileSize / (1024 * 1024)}MB limit.");

            var pageFile = new PageFile(
                request.Data.FileName,
                request.ContentType,
                page.Id,
                (TimeToExpire)request.TimeToExpire
            );

            using (var ms = new MemoryStream())
            {
                await request.Data.CopyToAsync(ms);
                pageFile.AddFile(ms.ToArray());
            }

            _repository.AddAsync(pageFile);
            var committed = await _unitOfWork.CommitAsync();
            if (!committed)
                return Result<bool>.Fail("Failed to save the file.");

            return Result<bool>.Ok(true);
        }

        public async Task<Result<bool>> DeletePageFile(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null)
                return Result<bool>.Fail("File not found.");

            if (pageFile.VerifyIfExpire())
                return Result<bool>.Fail("Cannot delete an expired file.");

            pageFile.Disable();
            _repository.Update(pageFile);
            var committed = await _unitOfWork.CommitAsync();

            if (!committed)
                return Result<bool>.Fail("Failed to delete file.");

            return Result<bool>.Ok(true);
        }

        public async Task<Result<byte[]>> DowloadPageFile(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null || !pageFile.IsActive)
                return Result<byte[]>.Fail("File not found or inactive.");

            if (pageFile.VerifyIfExpire())
                return Result<byte[]>.Fail("File has expired.");

            return Result<byte[]>.Ok(pageFile.Data);
        }

        public async Task<Result<IEnumerable<PageFileResponse>>> GetAllByPageId(string PageId)
        {
            var files = await _repository.GetByPageIdAsync(PageId);
            return Result<IEnumerable<PageFileResponse>>.Ok(files);
        }

        public async Task<Result<PageFileResponse>> GetById(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null || !pageFile.IsActive)
                return Result<PageFileResponse>.Fail("File not found or inactive.");

            var response = new PageFileResponse
            {
                Id = pageFile.Id,
                FileName = pageFile.FileName,
                Size = pageFile.Size,
                ContentType = pageFile.ContentType,
                PageId = pageFile.PageId,
                CreatedAt = pageFile.CreatedAt,
                TimeToExpire = (int)pageFile.TimeToExpire,
                ExpiresAt = pageFile.ExpiresAt,
                IsActive = pageFile.IsActive,
                DownloadUrl = $"{pageFile.PageId}/{pageFile.Id}" 
            };

            return Result<PageFileResponse>.Ok(response);
        }

    }
}
