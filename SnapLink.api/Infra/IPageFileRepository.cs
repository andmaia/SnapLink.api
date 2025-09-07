using SnapLink.api.Domain;

namespace SnapLink.api.Infra
{
    public interface IPageFileRepository:IRepository<PageFile>
    {
        Task<IEnumerable<PageFileResponse>> GetByPageIdAsync(string pageId);
        Task<byte[]?> GetDataByIdAsync(string id);
    }
}
