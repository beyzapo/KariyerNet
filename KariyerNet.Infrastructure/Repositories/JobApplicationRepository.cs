using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;
using KariyerNet.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace KariyerNet.Infrastructure.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly AppDbContext _context;
        public JobApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(Guid id)
        {
            return await _context.JobApplications
                .Include(ja => ja.Candidate)
                .Include(ja => ja.JobPosting)
                .FirstOrDefaultAsync(ja => ja.Id == id);
        }

        public async Task<JobApplication?> GetByCandidateAndPostingAsync(Guid candidateId, Guid jobPostingId)
        {
            return await _context.JobApplications
                .Include(ja => ja.Candidate)
                .Include(ja => ja.JobPosting)
                .FirstOrDefaultAsync(ja => ja.CandidateId == candidateId && ja.JobPostingId == jobPostingId);
        }

        public async Task<List<JobApplication>> GetByCandidateIdAsync(Guid candidateId)
        {
            return await _context.JobApplications
                .Include(ja => ja.Candidate)
                .Include(ja => ja.JobPosting)
                .Where(ja => ja.CandidateId == candidateId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetByJobPostingIdAsync(Guid jobPostingId)
        {
            return await _context.JobApplications
                .Include(ja => ja.Candidate)
                .Include(ja => ja.JobPosting)
                .Where(ja => ja.JobPostingId == jobPostingId)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
            {
                throw new InvalidOperationException("Bu ilana zaten başvurdunuz.", ex);
            }
        }
    }
}
