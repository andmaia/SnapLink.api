namespace SnapLink.api.Infra
{
    public interface IUnitOfWork : IDisposable
    {
        Task<bool> CommitAsync();
        void Rollback();
    }
}
