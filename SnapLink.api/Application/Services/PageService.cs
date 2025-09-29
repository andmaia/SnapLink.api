using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Infra;
using SnapLink.Api.Crosscutting.Events;
using SnapLink.Api.Domain;

namespace SnapLink.api.Application.Services
{
    public class PageService : IPageService
    {
        private readonly IPageRepository _pageRepository;
        private readonly IUnitOfWork _unitOfWork;
        public PageService(IPageRepository pageRepository, IUnitOfWork unitOfWork)
        {
            _pageRepository = pageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreatePageWithUniqueName(CreatePageRequest request)
        {
            if (await _pageRepository.ExistsByNameAsync(request.Name))
            {
                MessageService.AddMessage("P�gina com essa nome j� existe.");
                return false;
            }

            Page page = new Page
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Id = Guid.NewGuid().ToString()
            };

            if (request.IsPrivate)
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
            var result = await _unitOfWork.CommitAsync();
            if (result)
            {
                return true;
            }
            MessageService.AddMessage("Falha ao criar p�gina.");
            return false;
        }

        public async Task<PageResponse?> GetPageByName(string name)
        {
            var result = await _pageRepository.GetByNameAsync(name);
            if (result == null)
            {
                MessageService.AddMessage("P�gina n�o encontrada.");
                return null;
            }
            var pageResponse = new PageResponse
            {
                Id = result.Id,
                Name = result.Name,
                IsPrivate = result.IsPrivate,
                CreatedAt = result.CreatedAt,
                IsActive = result.IsActive
            };
            return pageResponse;
        }

        public Task<bool> UpdatePage(CreatePageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdatePrivacityPage(CreatePageRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ValideAcessCode(ValideAcessCodeRequest request)
        {

            var page = await _pageRepository.GetByNameAsync(request.Name);
            if (page == null)
            {
                MessageService.AddMessage("P�gina n�o encontrada.");
                return false;
            }
               
            var isValid = page.VerifyPassword(request.AccessCode);
            if (!isValid)
            {
                MessageService.AddMessage("C�digo de acesso inv�lido.");
                return false;
            }
            return true;
        }

    }
}