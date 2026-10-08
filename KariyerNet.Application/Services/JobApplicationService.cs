using KariyerNet.Application.DTOs;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class JobApplicationService
    {
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IJobPostingRepository _jobPostingRepository;
        private readonly ICandidateProfileRepository _candidateProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAgentClient _agentClient;
        private readonly IEvaluationQueue _evaluationQueue;

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IJobPostingRepository jobPostingRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IUserRepository userRepository,
            IAgentClient agentClient,
            IEvaluationQueue evaluationQueue)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPostingRepository = jobPostingRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _userRepository = userRepository;
            _agentClient = agentClient;
            _evaluationQueue = evaluationQueue;
        }
        public async Task<JobApplicationSummaryDto> ApplyAsync(Guid candidateId, CreateJobApplicationDto dto)
        {
            var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(candidateId);
            if (candidateProfile == null || string.IsNullOrWhiteSpace(candidateProfile.CvFilePath))
            {
                throw new InvalidOperationException("Başvuru yapabilmek için önce CV yüklemelisiniz.");
            }

            var jobPosting = await _jobPostingRepository.GetByIdAsync(dto.JobPostingId);
            if (jobPosting == null)
            {
                throw new KeyNotFoundException("İlan bulunamadı.");
            }

            var application = await _jobApplicationRepository.GetByCandidateAndPostingAsync(candidateId, dto.JobPostingId);

            if (application == null)
            {
                application = new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                JobPostingId = dto.JobPostingId,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
         };

            await _jobApplicationRepository.AddAsync(application);
             }
            else
            {
                switch (application.Status)
                {
                case ApplicationStatus.Pending:
                    throw new InvalidOperationException("Başvurunuz zaten değerlendirme aşamasında.");
                case ApplicationStatus.Accepted:
                    throw new InvalidOperationException("Bu ilana başvurunuz zaten kabul edildi.");
                case ApplicationStatus.Rejected:
                // Reddedilen aday tekrar başvurabilir: aynı kaydı sıfırla
                    application.Status = ApplicationStatus.Pending;
                    application.AppliedAt = DateTime.UtcNow;
                    application.MatchScore = null;
                    application.AiExplanation = null;
                    application.CandidateAiExplanation = null;
                    application.AiEvaluatedAt = null;
                    break;
                }
            }

            await _jobApplicationRepository.SaveChangesAsync();

    // AI değerlendirmesi arka planda yapılır, aday beklemez
            await _evaluationQueue.EnqueueAsync(application.Id);

            var candidate = await _userRepository.GetByIdAsync(candidateId);

             return new JobApplicationSummaryDto
            {
                Id = application.Id,
                CandidateId = application.CandidateId,
                JobPostingId = application.JobPostingId,
                JobPostingTitle = jobPosting.Title,
                CandidateFullName = candidate?.FullName ?? string.Empty,
                CandidateEmail = candidate?.Email ?? string.Empty,
                Status = application.Status.ToString(),
                AppliedAt = application.AppliedAt,
                HasCv = true
            };
        }
        
        public async Task EvaluateAsync(Guid applicationId, string contentRootPath)
        {
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return;
            }

            var jobPosting = await _jobPostingRepository.GetByIdAsync(application.JobPostingId);
            var profile = await _candidateProfileRepository.GetByUserIdAsync(application.CandidateId);
            if (jobPosting == null || profile == null || string.IsNullOrWhiteSpace(profile.CvFilePath))
            {
                return;
            }

            var uploadsRoot = Path.GetFullPath(Path.Combine(contentRootPath, "Uploads", "Cvs"));
            var fullPath = Path.GetFullPath(Path.Combine(contentRootPath, profile.CvFilePath));
            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            {
                return;
            }

            var matchResult = await _agentClient.MatchCandidateToJobAsync(
                fullPath, jobPosting.Title, jobPosting.Description, jobPosting.Requirements);

            application.MatchScore = matchResult.CompatibilityScore;
            application.AiExplanation = matchResult.Explanation;
            application.CandidateAiExplanation = matchResult.CandidateExplanation;
            application.AiEvaluatedAt = DateTime.UtcNow;

            await _jobApplicationRepository.SaveChangesAsync();
        }

        public async Task<List<JobApplicationSummaryDto>> GetMineAsync(Guid candidateId)
        {
            var applications = await _jobApplicationRepository.GetByCandidateIdAsync(candidateId);
            var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(candidateId);
            var hasCv = candidateProfile != null && !string.IsNullOrWhiteSpace(candidateProfile.CvFilePath);

            return applications.Select(app => new JobApplicationSummaryDto
            {
                Id = app.Id,
                CandidateId = app.CandidateId,
                JobPostingId = app.JobPostingId,
                JobPostingTitle = app.JobPosting.Title,
                CandidateFullName = app.Candidate.FullName,
                CandidateEmail = app.Candidate.Email,
                Status = app.Status.ToString(),
                AppliedAt = app.AppliedAt,
                HasCv = hasCv,
                MatchScore = app.MatchScore,
                AiExplanation = app.CandidateAiExplanation
            }).ToList();
        }

        public async Task<List<JobApplicationSummaryDto>> GetForPostingAsync(Guid employerId, Guid jobPostingId)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(jobPostingId);
            if (jobPosting == null)
            {
                throw new KeyNotFoundException("İlan bulunamadı.");
            }

            if (jobPosting.EmployerId != employerId)
            {
                throw new UnauthorizedAccessException("Bu ilana ait başvuruları görüntüleme yetkiniz yok.");
            }

            var applications = await _jobApplicationRepository.GetByJobPostingIdAsync(jobPostingId);
            var candidateIds = applications.Select(a => a.CandidateId).Distinct().ToList();
            var profiles = await _candidateProfileRepository.GetByUserIdsAsync(candidateIds);
            var profilesByUserId = profiles.ToDictionary(p => p.UserId);

            return applications.Select(app => new JobApplicationSummaryDto
            {
                Id = app.Id,
                CandidateId = app.CandidateId,
                JobPostingId = app.JobPostingId,
                JobPostingTitle = app.JobPosting.Title,
                CandidateFullName = app.Candidate.FullName,
                CandidateEmail = app.Candidate.Email,
                Status = app.Status.ToString(),
                AppliedAt = app.AppliedAt,
                HasCv = profilesByUserId.TryGetValue(app.CandidateId, out var profile)
                    && !string.IsNullOrWhiteSpace(profile.CvFilePath),
                MatchScore = app.MatchScore,
                AiExplanation = app.AiExplanation
            }).ToList();
        }

        public async Task<JobApplicationSummaryDto> UpdateStatusAsync(Guid employerId, Guid applicationId, UpdateJobApplicationStatusDto dto)
        {
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                throw new KeyNotFoundException("Başvuru bulunamadı.");
            }

            var jobPosting = await _jobPostingRepository.GetByIdAsync(application.JobPostingId);
            if (jobPosting == null || jobPosting.EmployerId != employerId)
            {
                throw new UnauthorizedAccessException("Bu başvuruyu güncelleme yetkiniz yok.");
            }

            application.Status = dto.Status;
            await _jobApplicationRepository.SaveChangesAsync();

            var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(application.CandidateId);
            return new JobApplicationSummaryDto
            {
                Id = application.Id,
                CandidateId = application.CandidateId,
                JobPostingId = application.JobPostingId,
                JobPostingTitle = jobPosting.Title,
                CandidateFullName = application.Candidate.FullName,
                CandidateEmail = application.Candidate.Email,
                Status = application.Status.ToString(),
                AppliedAt = application.AppliedAt,
                HasCv = candidateProfile != null && !string.IsNullOrWhiteSpace(candidateProfile.CvFilePath)
            };
        }

        public async Task<string> GetCvFilePathForApplicationAsync(Guid employerId, Guid applicationId)
        {
            var application = await _jobApplicationRepository.GetByIdAsync(applicationId);
            if (application == null)
            {
                throw new KeyNotFoundException("Başvuru bulunamadı.");
            }

            var jobPosting = await _jobPostingRepository.GetByIdAsync(application.JobPostingId);
            if (jobPosting == null || jobPosting.EmployerId != employerId)
            {
                throw new UnauthorizedAccessException("Bu başvuruyu görüntüleme yetkiniz yok.");
            }

            var candidateProfile = await _candidateProfileRepository.GetByUserIdAsync(application.CandidateId);
            if (candidateProfile == null || string.IsNullOrWhiteSpace(candidateProfile.CvFilePath))
            {
                throw new FileNotFoundException("Bu adayın yüklenmiş bir CV'si yok.");
            }

            return candidateProfile.CvFilePath;
        }
    }
}