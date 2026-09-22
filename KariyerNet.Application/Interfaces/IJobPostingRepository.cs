using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Interfaces
{
    public interface IJobPostingRepository
    {
        Task<JobPosting?> GetByIdAsync(Guid id);
        Task<List<JobPosting>> GetAllAsync();
        Task<List<JobPosting>> GetByEmployerIdAsync(Guid employerId);
        Task AddAsync(JobPosting jobPosting);
        Task SaveChangesAsync();
    }
}
