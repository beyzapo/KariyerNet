using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.DTOs
{
    public class RegisterUserDto
    {
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public UserRole Role { get; set; }
    }
}
