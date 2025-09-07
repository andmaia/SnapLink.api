using Microsoft.EntityFrameworkCore;
using SnapLink.api.Domain;

namespace SnapLink.api.Infra
{
    public class PageFileRepository : IPageFileRepository
    {
        private readonly AppDbContext _context;

        public PageFileRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddAsync(PageFile entity)
        {
            _context.PageFiles.Add(entity);
        }

        public async Task<IEnumerable<PageFile>> GetAllAsync()
        {
            return await _context.PageFiles
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<PageFile?> GetByIdAsync(string id)
        {
            return await _context.PageFiles
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PageFileResponse>> GetByPageIdAsync(string pageId)
        {
            return await _context.PageFiles
                .AsNoTracking()
                .Where(pf => pf.PageId == pageId && pf.IsActive)
                .Select(pf => new PageFileResponse
                {
                    Id = pf.Id,
                    FileName = pf.FileName,
                    Size = pf.Size,
                    ContentType = pf.ContentType,
                    PageId = pf.PageId,
                    CreatedAt = pf.CreatedAt,
                    TimeToExpire = (int)pf.TimeToExpire,
                    ExpiresAt = pf.ExpiresAt,
                    IsActive = pf.IsActive,
                    DownloadUrl = $"{pf.PageId}/{pf.Id}"
                })
                .ToListAsync();
        }

                public async Task<byte[]?> GetDataByIdAsync(string id)
        {
            return await _context.PageFiles
                .Where(pf => pf.Id == id)
                .Select(pf => pf.Data)
                .FirstOrDefaultAsync();
        }

        public void Update(PageFile entity)
        {
            _context.PageFiles.Update(entity);
        }
    }
}
