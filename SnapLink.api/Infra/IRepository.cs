namespace SnapLink.api.Infra
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        void AddAsync(T entity);
        void Update(T entity);
    }
}
