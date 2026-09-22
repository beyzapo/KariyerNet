using FluentValidation;
using KariyerNet.API.Extensions;
using KariyerNet.Application.DTOs;
using KariyerNet.Application.Services;
using KariyerNet.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KariyerNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostingsController : ControllerBase
    {
        private readonly JobPostingService _jobPostingService;
        private readonly IValidator<CreateJobPostingDto> _validator;

        public JobPostingsController(JobPostingService jobPostingService, IValidator<CreateJobPostingDto> validator)
        {
            _jobPostingService = jobPostingService;
            _validator = validator;
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> Create([FromBody] CreateJobPostingDto dto)
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }

            var jobPosting = await _jobPostingService.CreateAsync(User.GetUserId(), dto);
            return Ok(jobPosting);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var jobPostings = await _jobPostingService.GetAllAsync();
            return Ok(jobPostings);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetMine()
        {
            var jobPostings = await _jobPostingService.GetMineAsync(User.GetUserId());
            return Ok(jobPostings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var jobPosting = await _jobPostingService.GetByIdAsync(id);
            return Ok(jobPosting);
        }
    }
}
