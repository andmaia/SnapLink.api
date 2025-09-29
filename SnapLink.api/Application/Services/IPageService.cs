using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;

namespace SnapLink.api.Application.Services
{
    public interface IPageService
    {
        Task<bool> CreatePageWithUniqueName(CreatePageRequest request);
        Task<bool> UpdatePrivacityPage(CreatePageRequest request);
        Task<bool> UpdatePage(CreatePageRequest request);
        Task<PageResponse> GetPageByName(string name);
        Task<bool> ValideAcessCode(ValideAcessCodeRequest request);

    }
}
