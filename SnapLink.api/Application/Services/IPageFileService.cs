using SnapLink.api.Crosscutting.DTO.Request;
using SnapLink.api.Crosscutting.DTO.Response;
using SnapLink.api.Crosscutting;

namespace SnapLink.api.Application.Services
{
    public interface IPageFileService
    {

        Task<Result<bool>> CreatePageFile(CreatePageFileRequest request);
        Task<Result<bool>> DeletePageFile(string Id);
        Task<Result<IEnumerable<PageFileResponse>>> GetAllByPageId(string PageId);
        Task<Result<Byte[]>> DowloadPageFile(string url);
        Task<Result<PageFileResponse>> GetById(string Id);



    }
}
