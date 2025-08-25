using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnapLink.api.Infra;
using SnapLink.api.Application;
using SnapLink.api.Application.Services;
using SnapLink.api.Crosscutting.DTO;
using Moq;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.Api.Domain;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
namespace SnapLink.tests.Services
{
    public class PageServiceTest
    {

        private readonly Mock<IPageRepository> _pageRepository = new Mock<IPageRepository>();
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();
        private readonly PageService _pageService;

        public PageServiceTest()
        {
            _pageService = new PageService(_pageRepository.Object, _unitOfWork.Object);
        }

        [Fact(DisplayName = "Should return false when trying to create a page with an existing name")]
        public async Task CreatePage_WithExistingName_ShouldReturnFalse()
        {
            var Request = new CreatePageRequest
            {
                Name = "Test Page",
                IsPrivate = false,
                AccessCode = null
            };

            _pageRepository.Setup(repo => repo.ExistsByNameAsync(Request.Name))
               .ReturnsAsync(true);

            var result = await _pageService.CreatePageWithUniqueName(Request);

            Assert.False(result.Success);
            Assert.Equal("Page with this name already exists.", result.Message);
        }

        [Fact(DisplayName = "CreatePage_AsPrivateShouldHashPassword")]
        public async Task CreatePage_AsPrivateShouldHashPassword()
        {
            var request = new CreatePageRequest
            {
                Name = "Test Page",
                IsPrivate = true,
                AccessCode = "123456"
            };

            _pageRepository
                .Setup(repo => repo.ExistsByNameAsync(request.Name))
                .ReturnsAsync(false);

            _pageRepository
                .Setup(repo => repo.AddAsync(It.IsAny<Page>()));
            _unitOfWork
                .Setup(uow => uow.CommitAsync())
                .ReturnsAsync(true);    

            var result = await _pageService.CreatePageWithUniqueName(request);

            Assert.True(result.Success);
        }

        [Fact(DisplayName = "Should return false when trying to valide acess code with page name not existing")]
        public async Task ValideAcessCode_WithNameNotExisting_ShouldReturnFail()
        {
            var request = new ValideAcessCodeRequest
            {
                Name = "Test Page",
                AccessCode = "Teste@123"
            };

            _pageRepository
                .Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync((Page)null);

            var result = await _pageService.ValideAcessCode(request);
            Assert.False(result.Success);
        }

        [Fact(DisplayName = "Should return false when trying to validate access code with invalid access code")]
        public async Task ValideAcessCode_WithInvalidAccessCode_ShouldReturnFail()
        {
            var page = new Page
            {
                Name = "Test Page",
                IsPrivate = true,
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Cria hash com PasswordHasher<Page>
            var hasher = new PasswordHasher<Page>();
            page.AccessCode = hasher.HashPassword(page, "WrongPassword"); // senha armazenada é diferente

            var request = new ValideAcessCodeRequest
            {
                Name = page.Name,
                AccessCode = "Teste@123" // senha fornecida incorreta
            };

            _pageRepository
                .Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync(page);

            var result = await _pageService.ValideAcessCode(request);

            Assert.False(result.Success);
            Assert.Equal("Invalid access code.", result.Message);
        }

        [Fact(DisplayName = "Should return true when trying to validate access code")]
        public async Task ValideAcessCode_WithValidAccessCode_ShouldReturnTrue()
        {
            var page = new Page
            {
                Name = "Test Page",
                IsPrivate = true,
                Id = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            // Cria hash com PasswordHasher<Page>
            var hasher = new PasswordHasher<Page>();
            page.AccessCode = hasher.HashPassword(page, "Teste@123"); // senha armazenada correta

            var request = new ValideAcessCodeRequest
            {
                Name = page.Name,
                AccessCode = "Teste@123" // senha fornecida correta
            };

            _pageRepository
                .Setup(repo => repo.GetByNameAsync(request.Name))
                .ReturnsAsync(page);

            var result = await _pageService.ValideAcessCode(request);

            Assert.True(result.Success);
        }





    }
}
