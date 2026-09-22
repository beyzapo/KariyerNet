using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;
using KariyerNet.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KariyerNet.Infrastructure.Repositories
{
    public class CandidateProfileRepository : ICandidateProfileRepository
    {
        private readonly AppDbContext _context;
        public CandidateProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CandidateProfile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.CandidateProfiles.FirstOrDefaultAsync(cp => cp.UserId == userId);
        }

        public async Task<List<CandidateProfile>> GetByUserIdsAsync(IEnumerable<Guid> userIds)
        {
            return await _context.CandidateProfiles
                .Where(cp => userIds.Contains(cp.UserId))
                .ToListAsync();
        }

        public async Task AddAsync(CandidateProfile profile)
        {
            await _context.CandidateProfiles.AddAsync(profile);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new InvalidOperationException("Bu kullanıcı için aday profili zaten mevcut.", ex);
            }
        }
    }
}
