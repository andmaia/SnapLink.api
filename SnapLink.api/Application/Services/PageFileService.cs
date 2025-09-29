using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Infra;
using Microsoft.Extensions.Configuration;
using SnapLink.api.Domain;
using SnapLink.api.Crosscutting.Enum;
using Microsoft.EntityFrameworkCore;
using SnapLink.Api.Crosscutting.Events;

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

        public async Task<bool> CreatePageFile(CreatePageFileRequest request)
        {
            var page = await _pageRepository.GetByNameAsync(request.PageName);
            if (page == null)
            {
                MessageService.AddMessage("Página não encontrada");
                return false;
            }

            if (request.Data == null || request.Data.Length == 0)
            {
                MessageService.AddMessage("Arquivo está vazio");
                return false;
            }

            var maxFileSize = _configuration.GetValue<long>("FileSettings:MaxFileSizeInBytes");
            if (request.Data.Length > maxFileSize)
            {
                MessageService.AddMessage($"O arquivo excede o limite de {maxFileSize / (1024 * 1024)}MB.");
                return false;
            }

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
            {
                MessageService.AddMessage("Falha ao salvar arquivo");
                return false;
            }

            return true;
        }

        public async Task<bool> DeletePageFile(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null)
            {
                MessageService.AddMessage("Arquivo não encontrado");
                return false;
            }

            if (pageFile.VerifyIfExpire())
            {
                MessageService.AddMessage("Não é possível excluir um arquivo expirado");
                return false;
            }

            pageFile.Disable();
            _repository.Update(pageFile);
            var committed = await _unitOfWork.CommitAsync();

            if (!committed)
            {
                MessageService.AddMessage("Falha ao excluir arquivo");
                return false;
            }

            return true;
        }

        public async Task<byte[]> DowloadPageFile(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null || !pageFile.IsActive)
            {
                MessageService.AddMessage("Arquivo não encontrado ou inativo");
                return Array.Empty<byte>();
            }

            if (pageFile.VerifyIfExpire())
            {
                MessageService.AddMessage("Arquivo expirado");
                return Array.Empty<byte>();
            }

            return pageFile.Data;
        }

        public async Task<IEnumerable<PageFileResponse>> GetAllByPageId(string PageId)
        {
            var files = await _repository.GetByPageIdAsync(PageId);
            return files;
        }

        public async Task<PageFileResponse?> GetById(string id)
        {
            var pageFile = await _repository.GetByIdAsync(id);
            if (pageFile == null || !pageFile.IsActive)
            {
                MessageService.AddMessage("Arquivo não encontrado ou inativo");
                return null;
            }

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

            return response;
        }
    }
}
