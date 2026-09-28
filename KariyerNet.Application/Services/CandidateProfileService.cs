using KariyerNet.Application.DTOs;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class CandidateProfileService
    {
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IAgentClient _agentClient;

        public CandidateProfileService(
            ICandidateProfileRepository candidateProfileRepository,
            IAgentClient agentClient)
        {
            _candidateProfileRepository = candidateProfileRepository;
            _agentClient = agentClient;
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

        public async Task<CvAnalysisDto> AnalyzeCvAsync(Guid userId, string contentRootPath)
        {
            var profile = await _candidateProfileRepository.GetByUserIdAsync(userId);
            if (profile == null || string.IsNullOrWhiteSpace(profile.CvFilePath))
            {
                throw new InvalidOperationException("Analiz için önce CV yüklemelisiniz.");
            }

            var uploadsRoot = Path.GetFullPath(Path.Combine(contentRootPath, "Uploads", "Cvs"));
            var fullPath = Path.GetFullPath(Path.Combine(contentRootPath, profile.CvFilePath));

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Geçersiz dosya yolu.");
            }

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("CV dosyası sunucuda bulunamadı.");
            }

            var result = await _agentClient.AnalyzeCvAsync(fullPath);

            return new CvAnalysisDto
            {
                Skills = result.Skills,
                ExperienceLevel = result.ExperienceLevel,
                Summary = result.Summary,
                Suggestions = result.Suggestions
            };
        }
    }
}