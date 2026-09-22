using FluentValidation;
using KariyerNet.API.Extensions;
using KariyerNet.Application.DTOs;
using KariyerNet.Application.Services;
using KariyerNet.Application.Validators;
using KariyerNet.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace KariyerNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Candidate")]
    public class CandidateProfileController : ControllerBase
    {
        private readonly CandidateProfileService _candidateProfileService;
        private readonly IValidator<CreateOrUpdateCandidateProfileDto> _validator;
        private readonly IWebHostEnvironment _environment;

        public CandidateProfileController(
            CandidateProfileService candidateProfileService,
            IValidator<CreateOrUpdateCandidateProfileDto> validator,
            IWebHostEnvironment environment)
        {
            _candidateProfileService = candidateProfileService;
            _validator = validator;
            _environment = environment;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateOrUpdate([FromForm] IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest(new { message = "Lütfen bir CV dosyası seçin." });
            }

            var allowedExtensions = new[] { ".pdf", ".docx" };
            var extension = Path.GetExtension(cvFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Sadece PDF veya DOCX dosyaları kabul edilir." });
            }

            const long maxFileSize = 5 * 1024 * 1024;
            if (cvFile.Length > maxFileSize)
            {
                return BadRequest(new { message = "Dosya boyutu 5 MB'ı geçemez." });
            }

            using (var stream = cvFile.OpenReadStream())
            {
                var header = new byte[4];
                var bytesRead = await stream.ReadAsync(header, 0, header.Length);

                var isValidPdf = extension == ".pdf" && bytesRead >= 4
                    && header[0] == 0x25 && header[1] == 0x50
                    && header[2] == 0x44 && header[3] == 0x46;
                var isValidDocx = extension == ".docx" && bytesRead >= 4
                    && header[0] == 0x50 && header[1] == 0x4B
                    && (header[2] == 0x03 || header[2] == 0x05 || header[2] == 0x07);

                if (!isValidPdf && !isValidDocx)
                {
                    return BadRequest(new { message = "Dosya içeriği, uzantısıyla uyuşmuyor veya bozuk." });
                }
            }

            var existingProfile = await _candidateProfileService.GetByUserIdAsync(User.GetUserId());
            var oldCvFilePath = existingProfile?.CvFilePath;

            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Cvs");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            var relativePath = Path.Combine("Uploads", "Cvs", uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await cvFile.CopyToAsync(stream);
            }

            CandidateProfile profile;
            try
            {
                var dto = new CreateOrUpdateCandidateProfileDto { CvFilePath = relativePath };
                profile = await _candidateProfileService.CreateOrUpdateAsync(User.GetUserId(), dto);
            }
            catch
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                throw;
            }

            if (!string.IsNullOrWhiteSpace(oldCvFilePath) && oldCvFilePath != relativePath)
            {
                var oldFullPath = Path.Combine(_environment.ContentRootPath, oldCvFilePath);
                if (System.IO.File.Exists(oldFullPath))
                {
                    System.IO.File.Delete(oldFullPath);
                }
            }

            return Ok(new { profile.Id, profile.UserId, profile.CvFilePath, profile.CvUploadedAt });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMine()
        {
            var profile = await _candidateProfileService.GetByUserIdAsync(User.GetUserId());
            if (profile == null)
            {
                return NotFound(new { message = "Henüz bir aday profili oluşturulmamış." });
            }

            return Ok(new { profile.Id, profile.UserId, profile.CvFilePath, profile.CvUploadedAt });
        }
    }
}
