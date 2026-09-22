using KariyerNet.Application.DTOs;
using KariyerNet.Application.Exceptions;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;

namespace KariyerNet.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<User> RegisterAsync(RegisterUserDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists.");
            }


            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();


            return user;
        }

        public async Task<string> LoginAsync(LoginUserDto dto)
        {
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new AuthenticationFailedException("Invalid email or password.");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }
    }
}
