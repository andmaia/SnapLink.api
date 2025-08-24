using SnapLink.Api.Domain;

namespace SnapLink.api.Infra
{
    public interface IPageRepository:IRepository<Page>
    {
        Task<bool> ExistsByNameAsync(string name);
        Task<Page?> GetByNameAsync(string name);


    }
}
