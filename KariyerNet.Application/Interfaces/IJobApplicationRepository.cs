using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(Guid id);
        Task<JobApplication?> GetByCandidateAndPostingAsync(Guid candidateId, Guid jobPostingId);
        Task<List<JobApplication>> GetByCandidateIdAsync(Guid candidateId);
        Task<List<JobApplication>> GetByJobPostingIdAsync(Guid jobPostingId);
        Task AddAsync(JobApplication application);
        Task SaveChangesAsync();
    }
}
