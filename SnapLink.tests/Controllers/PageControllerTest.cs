using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.Tests.Configuration;

namespace SnapLink.tests.Controllers
{
    [Collection("App collection")]

    public class PageControllerTests
    {
        private readonly AppFixture<TestProgram> _fixture;

        public PageControllerTests(AppFixture <TestProgram> fixture)
        {
            _fixture = fixture;
        }

        public async Task Create_ShouldReturnOk_WhenRequestIsValid()
        {
            var request = new CreatePageRequest
            {
                IsPrivate = false,
                AccessCode = null,
                Name = "TestPage"
            };



            var response = await _fixture.Client.PostAsJsonAsync("/Page", request);

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadFromJsonAsync<dynamic>();
            Assert.Equal("Página criada com sucesso.", (string)content.data);
        }

        public async Task GetByName_ShouldReturnNotFound_WhenPageDoesNotExist()
        {
            var response = await _fixture.Client.GetAsync("/Page/by-name/UnknownPage");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

       
        public async Task AccessPage_ShouldReturnUnauthorized_WhenAccessCodeIsInvalid()
        {
            
            var request = new ValideAcessCodeRequest
            {
                
                Name = "PrivatePage",
                AccessCode = "wrong-code",
                
            };

            var response = await _fixture.Client.PostAsJsonAsync("/Page/access", request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
