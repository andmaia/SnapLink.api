using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Crosscutting;

namespace SnapLink.api.Application.Services
{
    public interface IPageFileService
    {

        Task<bool> CreatePageFile(CreatePageFileRequest request);
        Task<bool> DeletePageFile(string Id);
        Task<IEnumerable<PageFileResponse>> GetAllByPageId(string PageId);
        Task<Byte[]> DowloadPageFile(string url);
        Task<PageFileResponse> GetById(string Id);


    }
}
