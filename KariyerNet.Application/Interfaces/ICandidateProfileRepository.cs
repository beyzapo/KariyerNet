using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Interfaces
{
    public interface ICandidateProfileRepository
    {
        Task<CandidateProfile?> GetByUserIdAsync(Guid userId);
        Task<List<CandidateProfile>> GetByUserIdsAsync(IEnumerable<Guid> userIds);
        Task AddAsync(CandidateProfile profile);
        Task SaveChangesAsync();
    }
}
