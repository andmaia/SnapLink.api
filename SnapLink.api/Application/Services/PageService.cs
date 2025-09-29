using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Infra;
using SnapLink.Api.Domain;

namespace SnapLink.api.Application.Services
{   public class PageService : IPageService
    {
        private readonly IPageRepository _pageRepository;
        private readonly IUnitOfWork _unitOfWork;   
        public PageService(IPageRepository pageRepository,IUnitOfWork unitOfWork)
        {
            _pageRepository = pageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> CreatePageWithUniqueName(CreatePageRequest request)
        {
            if (await _pageRepository.ExistsByNameAsync(request.Name))
            {
                return Result<bool>.Fail("Page with this name already exists.");
            }

            Page page = new Page
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Id = Guid.NewGuid().ToString()
            };

            if(request.IsPrivate)
            {
                page.IsPrivate = true;
                page.AccessCode = request.AccessCode;
                page.HashPassword();
            }
            else
            {
                page.IsPrivate = false;
                page.AccessCode = null;
            }

            _pageRepository.AddAsync(page);
            var result =await _unitOfWork.CommitAsync();
            if (result)
            {
                return Result<bool>.Ok(true);
            }
            return Result<bool>.Fail("Failed to create page.");
        }

        public async Task<Result<PageResponse>> GetPageByName(string name)
        {
            var result = await _pageRepository.GetByNameAsync(name);
            if (result == null)
            {
                return Result<PageResponse>.Fail("Page not found.");
            }
            var pageResponse = new PageResponse
            {
                Id = result.Id,
                Name = result.Name,
                IsPrivate = result.IsPrivate,
                CreatedAt = result.CreatedAt,
                IsActive = result.IsActive
            };
            return Result<PageResponse>.Ok(pageResponse);
        }

        public Task<Result<bool>> UpdatePage(CreatePageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<Result<bool>> UpdatePrivacityPage(CreatePageRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Result<bool>> ValideAcessCode(ValideAcessCodeRequest request)
        {
           
            var page = await _pageRepository.GetByNameAsync(request.Name);
            if (page == null)
                return Result<bool>.Fail("Page not found.");

            var isValid = page.VerifyPassword(request.AccessCode);
            if (!isValid)
                return Result<bool>.Fail("Invalid access code.");

            return Result<bool>.Ok(true);
        }

    }
}
