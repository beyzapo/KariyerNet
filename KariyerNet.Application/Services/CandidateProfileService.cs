using KariyerNet.Application.DTOs;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class CandidateProfileService
    {
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        public CandidateProfileService(ICandidateProfileRepository candidateProfileRepository)
        {
            _candidateProfileRepository = candidateProfileRepository;
        }

        public async Task<CandidateProfile> CreateOrUpdateAsync(Guid userId, CreateOrUpdateCandidateProfileDto dto)
        {
            var profile = await _candidateProfileRepository.GetByUserIdAsync(userId);
            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CvFilePath = dto.CvFilePath,
                    CvUploadedAt = DateTime.UtcNow
                };
                await _candidateProfileRepository.AddAsync(profile);
            }
            else
            {
                profile.CvFilePath = dto.CvFilePath;
                profile.CvUploadedAt = DateTime.UtcNow;
            }

            await _candidateProfileRepository.SaveChangesAsync();
            return profile;
        }

        public Task<CandidateProfile?> GetByUserIdAsync(Guid userId)
        {
            return _candidateProfileRepository.GetByUserIdAsync(userId);
        }
    }
}
