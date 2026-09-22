using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;
using KariyerNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KariyerNet.Infrastructure.Repositories
{
    public class JobPostingRepository : IJobPostingRepository
    {
        private readonly AppDbContext _context;
        public JobPostingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobPosting?> GetByIdAsync(Guid id)
        {
            return await _context.JobPostings.FirstOrDefaultAsync(jp => jp.Id == id);
        }

        public async Task<List<JobPosting>> GetAllAsync()
        {
            return await _context.JobPostings.OrderByDescending(jp => jp.CreatedAt).ToListAsync();
        }

        public async Task<List<JobPosting>> GetByEmployerIdAsync(Guid employerId)
        {
            return await _context.JobPostings
                .Where(jp => jp.EmployerId == employerId)
                .OrderByDescending(jp => jp.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(JobPosting jobPosting)
        {
            await _context.JobPostings.AddAsync(jobPosting);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
