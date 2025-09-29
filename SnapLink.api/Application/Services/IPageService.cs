using SnapLink.api.Crosscutting;
using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;

namespace SnapLink.api.Application.Services
{
    public interface IPageService
    {
        Task<Result<bool>> CreatePageWithUniqueName(CreatePageRequest request);
        Task<Result<bool>> UpdatePrivacityPage(CreatePageRequest request);
        Task<Result<bool>> UpdatePage(CreatePageRequest request);
        Task<Result<PageResponse>> GetPageByName(string name);
        Task<Result<bool>> ValideAcessCode(ValideAcessCodeRequest request);



    }
}
