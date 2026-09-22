using FluentValidation;
using KariyerNet.Application.DTOs;
using KariyerNet.Application.Services;
using KariyerNet.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace KariyerNet.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IValidator<RegisterUserDto> _registerValidator;
        private readonly IValidator<LoginUserDto> _loginValidator;

        public AuthController(UserService userService, IValidator<RegisterUserDto> registerValidator, IValidator<LoginUserDto> loginValidator)
        {
            _userService = userService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }

            try
            {
                var user = await _userService.RegisterAsync(dto);
                return Ok(new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.Role,
                    user.CreatedAt
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return ValidationProblem(new ValidationProblemDetails(validationResult.ToErrorDictionary()));
            }

            var token = await _userService.LoginAsync(dto);
            return Ok(new { token });
        }
    }
}
