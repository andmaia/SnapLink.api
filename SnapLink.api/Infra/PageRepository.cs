using Microsoft.EntityFrameworkCore;
using SnapLink.api.Infra.SnapLink.Api.Infra;
using SnapLink.Api.Domain;

namespace SnapLink.api.Infra
{
    public class PageRepository : IPageRepository
    {
        private readonly AppDbContext _context;

        public PageRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddAsync(Page entity)
        {
            _context.Pages.Add(entity);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Pages
                .AnyAsync(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IEnumerable<Page>> GetAllAsync()
        {
            return await _context.Pages
                .AsNoTracking()
                .ToListAsync(); 
        }

        public async Task<Page?> GetByIdAsync(Guid id)
        {
           return await _context.Pages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Page?> GetByNameAsync(string name)
        {
            return await _context.Pages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void Update(Page entity)
        {
            _context.Pages.Update(entity);

        }
    }
}
