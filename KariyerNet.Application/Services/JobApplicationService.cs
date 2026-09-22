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

        public JobApplicationService(
            IJobApplicationRepository jobApplicationRepository,
            IJobPostingRepository jobPostingRepository,
            ICandidateProfileRepository candidateProfileRepository,
            IUserRepository userRepository)
        {
            _jobApplicationRepository = jobApplicationRepository;
            _jobPostingRepository = jobPostingRepository;
            _candidateProfileRepository = candidateProfileRepository;
            _userRepository = userRepository;
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

            var existingApplication = await _jobApplicationRepository.GetByCandidateAndPostingAsync(candidateId, dto.JobPostingId);
            if (existingApplication != null)
            {
                throw new InvalidOperationException("Bu ilana zaten başvurdunuz.");
            }

            var application = new JobApplication
            {
                Id = Guid.NewGuid(),
                CandidateId = candidateId,
                JobPostingId = dto.JobPostingId,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };

            await _jobApplicationRepository.AddAsync(application);
            await _jobApplicationRepository.SaveChangesAsync();

            var candidate = await _userRepository.GetByIdAsync(candidateId);
            if (candidate == null)
            {
                throw new KeyNotFoundException("Aday bulunamadı.");
            }

            return new JobApplicationSummaryDto
            {
                Id = application.Id,
                CandidateId = application.CandidateId,
                JobPostingId = application.JobPostingId,
                JobPostingTitle = jobPosting.Title,
                CandidateFullName = candidate.FullName,
                CandidateEmail = candidate.Email,
                Status = application.Status.ToString(),
                AppliedAt = application.AppliedAt,
                HasCv = true
            };
        }

        public async Task<List<JobApplicationSummaryDto>> GetMineAsync(Guid candidateId)
        {
            var applications = await _jobApplicationRepository.GetByCandidateIdAsync(candidateId);
            return applications.Select(app => new JobApplicationSummaryDto
            {
                Id = app.Id,
                CandidateId = app.CandidateId,
                JobPostingId = app.JobPostingId,
                JobPostingTitle = app.JobPosting.Title,
                CandidateFullName = app.Candidate.FullName,
                CandidateEmail = app.Candidate.Email,
                Status = app.Status.ToString(),
                AppliedAt = app.AppliedAt
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
                    && !string.IsNullOrWhiteSpace(profile.CvFilePath)
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
                JobPostingTitle = jobPosting!.Title,
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
