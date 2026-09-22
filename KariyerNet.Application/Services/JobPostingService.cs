using KariyerNet.Application.DTOs;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class JobPostingService
    {
        private readonly IJobPostingRepository _jobPostingRepository;
        public JobPostingService(IJobPostingRepository jobPostingRepository)
        {
            _jobPostingRepository = jobPostingRepository;
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
    }
}
