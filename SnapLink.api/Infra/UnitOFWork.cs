
using Microsoft.EntityFrameworkCore;

namespace SnapLink.api.Infra
{
    public class UnitOFWork : IUnitOfWork
    {

        private readonly AppDbContext _context;

        public UnitOFWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Rollback()
        {
            foreach (var entry in _context.ChangeTracker.Entries()
                      .Where(e => e.State != EntityState.Unchanged))
            {
                entry.State = EntityState.Unchanged;
            }
        }

    }
}
