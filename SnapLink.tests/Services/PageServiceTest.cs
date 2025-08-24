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
               
            var result = await _pageService.CreatePageWithUniqueName(request);

            Assert.True(result.Success);
        }


    }
}
