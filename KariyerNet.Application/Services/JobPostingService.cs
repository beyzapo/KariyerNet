using KariyerNet.Application.DTOs;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class JobPostingService
    {
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IAgentClient _agentClient;

        public JobPostingService(
            IJobPostingRepository jobPostingRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IJobApplicationRepository jobApplicationRepository,
            IAgentClient agentClient)
        {
            _jobPostingRepository = jobPostingRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _agentClient = agentClient;
        }

        public async Task<JobPosting> CreateAsync(Guid employerId, CreateJobPostingDto dto)
        {
            var jobPosting = new JobPosting
            {
                Id = Guid.NewGuid(),
                EmployerId = employerId,
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                CreatedAt = DateTime.UtcNow
            };

            await _jobPostingRepository.AddAsync(jobPosting);
            await _jobPostingRepository.SaveChangesAsync();
            return jobPosting;
        }

        public Task<List<JobPosting>> GetAllAsync()
        {
            return _jobPostingRepository.GetAllAsync();
        }

        public Task<List<JobPosting>> GetMineAsync(Guid employerId)
        {
            return _jobPostingRepository.GetByEmployerIdAsync(employerId);
        }

        public async Task<JobPosting> GetByIdAsync(Guid id)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(id);
            if (jobPosting == null)
            {
                throw new KeyNotFoundException("İlan bulunamadı.");
            }

            return jobPosting;
        }

        public async Task<List<JobRecommendationDto>> GetRecommendedAsync(Guid candidateId, string contentRootPath)
        {
            var profile = await _candidateProfileRepository.GetByUserIdAsync(candidateId);
            if (profile == null || string.IsNullOrWhiteSpace(profile.CvFilePath))
            {
                throw new InvalidOperationException("Öneri alabilmek için önce CV yüklemelisiniz.");
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

            var allJobs = await _jobPostingRepository.GetAllAsync();

            // Adayın zaten başvurduğu ilanları öneri dışında bırak
            var myApplications = await _jobApplicationRepository.GetByCandidateIdAsync(candidateId);
            var appliedJobIds = myApplications.Select(a => a.JobPostingId).ToHashSet();

            var availableJobs = allJobs
                .Where(j => !appliedJobIds.Contains(j.Id))
                .ToList();

            if (availableJobs.Count == 0)
            {
                return new List<JobRecommendationDto>();
            }

            // Prompt boyutunu kontrol altında tutmak için en yeni 50 ilan ile sınırla
            var jobsToEvaluate = availableJobs
                .OrderByDescending(j => j.CreatedAt)
                .Take(50)
                .ToList();

            var jobInputs = jobsToEvaluate.Select(j => new JobInfoInput
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Requirements = j.Requirements
            }).ToList();

            var recommendations = await _agentClient.RecommendJobsAsync(fullPath, jobInputs);
            var jobsById = jobsToEvaluate.ToDictionary(j => j.Id);

            return recommendations
                .Where(r => jobsById.ContainsKey(r.JobId))
                .Select(r => new JobRecommendationDto
                {
                    JobPostingId = r.JobId,
                    Title = jobsById[r.JobId].Title,
                    Description = jobsById[r.JobId].Description,
                    Score = r.Score,
                    Reason = r.Reason
                })
                .OrderByDescending(r => r.Score)
                .ToList();
        }
    }
}