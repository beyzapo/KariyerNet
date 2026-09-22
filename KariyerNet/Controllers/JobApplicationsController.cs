using FluentValidation;
using KariyerNet.API.Extensions;
using KariyerNet.Application.DTOs;
using KariyerNet.Application.Services;
using KariyerNet.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace KariyerNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobApplicationsController : ControllerBase
    {
        private readonly JobApplicationService _jobApplicationService;
        private readonly IValidator<CreateJobApplicationDto> _createValidator;
        private readonly IValidator<UpdateJobApplicationStatusDto> _updateStatusValidator;
        private readonly IWebHostEnvironment _environment;

        public JobApplicationsController(
            JobApplicationService jobApplicationService,
            IValidator<CreateJobApplicationDto> createValidator,
            IValidator<UpdateJobApplicationStatusDto> updateStatusValidator,
            IWebHostEnvironment environment)
        {
            _jobApplicationService = jobApplicationService;
            _createValidator = createValidator;
            _updateStatusValidator = updateStatusValidator;
            _environment = environment;
        }

        [HttpPost]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Apply([FromBody] CreateJobApplicationDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }

            try
            {
                var application = await _jobApplicationService.ApplyAsync(User.GetUserId(), dto);
                return Ok(application);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMine()
        {
            var applications = await _jobApplicationService.GetMineAsync(User.GetUserId());
            return Ok(applications);
        }

        [HttpGet("posting/{jobPostingId}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetForPosting(Guid jobPostingId)
        {
            try
            {
                var applications = await _jobApplicationService.GetForPostingAsync(User.GetUserId(), jobPostingId);
                return Ok(applications);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "Bu ilana ait başvuruları görüntüleme yetkiniz yok." });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateJobApplicationStatusDto dto)
        {
            var validationResult = await _updateStatusValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }

            var application = await _jobApplicationService.UpdateStatusAsync(User.GetUserId(), id, dto);
            return Ok(application);
        }

        [HttpGet("{id}/cv")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> DownloadCv(Guid id)
        {
            try
            {
                var relativePath = await _jobApplicationService.GetCvFilePathForApplicationAsync(User.GetUserId(), id);
                var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "Uploads", "Cvs"));
                var fullPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, relativePath));

                var uploadsRootWithSeparator = uploadsRoot.EndsWith(Path.DirectorySeparatorChar)
                    ? uploadsRoot
                    : uploadsRoot + Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(uploadsRootWithSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Geçersiz dosya yolu." });
                }

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new { message = "CV dosyası sunucuda bulunamadı." });
                }

                var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = extension == ".pdf"
                    ? "application/pdf"
                    : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                var downloadFileName = $"cv{extension}";
                return PhysicalFile(fullPath, contentType, downloadFileName);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(403, new { message = "Bu CV'yi görüntüleme yetkiniz yok." });
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
